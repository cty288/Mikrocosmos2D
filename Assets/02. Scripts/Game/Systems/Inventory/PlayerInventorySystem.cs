using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public struct OnBackpackItemRemoved {
        public NetworkIdentity Identity;
        public string PrefabName;
        public int CurrentCount;
        public GameObject NextObject;
        public GameObject OldObject;
    }

    [Serializable]
    public class BackpackSlot {

        public string PrefabName;
        public string SpriteName;
        public List<GameObject> StackedObjects = new List<GameObject>();
        public int ClientSlotCount;
        public int Count
        {
            get {
                return StackedObjects.Count;
            }
        }
    }

    public struct OnItemDropped {
        public NetworkIdentity Identity;
        public string PrefabName;
        public GameObject DroppedObject;
        public GameObject NextHookingObject;
        public bool DroppedCurrentSlot;
    }

    public struct OnClientInventoryUpdate {
        public List<BackpackSlot> AllSlots;
        public NetworkIdentity Owner;
        public int SelectedIndex;
    }

    public struct OnSwitchItemSlot
    {
        public NetworkIdentity Identity;
        public string PrefabName;
        public int SlotIndex;
        public GameObject SwitchedGameObject;
        public bool SameSlot;
    }


    public struct OnStartInventoryInit {
        public int InventoryCapacity;
        public int InitialBackPackCapacity;
    }
    public interface IPlayerInventorySystem : ISystem {
        void ServerHookToBackpack(string name, GameObject gameObject);

        void ServerAddToBackpack(string name, GameObject gameObject);
        void ServerRemoveFromCurrentBackpack();
        void ServerRemoveFromBackpack(string name);

        void ServerRemoveFromBackpack(GameObject obj);
        void ServerDropFromBackpack(string name);

        int GetSlotIndexFromItemName(string name);

        void ServerSwitchSlot(int index);
        int GetSlotCount();
        int GetCurrentSlot();

        bool ServerCheckCanAddToBackpack(IGoods goods, out BackpackSlot targetSlot);
        List<BackpackSlot> BackpackItems { get; }

    }
    
    public static class BackpackSlotWriter
    {
      

        public static void WriteBackpackItem(this NetworkWriter writer, BackpackSlot slot) {
            writer.WriteString(slot.PrefabName);
            writer.WriteString(slot.SpriteName);
            writer.WriteInt(slot.Count);
        }

        public static BackpackSlot ReadBackpackItem(this NetworkReader reader) {
            string prefabName = reader.ReadString();
            string spriteName = reader.ReadString();
            int count = reader.ReadInt();
            return new BackpackSlot() {
                PrefabName = prefabName, SpriteName = spriteName,
                StackedObjects = null,
                ClientSlotCount = count
            };
        }
    }


    public class PlayerInventorySystem : AbstractNetworkedSystem, IPlayerInventorySystem {
        private ResLoader resLoader;
        [SerializeField]
        private List<BackpackSlot> backpackItems = new List<BackpackSlot>();
      
        [SyncVar,SerializeField]
        private int currentIndex = 0;
        private int initialInventoryCapacity = 3;

        IHookSystem hookSystem;

        private int GetBackPackTotalItemCount() {
            int result = 0;
            foreach (BackpackSlot slot in backpackItems) {
                if (slot is {Count: > 0}) {
                    result += slot.Count;
                }
            }

            return result;
        }

        private List<BackpackSlot> GetNonEmptySlots() {
            List<BackpackSlot> slots = new List<BackpackSlot>();
            foreach (BackpackSlot slot in backpackItems)
            {
                if (slot is { Count: > 0 }) {
                    slots.Add(slot);
                }
            }

            return slots;
        }
        private void Awake() {
            ResLoader.Create(loader => resLoader = loader);
            hookSystem = GetComponent<IHookSystem>();
        }

        public override void OnStartAuthority() {
            base.OnStartAuthority();
            this.SendEvent<OnStartInventoryInit>(new OnStartInventoryInit() {
                InventoryCapacity = GetSlotCount(),
                InitialBackPackCapacity = initialInventoryCapacity
            });
            
        }

        public override void OnStartServer() {
            base.OnStartServer();
            for (int i = 0; i < GetSlotCount(); i++) {
                backpackItems.Add(new BackpackSlot(){});
            }

            this.RegisterEvent<OnSpaceshipRequestDropItems>(OnSpaceshipRequestDropItems)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnSpaceshipRequestDropItems(OnSpaceshipRequestDropItems e) {
            if (e.SpaceshipIdentity == netIdentity) {

                int backpackItemCount = GetBackPackTotalItemCount();
                if (backpackItemCount > 0) {

                    int realDropCount = Mathf.Min(e.NumberItemRequest, backpackItemCount);
                    List<BackpackSlot> slots = GetNonEmptySlots();

                    int dropped = 0;
                    BackpackSlot dropSlot = slots[Random.Range(0, slots.Count)];
                    while (dropped < realDropCount) {
                        while (dropSlot == null || dropSlot.Count == 0) {
                            dropSlot = slots[Random.Range(0, slots.Count)];
                        }

                        ServerDropFromBackpack(dropSlot.PrefabName);
                        dropped++;
                    }
                }
                
            }
        }

        
       

        private void OnDestroy() {
            resLoader.ReleaseAllAssets();
        }

        [ServerCallback]
        public void ServerHookToBackpack(string name, GameObject gameObject) {
            BackpackSlot slot = FindItemStackInBackpack(name);

            if (slot != null) {
                slot.PrefabName = name;
                slot.SpriteName =  name + "Sprite";
                slot.StackedObjects.Insert(0,gameObject);
                if (gameObject.TryGetComponent<IGoods>(out IGoods goods))
                {
                    goods.OnAddedToBackpack();
                }
              
                ServerSwitchSlot(backpackItems.FindIndex((backpackSlot => backpackSlot == slot)));
                
            }
            
            TargetOnInventoryUpdate(backpackItems,currentIndex);
        }

        [ServerCallback]
        public void ServerAddToBackpack(string name, GameObject gameObject) {
           ServerCheckCanAddToBackpack(gameObject.GetComponent<IGoods>(), out BackpackSlot slot);
            if (slot != null)
            {
                slot.PrefabName = name;
                slot.SpriteName = name + "Sprite";
                slot.StackedObjects.Insert(0, gameObject);
                if (gameObject.TryGetComponent<IGoods>(out IGoods goods))
                {
                    goods.OnAddedToBackpack();
                }
                
                NetworkServer.UnSpawn(gameObject);
                gameObject.SetActive(false);
                
                if (backpackItems[currentIndex] == slot) {
                   
                        this.SendEvent<OnSwitchItemSlot>(new OnSwitchItemSlot()
                        {
                            PrefabName = name,
                            SlotIndex = currentIndex,
                            Identity = netIdentity,
                            SwitchedGameObject = gameObject,
                            SameSlot = false
                        });
                    
                    
                }
                TargetOnInventoryUpdate(backpackItems, currentIndex);
            }
        }


        [ServerCallback]
        public void ServerRemoveFromCurrentBackpack() {
            if (currentIndex < backpackItems.Count) {
                BackpackSlot slot = backpackItems[currentIndex];
                if (slot!=null && slot.Count > 0) {

                    GameObject oldObj = slot.StackedObjects[0];
                    slot.StackedObjects.RemoveAt(0);

                    GameObject nextObject = null;
                    if (slot.StackedObjects.Count > 0) {
                        nextObject = slot.StackedObjects[0];
                    }
                    this.SendEvent<OnBackpackItemRemoved>(new OnBackpackItemRemoved() {
                        CurrentCount = slot.Count,
                        PrefabName = slot.PrefabName,
                        Identity = netIdentity,
                        NextObject = nextObject,
                        OldObject = oldObj
                    });
                }
                TargetOnInventoryUpdate(backpackItems,currentIndex);
            }
           
        }

        public void ServerRemoveFromBackpack(string name) {
            BackpackSlot slot = FindItemStackInBackpack(name);
            if (slot != null && slot.Count > 0)
            {

                GameObject oldObj = slot.StackedObjects[0];
                slot.StackedObjects.RemoveAt(0);


                BackpackSlot currentSlot = backpackItems[currentIndex];
                GameObject nextObject = null;
                
                if (currentSlot.StackedObjects.Count > 0) {
                    nextObject = currentSlot.StackedObjects[0];
                }
                this.SendEvent<OnBackpackItemRemoved>(new OnBackpackItemRemoved()
                {
                    CurrentCount = currentSlot.Count,
                    PrefabName = currentSlot.PrefabName,
                    Identity = netIdentity,
                    NextObject = nextObject,
                    OldObject = oldObj
                });
            }
            TargetOnInventoryUpdate(backpackItems, currentIndex);

        }

        public void ServerRemoveFromBackpack(GameObject obj) {
            IGoods goods = obj.GetComponent<IGoods>();
            if (goods != null) {
                BackpackSlot slot = FindItemStackInBackpack(goods.Name);

                if (slot != null && slot.Count > 0) {
                    GameObject oldObj = null;
                    if (slot.StackedObjects.Contains(obj)) {
                        slot.StackedObjects.Remove(obj);
                        oldObj = obj;
                    }


                    BackpackSlot currentSlot = backpackItems[currentIndex];
                    GameObject nextObject = null;

                    if (currentSlot.StackedObjects.Count > 0)
                    {
                        nextObject = currentSlot.StackedObjects[0];
                    }
                    this.SendEvent<OnBackpackItemRemoved>(new OnBackpackItemRemoved()
                    {
                        CurrentCount = currentSlot.Count,
                        PrefabName = currentSlot.PrefabName,
                        Identity = netIdentity,
                        NextObject = nextObject,
                        OldObject = oldObj
                    });
                }
            }
        }

        [ServerCallback]
        public void ServerDropFromBackpack(string name) {
            BackpackSlot slot = FindItemStackInBackpack(name);
            if (slot != null) {
                int prevCount = slot.Count;
                
                GameObject droppeGameObject =null;
               
                if (slot.StackedObjects.Count > 0) {
                    droppeGameObject = (slot.StackedObjects[0]);
                    slot.StackedObjects.RemoveAt(0);
                }

                GameObject nextObject = null;
                bool droppedCurrentSlot = false;
                if (slot == backpackItems[currentIndex]) {
                  
                    droppedCurrentSlot = true;
                    if (slot.StackedObjects.Count > 0) {
                        nextObject = slot.StackedObjects[0];
                    }
                }

                this.SendEvent<OnItemDropped>(new OnItemDropped()
                {
                    PrefabName = slot.PrefabName,
                    Identity = netIdentity,
                    DroppedObject = droppeGameObject,
                    NextHookingObject = nextObject,
                    DroppedCurrentSlot = droppedCurrentSlot
                });


                TargetOnInventoryUpdate(backpackItems,currentIndex);
            }
        }

        public int GetSlotIndexFromItemName(string name) { 
            for (int i = 0; i < backpackItems.Count; i++) {
                if (backpackItems[i].PrefabName == name) {
                    return i;
                }
            }

            return -1;
        }

        [ServerCallback]
        public void ServerSwitchSlot(int index) {
            
           
            if (hookSystem.HookedItem==null || hookSystem.HookedItem.Model.CanBeAddedToInventory) {
                GameObject switchedGameObject = null;
                if (index != currentIndex) {
                    if (index < 0)
                    {
                        index = GetSlotCount() - 1;
                    }

                    currentIndex = index % GetSlotCount();
                    BackpackSlot slot = backpackItems[currentIndex];
                    
                    string name = "";
                    if (slot != null && slot.Count > 0)
                    {
                        name = slot.PrefabName;
                        switchedGameObject = slot.StackedObjects[0];
                    }

                    this.SendEvent<OnSwitchItemSlot>(new OnSwitchItemSlot()
                    {
                        PrefabName = name,
                        SlotIndex = currentIndex,
                        Identity = netIdentity,
                        SwitchedGameObject = switchedGameObject,
                        SameSlot = false
                    });
                }
                else {
                    Debug.Log("Inventory System 2");
                    this.SendEvent<OnSwitchItemSlot>(new OnSwitchItemSlot()
                    {
                        PrefabName = name,
                        SlotIndex = currentIndex,
                        Identity = netIdentity,
                        SwitchedGameObject = hookSystem.HookedNetworkIdentity!=null? hookSystem.HookedNetworkIdentity.gameObject : null,
                        SameSlot = true
                    });
                }
               
            }
            TargetOnInventoryUpdate(backpackItems, currentIndex);
        }


        /// <summary>
        /// TODO: support backpack
        /// </summary>
        /// <returns></returns>
        public int GetSlotCount() {
            return initialInventoryCapacity;
        }

        public int GetCurrentSlot() {
            return currentIndex;
        }

        public bool ServerCheckCanAddToBackpack(IGoods goods, out BackpackSlot targetSlot) {
            BackpackSlot firstEmptySlot = null;
            foreach (BackpackSlot slot in backpackItems) {
                if (slot.Count > 0 && slot.PrefabName == goods.Name) {
                    targetSlot = slot;
                    return true;
                }

                if (slot.Count == 0 && firstEmptySlot==null) {
                    if (slot == backpackItems[currentIndex] && hookSystem.IsHooking) {
                        continue;
                    }
                    firstEmptySlot = slot;
                }
            }

            targetSlot = firstEmptySlot;
            return firstEmptySlot!=null;
        }


        public List<BackpackSlot> BackpackItems {
            get {
                return backpackItems;
            }
        }

        private BackpackSlot FindItemStackInBackpack(string name) {
            BackpackSlot firstEmptySlot = null;

            foreach (BackpackSlot item in backpackItems) {
                //available slot with the same name
                if (item.PrefabName == name && item.Count>0) {
                    return item;
                }

                if (item.Count <= 0 && firstEmptySlot == null) {
                    firstEmptySlot = item;
                }
            }

            return firstEmptySlot;
        }

       



        [TargetRpc]
        private void TargetOnInventoryUpdate(List<BackpackSlot> item, int index) {
            this.SendEvent<OnClientInventoryUpdate>(new OnClientInventoryUpdate() {
                AllSlots = item,
                Owner = netIdentity,
                SelectedIndex = index
            });
        }
    }
}

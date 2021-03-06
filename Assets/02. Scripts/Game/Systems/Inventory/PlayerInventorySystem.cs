using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public GoodsRarity Rarity;
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


    public struct OnClientInventorySlotIncrease {
        public int AddedCount;
        public int BackPackCapacity;
        public bool IsInitialBackpack;
    }
    public interface IPlayerInventorySystem : ISystem {
        void ServerHookToBackpack(string name, GameObject gameObject);

        void ServerAddToBackpack(string name, GameObject gameObject, Action<GameObject> onFailedToAdd);
        GameObject ServerRemoveFromCurrentBackpack();
        GameObject ServerRemoveFromBackpack(string name);

        GameObject ServerRemoveFromBackpack(int index);

        void ServerRemoveFromBackpack(GameObject obj);
        void ServerDropFromBackpack(string name);

        void ServerAddSlots(int count);
        int GetSlotIndexFromItemName(string name);

        bool FindItemExists(string name);

        void ServerSwitchSlot(int index);
        int GetSlotCount();
        int GetCurrentSlot();

        bool ServerCheckCanAddToBackpack(IGoods goods, out BackpackSlot targetSlot, out int index);
        List<BackpackSlot> BackpackItems { get; }

    }
    
    public static class BackpackSlotWriter
    {
      

        public static void WriteBackpackItem(this NetworkWriter writer, BackpackSlot slot) {
            writer.WriteString(slot.PrefabName);
            writer.WriteString(slot.SpriteName);
            writer.WriteInt(slot.Count);
            writer.WriteInt((int) slot.Rarity);
        }

        public static BackpackSlot ReadBackpackItem(this NetworkReader reader) {
            string prefabName = reader.ReadString();
            string spriteName = reader.ReadString();
            int count = reader.ReadInt();
            GoodsRarity rarity =(GoodsRarity) reader.ReadInt();
            return new BackpackSlot() {
                PrefabName = prefabName, SpriteName = spriteName,
                StackedObjects = null,
                ClientSlotCount = count,
                Rarity = rarity
            };
        }
    }


    public class PlayerInventorySystem : AbstractNetworkedSystem, IPlayerInventorySystem {
       // private ResLoader resLoader;
        [SerializeField]
        private List<BackpackSlot> backpackItems = new List<BackpackSlot>();
      
        [SyncVar,SerializeField]
        private int currentIndex = 0;
        private int initialInventoryCapacity = 3;
        private int maxInventoryCapacity = 10;
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

        private List<BackpackSlot> GetNonEmptySlotsWithDroppableItems()
        {
            List<BackpackSlot> slots = new List<BackpackSlot>();
            foreach (BackpackSlot slot in backpackItems)
            {
                if (slot is { Count: > 0 } && slot.StackedObjects[0] && slot.StackedObjects[0].GetComponent<IGoods>().DroppableFromBackpack) {
                    slots.Add(slot);
                }
            }

            return slots;
        }
        private void Awake() {
          //  ResLoader.Create(loader => resLoader = loader);
            hookSystem = GetComponent<IHookSystem>();
        }

        public override void OnStartAuthority() {
            base.OnStartAuthority();
            this.SendEvent<OnClientInventorySlotIncrease>(new OnClientInventorySlotIncrease() {
                AddedCount = GetSlotCount(),
                BackPackCapacity = initialInventoryCapacity,
                IsInitialBackpack = true
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
        public void ServerAddSlots(int count) {
            int remainingSlots = maxInventoryCapacity - backpackItems.Count;
            if (count > remainingSlots) {
                count = remainingSlots;
            }
            
            for (int i = 0; i < count; i++) {
                backpackItems.Add(new BackpackSlot());
            }

            TargetOnInventorySlotNumberIncrease(count, backpackItems.Count);
            TargetOnInventoryUpdate(backpackItems, currentIndex);

        }

        
        private void OnSpaceshipRequestDropItems(OnSpaceshipRequestDropItems e) {
            if (e.SpaceshipIdentity == netIdentity) {

                List<BackpackSlot> slots = GetNonEmptySlotsWithDroppableItems();

                if (slots.Count > 0) {
                    int totalItemCount = slots.Sum(slot => slot.Count);
                    int realDropCount = Mathf.Min(e.NumberItemRequest, totalItemCount);
                    
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
            //resLoader.ReleaseAllAssets();
        }

        [ServerCallback]
        public void ServerHookToBackpack(string name, GameObject gameObject) {
            BackpackSlot slot = FindItemStackInBackpack(name, out int index);

            if (slot != null) {
                slot.PrefabName = name;
                slot.SpriteName =  name + "Sprite";
                slot.StackedObjects.Insert(0,gameObject);
                if (gameObject.TryGetComponent<IGoods>(out IGoods goods)) {
                    goods.OnAddedToBackpack();
                    slot.Rarity = goods.GoodRarity;
                    this.SendEvent<OnServerItemAddedToBackpack>(new OnServerItemAddedToBackpack()
                    {
                        goods = goods,
                        HookedBy = goods.HookedByIdentity,
                        SlotIndex = index
                    });
                }
              
                ServerSwitchSlot(backpackItems.FindIndex((backpackSlot => backpackSlot == slot)));
                
            }
            
            TargetOnInventoryUpdate(backpackItems,currentIndex);
        }

        [ServerCallback]
        public void ServerAddToBackpack(string name, GameObject gameObject, Action<GameObject> onFailedToAdd) {
           ServerCheckCanAddToBackpack(gameObject.GetComponent<IGoods>(), out BackpackSlot slot, out int index);
           bool hookSuccess = true;
            if (slot != null)
            {
                slot.PrefabName = name;
                slot.SpriteName = name + "Sprite";
                slot.StackedObjects.Insert(0, gameObject);
                if (gameObject.TryGetComponent<IGoods>(out IGoods goods))
                {
                    goods.OnAddedToBackpack();
                    slot.Rarity = goods.GoodRarity;
                    if (goods.TryHook(netIdentity)) {
                        this.SendEvent<OnServerItemAddedToBackpack>(new OnServerItemAddedToBackpack()
                        {
                            goods = goods,
                            HookedBy = netIdentity,
                            SlotIndex = index
                        });
                    }
                    else {
                        hookSuccess = false;
                    }
                }

                if (!hookSuccess) {
                    return;
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
                GetComponent<ISpaceshipConfigurationModel>().ServerUpdateMass();
            }
            else {
                onFailedToAdd?.Invoke(gameObject);
            }
        }


        [ServerCallback]
        public GameObject ServerRemoveFromCurrentBackpack() {
            if (currentIndex < backpackItems.Count) {
                BackpackSlot slot = backpackItems[currentIndex];
                return ServerRemoveFromBackpack(slot, 1).FirstOrDefault();
            }

            return null;
        }

        public GameObject ServerRemoveFromBackpack(string name) {
            BackpackSlot slot = FindItemStackInBackpack(name, out int index);
            return ServerRemoveFromBackpack(slot,1).FirstOrDefault();
        }        

        public GameObject ServerRemoveFromBackpack(int index) {
            if (currentIndex < backpackItems.Count) {
                BackpackSlot slot = backpackItems[index];
                return ServerRemoveFromBackpack(slot,1).FirstOrDefault();
            }

            return null;
        }
        public void ServerRemoveFromBackpack(GameObject obj) {
            IGoods goods = obj.GetComponent<IGoods>();
            if (goods != null)
            {
                BackpackSlot slot = FindItemStackInBackpack(goods.Name, out int index);
                ServerRemoveFromBackpack(obj, slot);
            }
        }
        private List<GameObject> ServerRemoveFromBackpack(BackpackSlot slot, int number) {
            List<GameObject> retResult = new List<GameObject>();
            if (slot != null && slot.Count > 0) {
                for (int i = 0; i < number; i++) {
                    if (slot.StackedObjects.Count <= 0) {
                        break;
                    }
                    GameObject oldObj = slot.StackedObjects[0];
                    slot.StackedObjects.RemoveAt(0);
                    GameObject nextObject = null;
                    if (slot.StackedObjects.Count > 0) {
                        nextObject = slot.StackedObjects[0];
                    }
                    this.SendEvent<OnBackpackItemRemoved>(new OnBackpackItemRemoved()
                    {
                        CurrentCount = slot.Count,
                        PrefabName = slot.PrefabName,
                        Identity = netIdentity,
                        NextObject = nextObject,
                        OldObject = oldObj
                    });
                    retResult.Add(oldObj);
                }
            }
            TargetOnInventoryUpdate(backpackItems, currentIndex);
            return retResult;
        }
        private void ServerRemoveFromBackpack(GameObject oldObj, BackpackSlot slot) {
            if (slot != null && slot.Count > 0) {
                
                if (slot.StackedObjects.Contains(oldObj)) {
                    slot.StackedObjects.Remove(oldObj);
                }

                GameObject nextObject = null;

                if (slot.StackedObjects.Count > 0)
                {
                    nextObject = slot.StackedObjects[0];
                }
                this.SendEvent<OnBackpackItemRemoved>(new OnBackpackItemRemoved()
                {
                    CurrentCount = slot.Count,
                    PrefabName = slot.PrefabName,
                    Identity = netIdentity,
                    NextObject = nextObject,
                    OldObject = oldObj
                });
            }
            TargetOnInventoryUpdate(backpackItems, currentIndex);
        }

       

        [ServerCallback]
        public void ServerDropFromBackpack(string name) {
            BackpackSlot slot = FindItemStackInBackpack(name, out int index);
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

        public bool FindItemExists(string name) {
            for (int i = 0; i < backpackItems.Count; i++) {
                if (backpackItems[i].PrefabName == name && backpackItems[i].Count>0)
                {
                    return true;
                }
            }

            return false;
        }

        [ServerCallback]
        public void ServerSwitchSlot(int index) {
            
           
            if (hookSystem.HookedItem==null || hookSystem.HookedNetworkIdentity==null || hookSystem.HookedItem.Model.CanBeAddedToInventory) {
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
            return Mathf.Max(initialInventoryCapacity, backpackItems.Count);
        }

        public int GetCurrentSlot() {
            return currentIndex;
        }

        public bool ServerCheckCanAddToBackpack(IGoods goods, out BackpackSlot targetSlot, out int index) {
           // if (goods.HookState == HookState.Hooked) {
              //  targetSlot = null;
                //return false;
            //}
            index = -1;
            BackpackSlot firstEmptySlot = null;
            int firstEmptyIndex = -1;
            for (int i = 0; i < backpackItems.Count; i++) {
                BackpackSlot slot = backpackItems[i];
                if (slot.Count > 0 && slot.PrefabName == goods.Name)
                {
                    targetSlot = slot;
                    index = i;
                    return true;
                }

                if (slot.Count == 0 && firstEmptySlot == null) {
                    if (slot == backpackItems[currentIndex] && hookSystem.IsHooking) { 
                        continue;
                    }
                    firstEmptySlot = slot;
                    firstEmptyIndex = i;
                }
            }
            targetSlot = firstEmptySlot;
            index = firstEmptyIndex;
            return firstEmptySlot!=null;
        }


        public List<BackpackSlot> BackpackItems {
            get {
                return backpackItems;
            }
        }

        private BackpackSlot FindItemStackInBackpack(string name, out int index) {
            BackpackSlot firstEmptySlot = null;
            index = -1;

            for (int i = 0; i < backpackItems.Count; i++) {
                BackpackSlot item = backpackItems[i];
                //available slot with the same name
                if (item.PrefabName == name && item.Count > 0) {
                    index = i;
                    return item;
                }

                if (item.Count <= 0 && firstEmptySlot == null) {
                    index = i;
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

        [TargetRpc]
        private void TargetOnInventorySlotNumberIncrease(int increaseCount, int backpackCapacity) {
            this.SendEvent<OnClientInventorySlotIncrease>(new OnClientInventorySlotIncrease() {
                AddedCount = increaseCount,
                BackPackCapacity = backpackCapacity,
                IsInitialBackpack = false
            });
        }
    }
}

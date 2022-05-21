using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Mirror;
using UnityEngine;

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
        public Stack<GameObject> StackedObjects = new Stack<GameObject>();
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
        public int DropCount;
        public List<GameObject> DroppedObjects;
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
        void ServerAddToBackpack(string name, GameObject gameObject);
        void ServerRemoveFromCurrentBackpack();
        void ServerDropFromBackpack(string name, int number);

        void ServerSwitchSlot(int index);
        int GetSlotCount();

        int GetCurrentSlot();

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
        }

        private void OnDestroy() {
            resLoader.ReleaseAllAssets();
        }

        [ServerCallback]
        public void ServerAddToBackpack(string name, GameObject gameObject) {
            BackpackSlot slot = FindItemStackInBackpack(name);

            if (slot != null) {
                slot.PrefabName = name;
                slot.SpriteName =  name + "Sprite";
                slot.StackedObjects.Push(gameObject);
                ServerSwitchSlot(backpackItems.FindIndex((backpackSlot => backpackSlot == slot)));
            }
            
            TargetOnInventoryUpdate(backpackItems,currentIndex);
        }


        [ServerCallback]
        public void ServerRemoveFromCurrentBackpack() {
            if (currentIndex < backpackItems.Count) {
                BackpackSlot slot = backpackItems[currentIndex];
                if (slot!=null && slot.Count > 0) {

                    GameObject oldObj = slot.StackedObjects.Pop();

                    GameObject nextObject = null;
                    if (slot.StackedObjects.Count > 0) {
                        nextObject = slot.StackedObjects.Peek();
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

        [ServerCallback]
        public void ServerDropFromBackpack(string name, int number) {
            BackpackSlot slot = FindItemStackInBackpack(name);
            if (slot != null) {
                int prevCount = slot.Count;
                
                List<GameObject> droppeGameObjects = new List<GameObject>();
                for (int i = 0; i < number; i++) {
                    if (slot.StackedObjects.Count > 0) {
                        droppeGameObjects.Add(slot.StackedObjects.Pop());
                    }
                }

                int dropCount = prevCount - slot.Count;

                this.SendEvent<OnItemDropped>(new OnItemDropped()
                {
                    DropCount = dropCount,
                    PrefabName = slot.PrefabName,
                    Identity = netIdentity,
                    DroppedObjects = droppeGameObjects
                });


                TargetOnInventoryUpdate(backpackItems,currentIndex);
            }
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
                        switchedGameObject = slot.StackedObjects.Peek();
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
                    this.SendEvent<OnSwitchItemSlot>(new OnSwitchItemSlot()
                    {
                        PrefabName = name,
                        SlotIndex = currentIndex,
                        Identity = netIdentity,
                        SwitchedGameObject = hookSystem.HookedNetworkIdentity.gameObject,
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

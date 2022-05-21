using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    [Serializable]
    public class BackpackSlot {

        public string PrefabName;
        public string SpriteName;
        public int Count;
    }

    public struct OnItemDropped {
        public NetworkIdentity Identity;
        public string PrefabName;
        public int DropCount;
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
    }


    public struct OnStartInventoryInit {
        public int InventoryCapacity;
        public int InitialBackPackCapacity;
    }
    public interface IPlayerInventorySystem : ISystem {
        void ServerAddToBackpack(string name, int number);
        void ServerRemoveFromCurrentBackpack();
        void ServerDropFromBackpack(string name, int number);

        void ServerSwitchSlot(int index);
        int GetSlotCount();

        int GetCurrentSlot();

    }
    /*
    public static class BackpackSlotWriter
    {
        public static void WriteBackpackSlot(this NetworkWriter writer, List<BackpackSlot> list) {
            writer.WriteList<BackpackSlot>(list);
        }

        public static List<BackpackSlot> ReadBackpackSlot(this NetworkReader reader) {
            return reader.ReadList<BackpackSlot>();
        }

        public static void WriteBackpackItem(this NetworkWriter writer, BackpackSlot slot) {
            writer.WriteString(slot.PrefabName);
            writer.WriteString(slot.SpriteName);
            writer.WriteInt(slot.Count);
        }

        public static BackpackSlot ReadBackpackItem(this NetworkReader reader) {
            string prefabName = reader.ReadString();
            string spriteName = reader.ReadString();
            int count = reader.ReadInt();
            return new BackpackSlot() {Count = count, PrefabName = prefabName, SpriteName = spriteName};
        }
    }*/


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
                backpackItems.Add(new BackpackSlot(){Count = 0});
            }
        }

        private void OnDestroy() {
            resLoader.ReleaseAllAssets();
        }

        [ServerCallback]
        public void ServerAddToBackpack(string name, int number) {
            BackpackSlot slot = FindItemStackInBackpack(name);

            if (slot != null) {
                slot.PrefabName = name;
                slot.Count += number;
                slot.SpriteName =  name + "Sprite";
                ServerSwitchSlot(backpackItems.FindIndex((backpackSlot => backpackSlot == slot)));
            }
            
            TargetOnInventoryUpdate(backpackItems,currentIndex);
        }

        [ServerCallback]
        public void ServerRemoveFromCurrentBackpack() {
            if (currentIndex < backpackItems.Count) {
                BackpackSlot slot = backpackItems[currentIndex];
                if (slot!=null && slot.Count > 0) {
                    slot.Count--;
                    this.SendEvent<OnBackpackItemRemoved>(new OnBackpackItemRemoved() {
                        CurrentCount = slot.Count,
                        PrefabName = slot.PrefabName,
                        Identity = netIdentity
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
                slot.Count -= number;
                slot.Count = Mathf.Max(slot.Count, 0);
                this.SendEvent<OnItemDropped>(new OnItemDropped()
                {
                    DropCount = prevCount - slot.Count,
                    PrefabName = slot.PrefabName,
                    Identity = netIdentity
                });


                TargetOnInventoryUpdate(backpackItems,currentIndex);
            }
        }

        [ServerCallback]
        public void ServerSwitchSlot(int index) {
            

            if (hookSystem.HookedItem==null || hookSystem.HookedItem.Model.CanBeAddedToInventory) {
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
                    }

                    this.SendEvent<OnSwitchItemSlot>(new OnSwitchItemSlot()
                    {
                        PrefabName = name,
                        SlotIndex = currentIndex,
                        Identity = netIdentity
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

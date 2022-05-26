using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct CanBeUsedGoodsBasicInfo {
        public int Durability;
        public bool CanBeUsed;
        public float Frequency;
        public int MaxDurability;
        public ItemUseMode UseMode;
    }
    public abstract class AbstractCanBeUsedGoodsViewController : AbstractGoodsViewController, ICanBeUsedGoodsViewController{
        public new ICanBeUsed GoodsModel { get; protected set; }

        protected override void Awake() {
            base.Awake();
            GoodsModel = GetComponent<ICanBeUsed>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnItemUsed>(OnServerItemUsed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnItemBroken>(OnServerItemBroken).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnHookItemSwitched>(OnServerItemSwitched).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnItemDurabilityChange>(OnItemDurabilityChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            // this.RegisterEvent<OnBackpackItemRemoved>(OnItemStopBeingSelected)
            //  .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnItemDurabilityChange(OnItemDurabilityChange e) {
            if (e.Model == GoodsModel) {
                OnServerDurabilityChange();
                CanBeUsedGoodsBasicInfo basicInfo = new CanBeUsedGoodsBasicInfo()
                {
                    CanBeUsed = GoodsModel.CanBeUsed,
                    Durability = GoodsModel.Durability,
                    Frequency = GoodsModel.Frequency,
                    MaxDurability = GoodsModel.MaxDurability,
                    UseMode = GoodsModel.UseMode
                };
                RpcOnItemDurabilityChange(basicInfo);
                if (e.HookedBy) {
                    TargetOnItemDurabilityChange(e.HookedBy.connectionToClient, basicInfo,
                        e.HookedBy.GetComponent<IPlayerInventorySystem>().GetCurrentSlot());
                }
              
            }
        }

        [ServerCallback]
        private void OnItemStopBeingSelected(OnBackpackItemRemoved e) {
            CanBeUsedGoodsBasicInfo basicInfo = new CanBeUsedGoodsBasicInfo()
            {
                CanBeUsed = GoodsModel.CanBeUsed,
                Durability = GoodsModel.Durability,
                Frequency = GoodsModel.Frequency,
                MaxDurability = GoodsModel.MaxDurability,
                UseMode = GoodsModel.UseMode
            };

            if (e.OldObject.GetComponent<NetworkIdentity>() == netIdentity)
            {
                OnServerStopSelectThisItem();
                RpcOnItemStopBeingSelected();
                TargetOnItemStopBeingSelected(e.Identity.connectionToClient);
            }
        }

        private bool switched = false;

        [ServerCallback]
        private void OnServerItemSwitched(OnHookItemSwitched e) {
            
            CanBeUsedGoodsBasicInfo basicInfo = new CanBeUsedGoodsBasicInfo()
            {
                CanBeUsed = GoodsModel.CanBeUsed,
                Durability = GoodsModel.Durability,
                Frequency = GoodsModel.Frequency,
                MaxDurability = GoodsModel.MaxDurability,
                UseMode = GoodsModel.UseMode
            };

           

            if (e.NewIdentity == netIdentity && !switched) {
                switched = true;
                OnServerStartSelectThisItem();
                RpcOnItemStartBeingSelected(basicInfo);
                TargetOnItemStartBeingSelected(e.OwnerIdentity.connectionToClient, basicInfo,
                    e.OwnerIdentity.GetComponent<IPlayerInventorySystem>().GetCurrentSlot());
            } else if (e.OldIdentity == netIdentity && switched) {
                switched = false;
                OnServerStopSelectThisItem();
                RpcOnItemStopBeingSelected();
                TargetOnItemStopBeingSelected(e.OwnerIdentity.connectionToClient);
            }
        }

        [ServerCallback]
        private void OnServerItemBroken(OnItemBroken e) {
            if (e.Item == GoodsModel) {
                OnServerItemBroken();
                CanBeUsedGoodsBasicInfo basicInfo = new CanBeUsedGoodsBasicInfo()
                {
                    CanBeUsed = GoodsModel.CanBeUsed,
                    Durability = GoodsModel.Durability,
                    Frequency = GoodsModel.Frequency,
                    MaxDurability = GoodsModel.MaxDurability,
                    UseMode = GoodsModel.UseMode
                };
                RpcOnItemBroken(basicInfo);
                if (e.HookedBy) {
                    TargetOnItemBroken(e.HookedBy.connectionToClient, basicInfo);
                }

                NetworkServer.Destroy(gameObject);
            }
        }

        [ServerCallback]
        private void OnServerItemUsed(OnItemUsed e) {
            if (e.Item == GoodsModel) {
                OnServerItemUsed();
                CanBeUsedGoodsBasicInfo basicInfo = new CanBeUsedGoodsBasicInfo() {
                    CanBeUsed = GoodsModel.CanBeUsed,
                    Durability = GoodsModel.Durability,
                    Frequency = GoodsModel.Frequency,
                    MaxDurability = GoodsModel.MaxDurability,
                    UseMode = GoodsModel.UseMode
                };
                RpcOnItemUsed(basicInfo);
                if (e.HookedBy) {
                    TargetOnItemUsed(e.HookedBy.connectionToClient, basicInfo);
                }
                
            }
        }

        [ClientRpc]
        protected void RpcOnItemUsed(CanBeUsedGoodsBasicInfo basicInfo) {
            OnClientItemUsed(basicInfo);
        }

        [TargetRpc]
        protected void TargetOnItemUsed(NetworkConnection conn, CanBeUsedGoodsBasicInfo basicInfo) {
            OnClientOwnerItemUsed(basicInfo);
        }

        [ClientRpc]
        protected void RpcOnItemStopBeingSelected()
        {
            OnClientItemStopBeingSelected();
        }

       

        private void OnDisable() {
          
            Debug.Log("OnClientOwnerItemStopBeingSelected");
            OnClientOwnerItemStopBeingSelected();
        }

        [TargetRpc]
        protected void TargetOnItemStopBeingSelected(NetworkConnection conn)
        {
            OnClientOwnerItemStopBeingSelected();
        }

        [ClientRpc]
        protected void RpcOnItemStartBeingSelected(CanBeUsedGoodsBasicInfo basicInfo)
        {
            OnClientItemStartBeingSelected(basicInfo);
        }

        [TargetRpc]
        protected void TargetOnItemStartBeingSelected(NetworkConnection conn,CanBeUsedGoodsBasicInfo basicInfo,
            int slotNumber)
        {
            OnClientOwnerStartBeingSelected(basicInfo, slotNumber);
        }


        [ClientRpc]
        protected void RpcOnItemBroken(CanBeUsedGoodsBasicInfo basicInfo)
        {
            OnClientItemBroken(basicInfo);
        }

        [TargetRpc]
        protected void TargetOnItemBroken(NetworkConnection conn, CanBeUsedGoodsBasicInfo basicInfo)
        {
            OnClientOwnerItemBroken(basicInfo);
        }

        [ClientRpc]
        protected void RpcOnItemDurabilityChange(CanBeUsedGoodsBasicInfo basicInfo)
        {
            OnClientItemDurabilityChange(basicInfo);
        }

        [TargetRpc]
        protected void TargetOnItemDurabilityChange(NetworkConnection conn, CanBeUsedGoodsBasicInfo basicInfo, 
            int slotNumber)
        {
            OnClientOwnerDurabilityChange(basicInfo, slotNumber);
        }


        protected abstract void OnServerItemUsed();
        protected abstract void OnServerItemBroken();

        protected abstract void OnServerDurabilityChange();
        protected abstract void OnServerStopSelectThisItem();

        protected abstract void OnServerStartSelectThisItem();

        protected abstract void OnClientItemUsed(CanBeUsedGoodsBasicInfo basicInfo);
       protected abstract void OnClientOwnerItemUsed(CanBeUsedGoodsBasicInfo basicInfo);

       protected abstract void OnClientItemBroken(CanBeUsedGoodsBasicInfo basicInfo);
       protected abstract void OnClientOwnerItemBroken(CanBeUsedGoodsBasicInfo basicInfo);

       protected abstract void OnClientItemStopBeingSelected();
       protected abstract void OnClientOwnerItemStopBeingSelected();

       protected abstract void OnClientItemStartBeingSelected(CanBeUsedGoodsBasicInfo basicInfo);
       protected abstract void OnClientOwnerStartBeingSelected(CanBeUsedGoodsBasicInfo basicInfo, int slotNumber);

       protected abstract void OnClientItemDurabilityChange(CanBeUsedGoodsBasicInfo basicInfo);
       protected abstract void OnClientOwnerDurabilityChange(CanBeUsedGoodsBasicInfo basicInfo, int slotNumber);
    }
}

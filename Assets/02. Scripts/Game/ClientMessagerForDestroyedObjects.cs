using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class ClientMessagerForDestroyedObjects : AbstractNetworkedController<Mikrocosmos>, ISingleton, ICanSendEvent {
        public static ClientMessagerForDestroyedObjects Singleton {
            get {
                return SingletonProperty<ClientMessagerForDestroyedObjects>.Singleton;
            }
        }
        
        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnItemDurabilityChange>(OnItemDurabilityChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnItemDurabilityChange(OnItemDurabilityChange e) {
            if (e.IsItemDestroyed) {
                ICanBeUsed GoodsModel = e.Model;
                
                CanBeUsedGoodsBasicInfo basicInfo = new CanBeUsedGoodsBasicInfo()
                {
                    CanBeUsed = GoodsModel.CanBeUsed,
                    Durability = GoodsModel.Durability,
                    Frequency = GoodsModel.Frequency,
                    MaxDurability = GoodsModel.MaxDurability,
                    UseMode = GoodsModel.UseMode
                };
                Debug.Log($"Client Message Obj Triggered: {GoodsModel.HookedByIdentity}");
                if (GoodsModel.HookedByIdentity)
                {
                    Debug.Log("Client Message Obj Triggered");
                    TargetOnItemDurabilityChange(e.HookedBy.connectionToClient, basicInfo,
                        GoodsModel.HookedByIdentity.GetComponent<IPlayerInventorySystem>().GetSlotIndexFromItemName(GoodsModel.Name));
                }

            }
        }


      
        [TargetRpc]
        protected void TargetOnItemDurabilityChange(NetworkConnection conn, CanBeUsedGoodsBasicInfo basicInfo,
            int slotNumber) {
            if (slotNumber >= 0) {
                this.SendEvent<OnGoodsUpdateViewControllerDurability>(new OnGoodsUpdateViewControllerDurability()
                {
                    DurabilityFraction = basicInfo.Durability / (float)basicInfo.MaxDurability,
                    DurabilitySprite = null,
                    UsePreviousSprite = true,
                    SlotNumber = slotNumber
                });
            }
            
        }


        public void OnSingletonInit() {
            
        }
    }
}

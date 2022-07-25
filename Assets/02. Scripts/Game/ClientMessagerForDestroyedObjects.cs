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

        [SerializeField] private List<GameObject> particles;
        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnItemDurabilityChange>(OnItemDurabilityChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject, true);
            
        }

        public void OnItemAddedToBackpack(NetworkIdentity identity, CanBeUsedGoodsBasicInfo basicInfo, int slotIndex, Color durabilityColor) {
            TargetOnItemAddedToInventory(identity.connectionToClient, basicInfo, slotIndex, durabilityColor);
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

        public void ServerSpawnParticleOnClient(Vector2 position, int index) {
            RpcSpawnParticle(position, index);
        }

        [TargetRpc]
        protected void TargetOnItemAddedToInventory(NetworkConnection conn, CanBeUsedGoodsBasicInfo basicInfo,
            int slotNumber, Color durabilityColor) {
            if (slotNumber >= 0)
            {
                this.SendEvent<OnGoodsUpdateViewControllerDurability>(new OnGoodsUpdateViewControllerDurability()
                {
                    DurabilityFraction = basicInfo.Durability / (float)basicInfo.MaxDurability,
                    DurabilityColor = durabilityColor,
                    SlotNumber = slotNumber,
                    UsePreviousSprite = false
                });
            }
        }


        [TargetRpc]
        protected void TargetOnItemDurabilityChange(NetworkConnection conn, CanBeUsedGoodsBasicInfo basicInfo,
            int slotNumber) {
            if (slotNumber >= 0) {
                this.SendEvent<OnGoodsUpdateViewControllerDurability>(new OnGoodsUpdateViewControllerDurability()
                {
                    DurabilityFraction = basicInfo.Durability / (float)basicInfo.MaxDurability,
                    DurabilityColor = Color.white,
                    UsePreviousSprite = true,
                    SlotNumber = slotNumber
                });
            }
            
        }

        [ClientRpc]
        protected void RpcSpawnParticle(Vector2 position, int index) {
            Instantiate(particles[index], position, Quaternion.identity);
        }


        public void OnSingletonInit() {
            
        }
    }
}

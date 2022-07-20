using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{

    public struct OnGoodsUpdateViewControllerDurability {
        public int SlotNumber;
        public float DurabilityFraction;
        public Sprite DurabilitySprite;
        public bool UsePreviousSprite;
    }
    public class BasicGoodsViewController : AbstractCanBeUsedGoodsViewController, ICanSendEvent {
     
        [SerializeField] protected Sprite DurabilityCountSprite;


        protected NetworkIdentity Owner {
            get {
                if (Model.HookedByIdentity == null || Model.HookState == HookState.Freed) {
                    return null;
                }
                return Model.HookedByIdentity;
            }
        }

        protected void DealDamage(IDamagable victim) {
            int damage = GoodsModel.Damage;

            if (Owner) {
                Owner.TryGetComponent<IBuffSystem>(out var ownerBuffSystem);
                if (ownerBuffSystem != null) {
                    if (ownerBuffSystem.HasBuff<PermanentPowerUpBuff>(out PermanentPowerUpBuff powerBuff)) {
                        damage *= (1 + Mathf.RoundToInt(powerBuff.CurrentLevel * powerBuff.AdditionalDamageAdditionPercentage));
                    }
                }
            }

            victim.TakeRawDamage(damage, Model.HookedByIdentity);
        }

        [ServerCallback]
        protected override void OnServerItemUsed() {
            Debug.Log("OnServerItemUsed");
        }
        [ServerCallback]
        protected override void OnServerItemStopUsed() {
            Debug.Log("OnServerItemStopUsed");
        }

        [ServerCallback]
        protected override void OnServerItemBroken() {
            
            Debug.Log("OnServerItemBroken");
        }

        protected override void OnServerDurabilityChange() {
            Debug.Log("OnServerDurabilityChange");
        }

        [ServerCallback]
        protected override void OnServerStopSelectThisItem() {
            Debug.Log("OnServerStopSelectThisItem");
        }

        [ServerCallback]
        protected override void OnServerStartSelectThisItem() {
            Debug.Log("OnServerStartSelectThisItem");
        }

        [ClientCallback]
        protected override void OnClientItemUsed(CanBeUsedGoodsBasicInfo basicInfo) {
            Debug.Log("OnClientItemUsed");
        }

        [ClientCallback]
        protected override void OnClientOwnerItemUsed(CanBeUsedGoodsBasicInfo basicInfo) {
            Debug.Log("OnOwnerItemUsed");
        }

        [ClientCallback]
        protected override void OnClientItemBroken(CanBeUsedGoodsBasicInfo basicInfo) {
            Debug.Log("OnClientItemBroken");
        }

        [ClientCallback]
        protected override void OnClientOwnerItemBroken(CanBeUsedGoodsBasicInfo basicInfo) {
            Debug.Log("OnClientItemBroken");
        }

        [ClientCallback]
        protected override void OnClientItemStopBeingSelected() {
            Debug.Log("OnClientItemStopBeingSelected");
        }

        [ClientCallback]
        protected override void OnClientOwnerItemStopBeingSelected() {
            Debug.Log("OnClientOwnerItemStopBeingSelected");
         
        }

        [ClientCallback]
        protected override void OnClientItemStartBeingSelected(CanBeUsedGoodsBasicInfo basicInfo) {
            Debug.Log("OnClientItemStartBeingSelected");
            
            
        }

        [ClientCallback]
        protected override void OnClientOwnerStartBeingSelected(CanBeUsedGoodsBasicInfo basicInfo,
            int slotNumber) {
            Debug.Log($"OnClientOwnerStartBeingSelected. Slot: {slotNumber}");
            if (basicInfo.MaxDurability >= 0 && basicInfo.CanBeUsed) {
                this.SendEvent<OnGoodsUpdateViewControllerDurability>(new OnGoodsUpdateViewControllerDurability() {
                    DurabilityFraction = basicInfo.Durability /(float) basicInfo.MaxDurability,
                    DurabilitySprite =  DurabilityCountSprite,
                    SlotNumber = slotNumber
                });
            }
            else {
                this.SendEvent<OnGoodsUpdateViewControllerDurability>(new OnGoodsUpdateViewControllerDurability()
                {
                    DurabilityFraction = 0,
                    DurabilitySprite = null,
                    SlotNumber = slotNumber
                });
            }
        }

        protected override void OnClientItemDurabilityChange(CanBeUsedGoodsBasicInfo basicInfo) {
            Debug.Log("OnClientItemDurabilityChange");
        }

        protected override void OnClientOwnerDurabilityChange(CanBeUsedGoodsBasicInfo basicInfo, int slotNumber) {
            this.SendEvent<OnGoodsUpdateViewControllerDurability>(new OnGoodsUpdateViewControllerDurability()
            {
                DurabilityFraction = basicInfo.Durability / (float)basicInfo.MaxDurability,
                DurabilitySprite = DurabilityCountSprite,
                SlotNumber = slotNumber
            });
        }

        [ClientCallback]
        protected GameObject GetCurrentSelectedSlotObject(int index) {
            return GameObject.FindGameObjectsWithTag("ItemSlot")[index];
        }
    }
}

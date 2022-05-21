using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class BasicGoodsViewController : AbstractCanBeUsedGoodsViewController {
        [SerializeField] private GameObject DurabilityCountTextPrefab;
        [SerializeField] private Vector2 DurabilityCountTextSpawnLocalPosition;

        private GameObject currentDurabilityCountObject;

       


        [ServerCallback]
        protected override void OnServerItemUsed() {
            Debug.Log("OnServerItemUsed");
        }

        [ServerCallback]
        protected override void OnServerItemBroken() {
            Debug.Log("OnServerItemBroken");
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
            if (currentDurabilityCountObject) {
                currentDurabilityCountObject.GetComponent<TMP_Text>().text =
                    $"{basicInfo.Durability}/{basicInfo.MaxDurability}";
            }
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
            if (currentDurabilityCountObject) {
                Destroy(currentDurabilityCountObject);
            }
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
                Transform currentSelectedSlotObject = GetCurrentSelectedSlotObject(slotNumber).transform;
                currentDurabilityCountObject = Instantiate(DurabilityCountTextPrefab, currentSelectedSlotObject);
                currentDurabilityCountObject.transform.localPosition = DurabilityCountTextSpawnLocalPosition;
                currentDurabilityCountObject.GetComponent<TMP_Text>().text =
                    $"{basicInfo.Durability}/{basicInfo.MaxDurability}";
            }
        }

        [ClientCallback]
        protected GameObject GetCurrentSelectedSlotObject(int index) {
            return GameObject.FindGameObjectsWithTag("ItemSlot")[index];
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Mikrocosmos
{
    public class PlayerItemAbsorbTrigger : AbstractMikroController<Mikrocosmos> {
        private HashSet<GameObject> itemsCantAbsorb = new HashSet<GameObject>();
        private IPlayerInventorySystem inventorySystem;
        private ISpaceshipConfigurationModel playerModel;
        private NetworkIdentity identity;
        private Collider2D trigger;

        [SerializeField] private bool canAbsorbWhenBackpackEmpty = false;
        private void Awake() {
            inventorySystem = GetComponentInParent<IPlayerInventorySystem>();
            this.RegisterEvent<OnItemDropped>(OnSpaceshipItemDropped).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnHookedItemUnHooked>(OnHookedItemUnHooked)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            identity = GetComponentInParent<NetworkIdentity>();
            playerModel = GetComponentInParent<ISpaceshipConfigurationModel>();
            trigger = GetComponent<Collider2D>();
        }

        private void OnHookedItemUnHooked(OnHookedItemUnHooked e) {
            if (NetworkServer.active) {
                if (e.OwnerIdentity == identity) {
                    if (e.GameObject && e.GameObject.GetComponent<IGoodsViewController>() != null) {
                        itemsCantAbsorb.Add(e.GameObject);
                    }
                }
            }
        }

        private void OnSpaceshipItemDropped(OnItemDropped e) {
            if (e.Identity == identity && NetworkServer.active) {
                if (e.DroppedObject && e.DroppedObject.GetComponent<IGoodsViewController>()!=null) {
                    itemsCantAbsorb.Add(e.DroppedObject);
                    trigger.enabled = false;
                    this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => {
                        trigger.enabled = true;
                        itemsCantAbsorb.Clear();
                    });
                }
            }
        }


        private void OnTriggerStay2D(Collider2D col) {
            if (itemsCantAbsorb.Contains(col.gameObject)) {
                return;
            }

           
            if (NetworkServer.active) {
                if (playerModel.CurrentHealth <= 0) {
                    return;
                }
                if (col.gameObject.TryGetComponent<IGoodsViewController>(out IGoodsViewController goodsViewController)) {
                    
                    if (!inventorySystem.FindItemExists(goodsViewController.GoodsModel.Name) && !canAbsorbWhenBackpackEmpty) {
                        return;
                    }
                    goodsViewController.TryAbsorb(inventorySystem, transform.parent.gameObject);
                    itemsCantAbsorb.Add(col.gameObject);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (NetworkServer.active) {
                if (itemsCantAbsorb.Contains(other.gameObject)) {
                    itemsCantAbsorb.Remove(other.gameObject);
                }
            }

        }
    }
}

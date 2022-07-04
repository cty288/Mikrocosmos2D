using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnItemDurabilityChange {
        public ICanBeUsed Model;
        public NetworkIdentity HookedBy;
        public int NewDurability;
        public bool IsItemDestroyed;
    }

    public struct OnItemStopUsed {
        public ICanBeUsed Model;
        public NetworkIdentity HookedBy;
    }
    public abstract class AbstractCanBeUsedGoodsModel : AbstractGoodsModel, ICanBeUsed {
        [field: SyncVar, SerializeField] public bool CanBeUsed { get; set; } = true;

        [field: SyncVar, SerializeField]
        public ItemUseMode UseMode { get; protected set; }

        [field: SyncVar, SerializeField]
        public float Frequency { get; set; }


        [field: SyncVar, SerializeField] 
        public int Durability { get; set; } 

        [field: SerializeField]
        public int MaxDurability { get;  set; }

        protected override void Awake() {
            base.Awake();
            Durability = MaxDurability;
        }

       [field: SerializeField]
        public bool IsUsing { get; private set; } = false;

       // private bool serverItemStopUsedTriggered = true;

        [ServerCallback]
        public void OnItemUsed() {
            IsUsing = true;
            //serverItemStopUsedTriggered = false;
            OnUsed();
        }

        [ServerCallback]        
        public void OnItemStopUsed() {
            IsUsing = false;
            this.SendEvent<OnItemStopUsed>(new OnItemStopUsed() {
                Model = this,
                HookedBy = netIdentity
            });
            Debug.Log($"Item Stop Used: {gameObject.name}");
        }

        /*
        protected override void Update() {
            base.Update();
            if (isServer) {
                if (!IsUsing && !serverItemStopUsedTriggered) {
                    serverItemStopUsedTriggered = true;
                    this.SendEvent<OnItemStopUsed>(new OnItemStopUsed() {
                        Model = this,
                        HookedBy = HookedByIdentity
                    });
                    Debug.Log($"Item Stop Used: {gameObject.name}");
                }
            }
        }

        private void LateUpdate() {
            if (isServer) {
                //IsUsing = false;
            }
          
        }*/

        [ServerCallback]
        public void ReduceDurability(int count, bool isItemDestroyed = false) {
            if (Durability > 0) {
                Durability -= count;
                Durability = Mathf.Max(Durability, 0);
                this.SendEvent<OnItemDurabilityChange>(new OnItemDurabilityChange()
                {
                    HookedBy = HookedByIdentity,
                    Model = this,
                    NewDurability = Durability,
                    IsItemDestroyed = isItemDestroyed
                });
                OnDurabilityReduced();
                if (Durability == 0)
                {
                    OnBroken();
                    this.SendEvent<OnItemBroken>(new OnItemBroken()
                    {
                        Item = this,
                        HookedBy = HookedByIdentity,
                        ItemObj = gameObject
                    });
                    NetworkServer.Destroy(gameObject);
                }
            }
          
        }

        public abstract void OnDurabilityReduced();
        public abstract void OnUsed();

        public abstract void OnBroken();
    }
}

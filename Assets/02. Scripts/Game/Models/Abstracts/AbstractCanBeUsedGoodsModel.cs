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

        

        [ServerCallback]
        public void OnItemUsed() {
            OnUsed();
        }

        [ServerCallback]
        public void ReduceDurability(int count) {
            Durability -= count;
            this.SendEvent<OnItemDurabilityChange>(new OnItemDurabilityChange() {
                HookedBy = HookedByIdentity,
                Model = this,
                NewDurability = Durability
            });
            if (Durability == 0)
            {
                OnBroken();
                this.SendEvent<OnItemBroken>(new OnItemBroken() {
                    Item = this ,
                    HookedBy = HookedByIdentity
                });
            }
        }

        public abstract void OnDurabilityReduced();
        public abstract void OnUsed();

        public abstract void OnBroken();
    }
}

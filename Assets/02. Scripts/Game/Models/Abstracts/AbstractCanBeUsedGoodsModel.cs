using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public abstract class AbstractCanBeUsedGoodsModel : AbstractGoodsModel, ICanBeUsed {
        [field: SyncVar, SerializeField] public bool CanBeUsed { get; set; } = true;

        [field: SyncVar, SerializeField]
        public ItemUseMode UseMode { get; protected set; }

        [field: SyncVar, SerializeField]
        public float Frequency { get; set; }


        [field: SyncVar, SerializeField] 
        public int Durability { get; set; } 

        [field: SerializeField]
        public int MaxDurability { get; protected set; }

        protected override void Awake() {
            base.Awake();
            Durability = MaxDurability;
        }

        

        [ServerCallback]
        public void OnItemUsed() {
            Durability--;
            Debug.Log($"Item {gameObject.name} Used. Durability: {Durability}");
            if (Durability == 0) {
                OnBroken();
            }
        }

        public abstract void OnUsed();

        public abstract void OnBroken();
    }
}

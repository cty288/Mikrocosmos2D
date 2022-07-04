using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class StoneShieldModel : BasicGoodsModel, ICanAbsorbDamage
    {
        [SerializeField]
        private int currCharge;
        public int CurrCharge {
            get => currCharge;
            set => currCharge = value;
        }

      

        public override void OnClientHooked()
        {

        }

        public override void OnClientFreed()
        {

        }

        
        [field: SyncVar]
        public bool AbsorbDamage { get; set; }

        [ServerCallback]
        public int OnAbsorbDamage(float damage) {
            CurrCharge += (int) damage;
            ReduceDurability((int)damage);
            return 0;
        }

        public override void OnUsed() {
            
        }
    }
}

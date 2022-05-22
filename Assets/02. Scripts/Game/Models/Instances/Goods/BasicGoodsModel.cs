using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class BasicGoodsModel : AbstractCanBeUsedGoodsModel {

        [SerializeField] private bool reduceDurabilityInstantlyAfterUse = true;
        public override void OnClientHooked() {
            
        }

        public override void OnClientFreed() {
           
        }

        public override void OnUsed() {
            if (reduceDurabilityInstantlyAfterUse) {
                ReduceDurability(1);
            }
         
        }

        public override void OnBroken() {
          
        }
    }
}

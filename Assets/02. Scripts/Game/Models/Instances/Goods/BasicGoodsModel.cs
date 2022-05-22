using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class BasicGoodsModel : AbstractCanBeUsedGoodsModel {
     
        public override void OnClientHooked() {
            
        }

        public override void OnClientFreed() {
           
        }

        public override void OnUsed() {
            ReduceDurability(1);
        }

        public override void OnBroken() {
          
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class BasicGoodsModel : AbstractCanBeUsedGoodsModel {

        [SerializeField] private bool reduceDurabilityInstantlyAfterUse = true;


        private bool readyToReduceDurability = false;
        public override void OnClientHooked() {
            
        }

        public override void OnClientFreed() {
           
        }

        public override void OnDurabilityReduced() {
            readyToReduceDurability = false;
        }

      
        public override void OnStopServer() {
            base.OnStopServer();
            //to prevent switching to another item before the durability reduction is finished
            if (Durability > 0 && readyToReduceDurability) {
                readyToReduceDurability = false;
                ReduceDurability(1);
            }
        }

        public override void OnUsed() {
            if (reduceDurabilityInstantlyAfterUse) {
                ReduceDurability(1);
            }else {
                readyToReduceDurability = true;
            }
         
        }

        public override void OnBroken() {
          
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class LeafKnifeModel : BasicGoodsModel{
        [SerializeField, SyncVar]
        private float addedForce;

        [SerializeField]
        private float addedMomentum;

        public float AddedMomentum => addedMomentum;

                

        public float AddedForce => addedForce;

        public override void OnUsed() {
            
        }
    }
}

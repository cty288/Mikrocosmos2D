using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class LeafKnifeModel : BasicGoodsModel{
        [SerializeField, SyncVar]
        private float addedForce;

        public float AddedForce => addedForce;
    }
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class BasicGoodsModel : AbstractGoodsModel
    {
        [field: SyncVar, SerializeField]
        public override float SelfMass { get; protected set; }

        public override string Name { get; } = "Goods";

       

        public override void OnClientHooked() {
           
        }

        public override void OnClientFreed() {
           
        }
    }
}

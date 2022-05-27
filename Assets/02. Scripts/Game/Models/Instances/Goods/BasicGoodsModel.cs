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

        [field: SyncVar, SerializeField]
        public override string Name { get; set; } = "Goods";

       

        public override void OnClientHooked() {
           
        }

        public override void OnClientFreed() {
           
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class StoneShieldModel : BasicGoodsModel
    {
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
    }
}

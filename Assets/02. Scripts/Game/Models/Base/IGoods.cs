using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IGoods :  IModel{
        public int BasicSellPrice { get; set; }
   
        public int BasicBuyPrice { get; set; }
       
        public GoodsRarity GoodRarity { get; set; }

        public bool TransactionFinished { get; set; }

        int RealPrice { get; set; }

        /// <summary>
        /// true if is being sold; false if is being demanded
        /// </summary>
        bool IsSell { get; set; }
    }

}

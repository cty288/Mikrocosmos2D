using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public abstract class AbstractGoodsModel : AbstractBasicEntityModel, IGoods {
        [field:  SerializeField]
        public int BasicSellPrice { get; set; }
        [field: SerializeField]
        public int BasicBuyPrice { get; set; }
        [field: SerializeField]
        public GoodsRarity GoodRarity { get; set; }

        [field: SyncVar] public bool Sold { get; set; } = false;
    }
}

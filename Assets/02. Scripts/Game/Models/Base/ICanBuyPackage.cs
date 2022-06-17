using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ICanBuyPackage: IModel {
        List<GoodsConfigure> GetBuyItemsWithRarity(GoodsRarity rarity);
        SyncList<GoodsConfigure> BuyItemList { get; }
    }
}

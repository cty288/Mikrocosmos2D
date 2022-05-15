using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ICanSellPackage : IModel {
        SyncList<GoodsConfigure> SellItemList { get; }
    }
}

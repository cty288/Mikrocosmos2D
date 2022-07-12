using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IGoodsViewController : IController
    {
        public Transform FollowingPoint { get; set; }
        public IGoods GoodsModel { get; }

        public void TryAbsorb(IPlayerInventorySystem invneInventorySystem, GameObject absorbedTarget);
    }

    public interface ICanBeUsedGoodsViewController : IGoodsViewController {
        ICanBeUsed GoodsModel { get; }

        
    }
}

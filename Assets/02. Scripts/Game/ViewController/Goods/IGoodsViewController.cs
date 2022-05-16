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
    }
}

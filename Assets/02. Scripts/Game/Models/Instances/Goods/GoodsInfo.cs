using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using MikroFramework.Architecture;
using MikroFramework.DataStructures;
using UnityEngine;

namespace Mikrocosmos
{
    public enum GoodsRarity {
        RawResource,
        Secondary,
        Compound
    }


    [Serializable]
    public class GoodsConfigure {
        [field: SerializeField]
        public GameObject GoodPrefab { get;  set; }

        [field: SerializeField]
        public int RealPriceOffset { get;  set; }

        public IGoods Good {
            get {
                return GoodPrefab.GetComponent<IGoods>();
            }
        }

        public GoodsConfigure(){}
        public GoodsConfigure(GameObject goodPrefab) {
            this.GoodPrefab = goodPrefab;
        }
        public GoodsConfigure(GameObject goodPrefab, int overrideBuyPriceOffset, int overrideSellPriceOffset)
        {
            this.GoodPrefab = goodPrefab;
        }

       
    }
}

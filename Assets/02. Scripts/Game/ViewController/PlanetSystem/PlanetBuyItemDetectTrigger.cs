using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class PlanetBuyItemDetectTrigger : MonoBehaviour {
        private PlanetBuyBubble buyBubble;

        private void Awake() {
            buyBubble = GetComponent<PlanetBuyBubble>();
        }

        public IGoods GetGoods() {
            return buyBubble.ServerGoodsBuying;
        }


    }
}

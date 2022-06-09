using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class PlanetBuyItemDetectTrigger : MonoBehaviour {
        IPlanetTradingSystem planetTradingSystem;

        private void Awake() {
            planetTradingSystem = GetComponentInParent<IPlanetTradingSystem>();
        }

        public IGoods GetGoods() {
            return planetTradingSystem.ServerGetCurrentBuyItem();
        }


    }
}

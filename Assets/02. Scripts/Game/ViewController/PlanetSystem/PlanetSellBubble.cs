using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetSellBubble : MonoBehaviour {
        private Text priceText;

        public int Price;
        private void Awake() {
            priceText = transform.Find("SellPrice").GetComponent<Text>();
        }

        private void Start() {
            priceText.text = Price.ToString();
        }


        public void SetPrice(int pirce) {
            Price = pirce;
            priceText.text = Price.ToString();
        }
    }
}

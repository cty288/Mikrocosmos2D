using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetSellBubble : MonoBehaviour {
        private Text priceText;
        private Text rawText;
        public int Price;
        private void Awake() {
            priceText = transform.Find("SellPrice").GetComponent<Text>();
            Transform rawMaterialTextTr = transform.Find("RawMaterialText");
            if (rawMaterialTextTr)
            {
                rawText = rawMaterialTextTr.GetComponent<Text>();
            }
        }

        private void Start() {
            priceText.text = Price.ToString();
        }


        public void SetPrice(int pirce, bool isRaw = false) {
            Price = pirce;
            priceText.text = Price.ToString();
            if (isRaw && rawText)
            {
                rawText.gameObject.SetActive(true);
            }
            else if (!isRaw & rawText)
            {
                rawText.gameObject.SetActive(false);
            }
        }
    }
}

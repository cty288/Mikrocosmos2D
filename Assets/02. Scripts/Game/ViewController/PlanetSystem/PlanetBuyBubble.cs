using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetBuyBubble : MonoBehaviour {
        private Text priceText;
        private Text rawText;
        public int Price;
        public float Time;

        public IGoods ServerGoodsBuying;
        private void Awake()
        {
            priceText = transform.Find("BuyPrice").GetComponent<Text>();
            Transform rawMaterialTextTr = transform.Find("RawMaterialText");
            if (rawMaterialTextTr) {
                rawText = rawMaterialTextTr.GetComponent<Text>();
            }
        }

        public void UpdateInfo(int price, float time, bool isRaw = false)
        {
            Price = price;
            priceText.text = Price.ToString();
            this.Time = time;
            if (isRaw && rawText) {
                rawText.gameObject.SetActive(true);
            } else if (!isRaw & rawText) {
                rawText.gameObject.SetActive(false);
            }
        }


        private void Start() {
            priceText.text = Price.ToString();
        }
    }
}

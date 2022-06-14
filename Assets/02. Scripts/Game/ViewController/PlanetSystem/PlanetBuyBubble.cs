using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetBuyBubble : MonoBehaviour {
        private Text priceText;
        public int Price;
        public float Time;

        public IGoods ServerGoodsBuying;
        private void Awake()
        {
            priceText = transform.Find("BuyPrice").GetComponent<Text>();
        }

        public void UpdateInfo(int price, float time)
        {
            Price = price;
            priceText.text = Price.ToString();
            this.Time = time;
        }


        private void Start() {
            priceText.text = Price.ToString();
        }
    }
}

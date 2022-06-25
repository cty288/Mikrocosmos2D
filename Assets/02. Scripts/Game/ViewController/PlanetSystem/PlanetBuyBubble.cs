using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetBuyBubble : MonoBehaviour {
        private Text priceText;
        private Image itemTypeImage;
        public int Price;
        public float Time;

        public IGoods ServerGoodsBuying;

        [SerializeField] private Sprite[] itemTypeSprites;
        private void Awake()
        {
            priceText = transform.Find("BuyPrice").GetComponent<Text>();
            Transform itemTypeImageTr = transform.Find("ItemTypeImage");
            if (itemTypeImageTr) {
                itemTypeImage = itemTypeImageTr.GetComponent<Image>();
            }
        }

        public void UpdateInfo(int price, float time, bool isRaw = false)
        {
            Price = price;
            priceText.text = Price.ToString();
            this.Time = time;
            if (itemTypeImage) {
                itemTypeImage.enabled = true;
                if (isRaw) {
                    itemTypeImage.sprite = itemTypeSprites[0];
                }
                else if (!isRaw) {
                    itemTypeImage.sprite = itemTypeSprites[1];
                }
            }else{
                itemTypeImage.enabled = false;
            }
           
        }


        private void Start() {
            priceText.text = Price.ToString();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetBuyBubble : MonoBehaviour {
        private Text priceText;
        [SerializeField]
        private Image[] itemTypeImages;
        public int Price;
        public float Time;

        public IGoods ServerGoodsBuying;

        [SerializeField] private Sprite[] itemTypeSprites;
        private void Awake()
        {
            priceText = transform.Find("BuyPrice").GetComponent<Text>();
        }

        public void UpdateInfo(int price, float time, bool isRaw = false)
        {
            Price = price;
            priceText.text = Price.ToString();
            this.Time = time;
            if (itemTypeImages.Length>0) {
                foreach (var itemTypeImage in itemTypeImages) {
                    itemTypeImage.enabled = true;
                    if (isRaw)
                    {
                        itemTypeImage.sprite = itemTypeSprites[0];
                    }
                    else if (!isRaw)
                    {
                        itemTypeImage.sprite = itemTypeSprites[1];
                    }
                }
              
            }else{
                foreach (var itemTypeImage in itemTypeImages) {
                    itemTypeImage.enabled = false;
                }
              
            }
           
        }


        private void Start() {
            priceText.text = Price.ToString();
        }
    }
}

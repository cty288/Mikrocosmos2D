using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetSellBubble : AbstractMikroController<Mikrocosmos> {
        private Text priceText;
        [SerializeField]
        private Image[] itemTypeImages;
        public int Price;
        //private static readonly int Tint = Shader.PropertyToID("_Tint");
        [SerializeField] private Sprite[] itemTypeSprites;

        private Transform repeatSellRemainingTimeSliderParent;
        private Image repeatSellRemainingTimeSlider;

        public IGoods ServerGoodsSelling;
        public GameObject ServerGoodsObjectSelling;

        private float repeatSellMaxTime;
        private float repeatSellRemainingTime;
        
        private void Awake() {
            priceText = transform.Find("SellPrice").GetComponent<Text>();

            repeatSellRemainingTimeSliderParent = transform.Find("RepeatBuySliderBG");
            if (repeatSellRemainingTimeSliderParent) {
                repeatSellRemainingTimeSlider = repeatSellRemainingTimeSliderParent.Find("RepeatBuySlider")
                    .GetComponent<Image>();
            }
           

            this.RegisterEvent<OnClientMoneyChange>(OnClientMoneyChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

           
        }

        

        private void OnConnected(OnClientMainGamePlayerConnected e) {
            OnClientMoneyChange(new OnClientMoneyChange()
            {
                NewMoney = e.playerSpaceship
                    .GetComponent<IPlayerTradingSystem>().Money
            });
        }

        

        private void OnClientMoneyChange(OnClientMoneyChange e) {
            if (e.NewMoney >= Price) {
               
                priceText.color = Color.green;
            }
            else {

                priceText.color = Color.red;
            }
           
        }

        private void Start() {
            priceText.text = Price.ToString();
        }

        private void Update() {
            if (repeatSellMaxTime > 0 && repeatSellRemainingTimeSlider) {
                repeatSellRemainingTime -= Time.deltaTime;
                if (repeatSellRemainingTime >= 0) {
                    repeatSellRemainingTimeSlider.fillAmount =
                        0.34f + 0.31f * (repeatSellRemainingTime / repeatSellMaxTime);
                }
            }
        }


        public void SetInfo(int pirce, float repeatSellRemainingTime, bool isRaw = false) {
            Price = pirce;
            priceText.text = Price.ToString();

            if (itemTypeImages.Length > 0)
            {
                foreach (var itemTypeImage in itemTypeImages) {
                    itemTypeImage.enabled = true;
                    if (isRaw) {
                        itemTypeImage.sprite = itemTypeSprites[0];
                    }
                    else if (!isRaw) {
                        itemTypeImage.sprite = itemTypeSprites[1];
                    }
                }

            }
            else {
                foreach (var itemTypeImage in itemTypeImages) {
                    itemTypeImage.enabled = false;
                }
            }

            if (repeatSellRemainingTimeSliderParent) {
                if (repeatSellRemainingTime > 0) {
                    repeatSellRemainingTimeSliderParent.gameObject.SetActive(true);
                    if (this.repeatSellRemainingTime > 0) {
                        repeatSellRemainingTimeSlider.DOFillAmount(0.65f, 0.1f);
                    }
                    else {
                        repeatSellRemainingTimeSlider.fillAmount = 0.65f;
                    }
                  
                    repeatSellMaxTime = repeatSellRemainingTime;
                    this.repeatSellRemainingTime = repeatSellMaxTime;
                }
                else {
                    repeatSellRemainingTimeSliderParent.gameObject.SetActive(false);
                    repeatSellMaxTime = -1;
                }
            }
          

            if (NetworkClient.active && NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>()?.ControlledSpaceship)
            {
               
                OnClientMoneyChange(new OnClientMoneyChange()
                {
                    NewMoney = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>()
                        .ControlledSpaceship
                        .GetComponent<IPlayerTradingSystem>().Money
                });
                
            }
        }
    }
}

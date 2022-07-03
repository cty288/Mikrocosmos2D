using System;
using System.Collections;
using System.Collections.Generic;
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
        private void Awake() {
            priceText = transform.Find("SellPrice").GetComponent<Text>();
           

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
                priceText.material = Material.Instantiate(priceText.material);
                priceText.material.SetColor("_Tint", Color.green);
            }
            else {
                
                priceText.material = Material.Instantiate(priceText.material);
                priceText.material.SetColor("_Tint", Color.red);
            }
           
        }

        private void Start() {
            priceText.text = Price.ToString();
        }


        public void SetPrice(int pirce, bool isRaw = false) {
            Price = pirce;
            priceText.text = Price.ToString();

            if (itemTypeImages.Length > 0)
            {
                foreach (var itemTypeImage in itemTypeImages)
                {
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

            }
            else
            {
                foreach (var itemTypeImage in itemTypeImages)
                {
                    itemTypeImage.enabled = false;
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

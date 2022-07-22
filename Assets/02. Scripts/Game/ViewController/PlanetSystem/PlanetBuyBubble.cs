using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlanetBuyBubble : AbstractMikroController<Mikrocosmos> {
        private Text priceText;
        [SerializeField]
        private Image[] itemTypeImages;
        public int Price;
        public float Time;

        private string goodsName;
        private Animator animator;

        public IGoods ServerGoodsBuying;

        [SerializeField] private Sprite[] itemTypeSprites;

        private IHookSystem clientLocalPlayerHookSystem;

        private void Awake()
        {
            priceText = transform.Find("BuyPrice").GetComponent<Text>();
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            animator = GetComponent<Animator>();
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e) {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority) {
                RegisterClientHookSystem();
            }
        }

        private void RegisterClientHookSystem() {
            if (clientLocalPlayerHookSystem == null) {
                if (NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>()) {
                    clientLocalPlayerHookSystem = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>()
                        .ControlledSpaceship.GetComponent<IHookSystem>();
                    clientLocalPlayerHookSystem.ClientHookedItemName.RegisterWithInitValue(OnClientHookItemChanged)
                        .UnRegisterWhenGameObjectDestroyed(gameObject);
                }
            }
        }

        private void OnClientHookItemChanged(string oldName, string newName) {
            if (newName == goodsName) {
                animator.SetBool("IsTriggered", true);
            }
            else {
                animator.SetBool("IsTriggered", false);
            }
        }
        
        public void UpdateInfo(int price, float time, string goodsName, bool isRaw = false) {
            RegisterClientHookSystem();
            Price = price;
            priceText.text = Price.ToString();
            this.goodsName = goodsName;
            this.Time = time;
            if (clientLocalPlayerHookSystem != null) {
                OnClientHookItemChanged("", clientLocalPlayerHookSystem.ClientHookedItemName.Value);
            }
            
            if (itemTypeImages.Length>0) {
                foreach (var itemTypeImage in itemTypeImages) {
                    itemTypeImage.enabled = true;
                    if (isRaw) {
                        itemTypeImage.sprite = itemTypeSprites[0];
                    }
                    else if (!isRaw) {
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

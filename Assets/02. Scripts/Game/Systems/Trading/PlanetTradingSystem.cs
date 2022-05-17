using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public struct OnServerPlanetGenerateSellItem {
        public GameObject ParentPlanet;
        public int Price;
        public GameObject GeneratedItem;
    }

    public struct OnServerPlanetGenerateBuyingItem {
        public GameObject ParentPlanet;
        public int Price;
        public GameObject GeneratedItem;
    }
    public interface IPlanetTradingSystem : ISystem {

    }
    public class PlanetTradingSystem : AbstractNetworkedSystem, IPlanetTradingSystem {
        private IPlanetModel planetModel;

       [SerializeField, SyncVar]
        private float BuyItemMaxTime = 20;

        

        [SerializeField] private List<float> itemRarity = new List<float>() {
            0.5f,
            0.4f,
            0.1f
        };

        private float BuyItemTimer;

        private GoodsConfigure currentBuyingItemConfig;
        private IGoods currentBuyingItem;
        private GameObject currentBuyingItemObject;
        private int currentBuyingItemPrice;


        private GoodsConfigure currentSellingItemConfig;
        private IGoods currentSellingItem;
        private GameObject currentSellingItemObject;
        private int currentSellingItemPrice;

        private void Awake() {
            planetModel = GetComponent<IPlanetModel>();
            BuyItemTimer = Random.Range(1f, BuyItemMaxTime);
            
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnPlayerJoinGame)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerTrySellItem>(OnServerTrySellItem);
        }

        private void OnServerTrySellItem(OnServerTrySellItem e) {

            if (e.DemandedByPlanet == currentBuyingItem) {
                //need to check money first
                Debug.Log(e.HookedByIdentity.GetComponent<PlayerSpaceship>().name + "" +
                          $"Sold a {e.RequestingGoodsGameObject.name}");

                NetworkServer.Destroy(e.RequestingGoodsGameObject);
               SwitchBuyItem();
            }
        }


        //initialization
        private void OnPlayerJoinGame(OnNetworkedMainGamePlayerConnected obj) {
            if (currentSellingItem != null)
            {
                this.SendEvent<OnServerPlanetGenerateSellItem>(new OnServerPlanetGenerateSellItem()
                {
                    GeneratedItem = currentSellingItemObject,
                    ParentPlanet = this.gameObject,
                    Price = currentSellingItemPrice
                });
             
            }

            if (currentBuyingItem != null) {
                this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem()
                {
                    GeneratedItem = currentBuyingItemObject,
                    ParentPlanet = this.gameObject,
                    Price = currentBuyingItemPrice
                });
            }
        }

        private void Update() {
            if (isServer) {
                BuyItemTimer -= Time.deltaTime;
                if (BuyItemTimer <= 0) {
                    BuyItemTimer = Random.Range(BuyItemMaxTime - 5, BuyItemMaxTime + 5);
                    SwitchBuyItem();
                }

                if (currentSellingItem==null || currentSellingItem.TransactionFinished) {
                    SwitchSellItem();
                }
            }
        }

        
      
        [ServerCallback]
        private void SwitchSellItem()
        {
            
            List<GoodsConfigure> secondaryMaterials = planetModel.GetSellItemsWithRarity(GoodsRarity.Secondary);
            List<GoodsConfigure> targetList = secondaryMaterials;
            GoodsConfigure selectedGoodsConfigure = null;
            if (targetList.Count > 1)
            {
                while (selectedGoodsConfigure == null || selectedGoodsConfigure == currentSellingItemConfig)
                {
                    selectedGoodsConfigure = targetList[Random.Range(0, targetList.Count)];
                }
            }
            else
            {
                selectedGoodsConfigure = targetList[0];
            }

          

            currentSellingItemConfig = selectedGoodsConfigure;
            currentSellingItemObject = Instantiate(currentSellingItemConfig.GoodPrefab, transform.position,
                Quaternion.identity);

            NetworkServer.Spawn(currentSellingItemObject);
            currentSellingItem = currentSellingItemObject.GetComponent<IGoods>();
            currentSellingItem.IsSell = true;
            currentSellingItem.TransactionFinished = false;

            


            int basePrice = currentSellingItemConfig.RealPriceOffset + currentSellingItem.BasicBuyPrice;
            currentSellingItemPrice = Random.Range(basePrice - 5, basePrice + 5);
            currentSellingItem.RealPrice = currentSellingItemPrice;

            this.SendEvent<OnServerPlanetGenerateSellItem>(new OnServerPlanetGenerateSellItem()
            {
                GeneratedItem = currentSellingItemObject,
                ParentPlanet = this.gameObject,
                Price = currentSellingItemPrice
            });
        }





        [ServerCallback]
        private void SwitchBuyItem() {
            List<GoodsConfigure> rawMaterials = planetModel.GetBuyItemsWithRarity(GoodsRarity.RawResource);
            List<GoodsConfigure> secondaryMaterials = planetModel.GetBuyItemsWithRarity(GoodsRarity.Secondary);
            List<GoodsConfigure> compoundMaterials = planetModel.GetBuyItemsWithRarity(GoodsRarity.Compound);

            List<GoodsConfigure> targetList;
            if (rawMaterials.Any() && secondaryMaterials.Any() && compoundMaterials.Any()) { //get a resource with rarity
                int chance = Random.Range(0, 100);
                if (chance <= itemRarity[0]) {
                    targetList = rawMaterials;
                }else if (itemRarity[0] < chance && chance <= itemRarity[0] + itemRarity[1]) {
                    targetList = secondaryMaterials;
                }
                else {
                    targetList = compoundMaterials;
                }
            }
            else { //only get from raw resources
                targetList = rawMaterials;
            }

            GoodsConfigure selectedGoodsConfigure = null;
            if (targetList.Count > 1) {
                while (selectedGoodsConfigure==null || selectedGoodsConfigure== currentSellingItemConfig) {
                    selectedGoodsConfigure = targetList[Random.Range(0, targetList.Count)];
                }
            }
            else {
                selectedGoodsConfigure = targetList[0];
            }

            if (currentBuyingItemObject != null && !currentBuyingItem.TransactionFinished) {
                Destroy(currentBuyingItemObject);
            }

            currentBuyingItemConfig = selectedGoodsConfigure;
            currentBuyingItemObject = Instantiate(currentBuyingItemConfig.GoodPrefab, transform.position,
                Quaternion.identity);
            NetworkServer.Spawn(currentBuyingItemObject);
            currentBuyingItem = currentBuyingItemObject.GetComponent<IGoods>();
            currentBuyingItem.IsSell = false;
            currentBuyingItem.TransactionFinished = false;

            int basePrice = currentBuyingItemConfig.RealPriceOffset + currentBuyingItem.BasicBuyPrice;
            currentBuyingItemPrice = Random.Range(basePrice - 5, basePrice + 5);


            this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem() {
                GeneratedItem = currentBuyingItemObject,
                ParentPlanet = this.gameObject,
                Price = currentBuyingItemPrice
            });
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public struct OnClientPlanetAffinityWithTeam1Changed {
        public float NewAffinity;
        public NetworkIdentity PlanetIdentity;
    }
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

    public struct OnServerPlayerMoneyNotEnough {
        public NetworkIdentity PlayerIdentity;
    }
    public interface IPlanetTradingSystem : ISystem {
        /// <summary>
        /// A number between 0-1. 
        /// </summary>
        /// <param name="team">Team number. Either 1 or 2</param>
        /// <returns></returns>
        float GetAffinityWithTeam(int team);
    }
    public class PlanetTradingSystem : AbstractNetworkedSystem, IPlanetTradingSystem {
        private IPlanetModel planetModel;

        [SyncVar(hook = nameof(OnAffinityWithTeam1Changed))] 
        private float affinityWithTeam1 = 0.5f;

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
            this.RegisterEvent<OnServerTryBuyItem>(OnServerTryBuyItem);
        }

        //Planet SELL item, player BUY item
        private void OnServerTryBuyItem(OnServerTryBuyItem e) {
            if (e.RequestingGoods == currentSellingItem) {
                PlayerTradingSystem spaceship = e.HookedByIdentity.GetComponent<PlayerTradingSystem>();
                //Debug.Log($"Buy: {currentSellingItemPrice}, {currentSellingItemObject.name}");

                if (!currentSellingItem.TransactionFinished)
                {
                    spaceship.Money -= currentSellingItem.RealPrice;
                }

                currentSellingItem.TransactionFinished = true;
                currentSellingItem = null;

                ChangeAffinity(spaceship.GetComponent<PlayerSpaceship>().connectionToClient.identity
                    .GetComponent<NetworkMainGamePlayer>().matchInfo.Team);
            }
           
        }


        //Planet BUY item, player SELL item
        [ServerCallback]
        private void OnServerTrySellItem(OnServerTrySellItem e) {

            if (e.DemandedByPlanet == currentBuyingItem) {
               
              
                if (e.HookedByIdentity.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem spaceship)) {
                    

                    spaceship.Money += currentBuyingItem.RealPrice;
                    Debug.Log(currentBuyingItem.RealPrice);
                    NetworkServer.Destroy(e.RequestingGoodsGameObject);
                    SwitchBuyItem();

                    ChangeAffinity(e.HookedByIdentity.GetComponent<PlayerSpaceship>().connectionToClient.identity
                        .GetComponent<NetworkMainGamePlayer>().matchInfo.Team);

                    //this.SendEvent<OnServerPlayerMoneyNotEnough>(new OnServerPlayerMoneyNotEnough() {
                    // PlayerIdentity = e.HookedByIdentity
                    //});

                }
            }
        }

        [ServerCallback]
        private void ChangeAffinity(int teamNumber) {
            Debug.Log($"Team {teamNumber} completed a transaction");
            float affinityIncreasment = this.GetSystem<IGlobalTradingSystem>()
                .CalculateAffinityIncreasmentForOneTrade(GetAffinityWithTeam(teamNumber));
            if (teamNumber == 1) {
                affinityWithTeam1 += affinityIncreasment;
            }
            else {
                affinityWithTeam1 -= affinityIncreasment;
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
            currentSellingItemPrice = Random.Range(basePrice - 3, basePrice + 4);
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
            if (rawMaterials.Any() && secondaryMaterials.Any() && true){// compoundMaterials.Any()) { //get a resource with rarity
                int chance = Random.Range(0, 100);
                if (chance <= itemRarity[0]) {
                    targetList = rawMaterials;
                }else if (itemRarity[0] < chance && chance <= itemRarity[0] + itemRarity[1]) {
                    targetList = secondaryMaterials;
                }
                else {
                    targetList = secondaryMaterials;
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
                NetworkServer. Destroy(currentBuyingItemObject);
            }

            currentBuyingItemConfig = selectedGoodsConfigure;
            currentBuyingItemObject = Instantiate(currentBuyingItemConfig.GoodPrefab, transform.position,
                Quaternion.identity);
            NetworkServer.Spawn(currentBuyingItemObject);
            currentBuyingItem = currentBuyingItemObject.GetComponent<IGoods>();
            currentBuyingItem.IsSell = false;
            currentBuyingItem.TransactionFinished = false;

            int basePrice = currentBuyingItemConfig.RealPriceOffset + currentBuyingItem.BasicBuyPrice;
            currentBuyingItemPrice = Random.Range(basePrice - 3, basePrice + 4);
            currentBuyingItem.RealPrice = currentBuyingItemPrice;

            this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem() {
                GeneratedItem = currentBuyingItemObject,
                ParentPlanet = this.gameObject,
                Price = currentBuyingItemPrice
            });
        }

       /// <summary>
       /// Either server or client can call this function
       /// </summary>
       /// <param name="team"></param>
       /// <returns></returns>
        public float GetAffinityWithTeam(int team) {
            if (team == 1) {
                return affinityWithTeam1;
            }

            return 1 - affinityWithTeam1;
        }

       [ClientCallback]
       private void OnAffinityWithTeam1Changed(float oldAffinity, float newAffinity) {
            this.SendEvent<OnClientPlanetAffinityWithTeam1Changed>(new OnClientPlanetAffinityWithTeam1Changed() {
                NewAffinity = newAffinity,
                PlanetIdentity = netIdentity
            });
       }
    }
}

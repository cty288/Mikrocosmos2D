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
    public struct OnClientPlanetAffinityWithTeam1Changed
    {
        public float NewAffinity;
        public NetworkIdentity PlanetIdentity;
    }
    public struct OnServerPlanetGenerateSellItem
    {
        public GameObject ParentPlanet;
        public int Price;
        public GameObject GeneratedItem;
    }

    public struct OnServerPlanetGenerateBuyingItem
    {
        public GameObject ParentPlanet;
        public int Price;
        public GameObject GeneratedItem;
    }

    public struct OnServerPlayerMoneyNotEnough
    {
        public NetworkIdentity PlayerIdentity;
    }

    public struct OnServerTransactionFinished
    {
        public IGoods GoodsModel;
        public int Price;
        public bool IsSell;
    }


    public interface IPlanetTradingSystem : ISystem
    {
        /// <summary>
        /// A number between 0-1. 
        /// </summary>
        /// <param name="team">Team number. Either 1 or 2</param>
        /// <returns></returns>
        float GetAffinityWithTeam(int team);

        string CurrentBuyItemName { get; }

        float BuyItemTimer { get; }

        int BuyItemMaxTimeThisTime { get; }

        IGoods ServerGetCurrentBuyItem();
    }





    public class PlanetTradingSystem : AbstractNetworkedSystem, IPlanetTradingSystem
    {
        private IPlanetModel planetModel;

        [SyncVar(hook = nameof(OnAffinityWithTeam1Changed))]
        private float  affinityWithTeam1 = 0.5f;

        [SerializeField, SyncVar]
        private int BuyItemMaxTime = 20;



        [SerializeField]
        private List<float> itemRarity = new List<float>() {
            0.5f,
            0.4f,
            0.1f
        };

        [field: SyncVar]
        public float BuyItemTimer { get; protected set; }

        [field: SyncVar]
        public int BuyItemMaxTimeThisTime { get; protected set; }

        public IGoods ServerGetCurrentBuyItem() {
            return currentBuyingItem;
        }


        private GoodsConfigure currentBuyingItemConfig;
        private IGoods currentBuyingItem;
        private GameObject currentBuyingItemObject;
        private int currentBuyingItemPrice;


        private GoodsConfigure currentSellingItemConfig;
        private IGoods currentSellingItem;
        private GameObject currentSellingItemObject;
        private int currentSellingItemPrice;

        private void Awake()
        {
            planetModel = GetComponent<IPlanetModel>();
            BuyItemMaxTimeThisTime = Random.Range(1, BuyItemMaxTime + 1);
            BuyItemTimer = BuyItemMaxTimeThisTime;
        }


        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnPlayerJoinGame)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerTrySellItem>(OnServerTrySellItem);
            this.RegisterEvent<OnServerTryBuyItem>(OnServerTryBuyItem);
            SwitchBuyItem();
            SwitchSellItem();
            
        }

        //Planet SELL item, player BUY item
        private void OnServerTryBuyItem(OnServerTryBuyItem e)
        {
            if (e.RequestingGoods == currentSellingItem)
            {
                PlayerTradingSystem spaceship = e.HookedByIdentity.GetComponent<PlayerTradingSystem>();
                //Debug.Log($"Buy: {currentSellingItemPrice}, {currentSellingItemObject.name}");

                if (!currentSellingItem.TransactionFinished)
                {
                    spaceship.Money -= currentSellingItem.RealPrice;
                }

                currentSellingItem.TransactionFinished = true;
                this.SendEvent<OnServerTransactionFinished>(new OnServerTransactionFinished()
                {
                    GoodsModel = currentSellingItem,
                    IsSell = true,
                    Price = currentSellingItem.RealPrice
                });

                currentSellingItem = null;
                SwitchSellItem();
                ChangeAffinity(spaceship.GetComponent<PlayerSpaceship>().connectionToClient.identity
                    .GetComponent<NetworkMainGamePlayer>().matchInfo.Team);
            }

        }


        //Planet BUY item, player SELL item
        [ServerCallback]
        private void OnServerTrySellItem(OnServerTrySellItem e)
        {

            if (e.DemandedByPlanet == currentBuyingItem)
            {


                if (e.HookedByIdentity.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem spaceship))
                {


                    spaceship.Money += currentBuyingItem.RealPrice;
                    Debug.Log(currentBuyingItem.RealPrice);
                    NetworkServer.Destroy(e.RequestingGoodsGameObject);
                    this.SendEvent<OnServerTransactionFinished>(new OnServerTransactionFinished()
                    {
                        GoodsModel = currentBuyingItem,
                        IsSell = false,
                        Price = currentBuyingItem.RealPrice
                    });

                    //SwitchBuyItem();

                    ChangeAffinity(e.HookedByIdentity.GetComponent<PlayerSpaceship>().connectionToClient.identity
                        .GetComponent<NetworkMainGamePlayer>().matchInfo.Team);

                    //this.SendEvent<OnServerPlayerMoneyNotEnough>(new OnServerPlayerMoneyNotEnough() {
                    // PlayerIdentity = e.HookedByIdentity
                    //});

                }
            }
        }







        [ServerCallback]
        private void ChangeAffinity(int teamNumber)
        {
            Debug.Log($"Team {teamNumber} completed a transaction");
            float affinityIncreasment = this.GetSystem<IGlobalTradingSystem>()
                .CalculateAffinityIncreasmentForOneTrade(GetAffinityWithTeam(teamNumber));
            if (teamNumber == 1)
            {
                affinityWithTeam1 += affinityIncreasment;
            }
            else
            {
                affinityWithTeam1 -= affinityIncreasment;
            }

        }

        //initialization
        private void OnPlayerJoinGame(OnNetworkedMainGamePlayerConnected obj)
        {
            if (currentSellingItem != null)
            {
                this.SendEvent<OnServerPlanetGenerateSellItem>(new OnServerPlanetGenerateSellItem()
                {
                    GeneratedItem = currentSellingItemObject,
                    ParentPlanet = this.gameObject,
                    Price = currentSellingItemPrice
                });

            }

            if (currentBuyingItem != null)
            {
                this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem()
                {
                    GeneratedItem = currentBuyingItemObject,
                    ParentPlanet = this.gameObject,
                    Price = currentBuyingItemPrice
                });
            }
        }

        private void Update() {
            BuyItemTimer -= Time.deltaTime;
            if (BuyItemTimer <= 0)
            {
                BuyItemMaxTimeThisTime = Random.Range(BuyItemMaxTime - 5, BuyItemMaxTime + 5);
                BuyItemTimer = BuyItemMaxTimeThisTime;
                SwitchBuyItem();
            }
        }

       

        [ServerCallback]
        private void SwitchSellItem()
        {
            List<GoodsConfigure> rawMaterials = planetModel.GetSellItemsWithRarity(GoodsRarity.RawResource);
            List<GoodsConfigure> secondaryMaterials = planetModel.GetSellItemsWithRarity(GoodsRarity.Secondary);

            List<GoodsConfigure> targetList = secondaryMaterials;


            if (rawMaterials.Any() && secondaryMaterials.Any())
            {
                float chance = Random.Range(0f, 1f);
                if (chance <= itemRarity[0])
                {
                    targetList = rawMaterials;
                }
                else
                {
                    targetList = secondaryMaterials;
                }
            }
            else
            {
                targetList = secondaryMaterials;
            }

            GoodsConfigure selectedGoodsConfigure = null;

            if (targetList.Count > 1 || (rawMaterials.Any() && secondaryMaterials.Any())) {
                
                while (selectedGoodsConfigure == null || selectedGoodsConfigure == currentSellingItemConfig) {
                    if (targetList.Count > 1) {
                        selectedGoodsConfigure = targetList[Random.Range(0, targetList.Count)];
                    }
                    else {
                        targetList = targetList == rawMaterials ? secondaryMaterials : rawMaterials;
                        selectedGoodsConfigure = targetList[Random.Range(0, targetList.Count)];
                    }
                   
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




            int basePrice = currentSellingItemConfig.RealPriceOffset + currentSellingItem.BasicSellPrice;
            int offset = Mathf.RoundToInt(basePrice * 0.1f);
            currentSellingItemPrice = Random.Range(basePrice - offset, basePrice + offset + 1);
            currentSellingItemPrice = Mathf.Max(currentSellingItemPrice, 1);
            currentSellingItem.RealPrice = currentSellingItemPrice;

            this.SendEvent<OnServerPlanetGenerateSellItem>(new OnServerPlanetGenerateSellItem()
            {
                GeneratedItem = currentSellingItemObject,
                ParentPlanet = this.gameObject,
                Price = currentSellingItemPrice
            });
        }





        [ServerCallback]
        private void SwitchBuyItem()
        {
            List<GoodsConfigure> rawMaterials = planetModel.GetBuyItemsWithRarity(GoodsRarity.RawResource);
            List<GoodsConfigure> secondaryMaterials = planetModel.GetBuyItemsWithRarity(GoodsRarity.Secondary);
            List<GoodsConfigure> compoundMaterials = planetModel.GetBuyItemsWithRarity(GoodsRarity.Compound);

            List<GoodsConfigure> targetList;
            if (rawMaterials.Any() && secondaryMaterials.Any())
            {// compoundMaterials.Any()) { //get a resource with rarity
                float chance = Random.Range(0f, 1f);
                if (chance <= itemRarity[0])
                {
                    targetList = rawMaterials;
                }
                else if (itemRarity[0] < chance && chance <= itemRarity[0] + itemRarity[1])
                {
                    targetList = secondaryMaterials;
                }
                else
                {
                    targetList = secondaryMaterials;
                }
            }
            else
            { //only get from raw resources
              // targetList = rawMaterials;
                targetList = secondaryMaterials;
            }

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

            IGoods previousBuyingItem = null;
            GameObject previousBuyingItemGameObject = null;
            string previousBuyItemName = "";
            if (currentBuyingItemObject != null && !currentBuyingItem.TransactionFinished)
            {
                //NetworkServer.Destroy(currentBuyingItemObject);
                previousBuyingItem = currentBuyingItem;
                previousBuyingItemGameObject = currentBuyingItemObject;
                previousBuyItemName = currentBuyingItem.Name;
            }

            currentBuyingItemConfig = selectedGoodsConfigure;
            currentBuyingItemObject = Instantiate(currentBuyingItemConfig.GoodPrefab, transform.position,
                Quaternion.identity);
            NetworkServer.Spawn(currentBuyingItemObject);
            currentBuyingItem = currentBuyingItemObject.GetComponent<IGoods>();
            currentBuyingItem.IsSell = false;
            currentBuyingItem.TransactionFinished = false;

            int basePrice = currentBuyingItemConfig.RealPriceOffset + currentBuyingItem.BasicBuyPrice;
            int offset = Mathf.RoundToInt(basePrice * 0.1f);
            currentBuyingItemPrice = Random.Range(basePrice - offset, basePrice + offset + 1);
            currentBuyingItemPrice = Mathf.Max(1, currentBuyingItemPrice);
            currentBuyingItem.RealPrice = currentBuyingItemPrice;
            CurrentBuyItemName = currentBuyingItem.Name;

            this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem()
            {
                GeneratedItem = currentBuyingItemObject,
                ParentPlanet = this.gameObject,
                Price = currentBuyingItemPrice
            });

            RpcOnPlanetSwitchBuyItem(previousBuyItemName, currentBuyingItem.Name);

            if (previousBuyingItem != null)
            {
                NetworkServer.Destroy(previousBuyingItemGameObject);
            }
        }

        /// <summary>
        /// Either server or client can call this function
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public float GetAffinityWithTeam(int team)
        {
            if (team == 1)
            {
                return affinityWithTeam1;
            }

            return 1 - affinityWithTeam1;
        }

        [field: SyncVar]
        public string CurrentBuyItemName { get; protected set; }

        [ClientCallback]
        private void OnAffinityWithTeam1Changed(float oldAffinity, float newAffinity)
        {
            this.SendEvent<OnClientPlanetAffinityWithTeam1Changed>(new OnClientPlanetAffinityWithTeam1Changed()
            {
                NewAffinity = newAffinity,
                PlanetIdentity = netIdentity
            });
        }

        [ClientRpc]
        private void RpcOnPlanetSwitchBuyItem(string oldBuyItemName, string newBuyItemName)
        {
            this.SendEvent<OnClientPlanetSwitchBuyItem>(new OnClientPlanetSwitchBuyItem() {
                OldBuyItemName = oldBuyItemName,
                NewBuyItemName = newBuyItemName,
                TargetPlanet = gameObject
            });
        }
    }

    struct OnClientPlanetSwitchBuyItem {
        public string OldBuyItemName;
        public string NewBuyItemName;
        public GameObject TargetPlanet;
    }
}

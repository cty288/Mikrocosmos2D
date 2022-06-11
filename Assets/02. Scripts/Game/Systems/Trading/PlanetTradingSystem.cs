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
        public GameObject PreviousItem;
        public bool CountTowardsGlobalIItemList;
    }

    public struct OnServerPlanetGenerateBuyingItem
    {
        public GameObject ParentPlanet;
        public int Price;
        public GameObject GeneratedItem;
        public GameObject PreviousItem;
        public bool CountTowardsGlobalIItemList;
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


    public interface IPlanetTradingSystem : ISystem {
        /// <summary>
        /// A number between 0-1. 
        /// </summary>
        /// <param name="team">Team number. Either 1 or 2</param>
        /// <returns></returns>
        float GetAffinityWithTeam(int team);
        
       // IGoods ServerGetCurrentBuyItem();
    }

    public class TradingItemInfo {
        public GoodsConfigure currentItemConfig;
        public IGoods currentItem;
        public GameObject currentItemGameObject;
        public int currentItemPrice;
        public float buyTime;

        public TradingItemInfo(GoodsConfigure currentItemConfig, IGoods currentItem, GameObject currentItemGameObject, int currentItemPrice)
        {
            this.currentItemConfig = currentItemConfig;
            this.currentItem = currentItem;
            this.currentItemGameObject = currentItemGameObject;
            this.currentItemPrice = currentItemPrice;
        }
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

       

       

        [SyncVar, SerializeField] private int sellItemCount = 2;
        [SyncVar, SerializeField] private int buyItemCount = 1;


        private List<TradingItemInfo> currentBuyItemLists = new List<TradingItemInfo>();
        private List<TradingItemInfo> currentSellItemLists = new List<TradingItemInfo>();

     
        private void Awake()
        {
            planetModel = GetComponent<IPlanetModel>();

        }


        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnPlayerJoinGame)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerTrySellItem>(OnServerTrySellItem);
            this.RegisterEvent<OnServerTryBuyItem>(OnServerTryBuyItem);
            SwitchBuyItem(null, GoodsRarity.RawResource);
            SwitchBuyItem(null, GoodsRarity.Secondary);
            SwitchSellItem(null);
            
        }

        private bool CheckItemExistsInTradingItemList(List<TradingItemInfo> list, IGoods good, out TradingItemInfo info) {
            var l = list.Where(item => item.currentItem.Name == good.Name).ToList();
            if (l.Count > 0) {
                info = l.FirstOrDefault();
                return true;
            }

            info = null;
            return false;
        }
        
        //Planet SELL item, player BUY item
        private void OnServerTryBuyItem(OnServerTryBuyItem e)
        {
            if (CheckItemExistsInTradingItemList(currentSellItemLists, e.RequestingGoods, out TradingItemInfo info))
            {
                PlayerTradingSystem spaceship = e.HookedByIdentity.GetComponent<PlayerTradingSystem>();
                //Debug.Log($"Buy: {currentSellingItemPrice}, {currentSellingItemObject.name}");

                IGoods currentSellingItem = e.RequestingGoods;
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

                //currentSellItemLists.Remove(info);
                SwitchSellItem(info);
                ChangeAffinity(spaceship.GetComponent<PlayerSpaceship>().connectionToClient.identity
                    .GetComponent<NetworkMainGamePlayer>().matchInfo.Team);
            }

        }


        //Planet BUY item, player SELL item
        [ServerCallback]
        private void OnServerTrySellItem(OnServerTrySellItem e)
        {

            if (CheckItemExistsInTradingItemList(currentSellItemLists, e.DemandedByPlanet, out TradingItemInfo info)) {
                if (e.HookedByIdentity.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem spaceship)) {

                    spaceship.Money += info.currentItemPrice;
                    currentBuyItemLists.Remove(info);
                    NetworkServer.Destroy(e.RequestingGoodsGameObject);
                    this.SendEvent<OnServerTransactionFinished>(new OnServerTransactionFinished()
                    {
                        GoodsModel = info.currentItem,
                        IsSell = false,
                        Price = info.currentItemPrice
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
        private void OnPlayerJoinGame(OnNetworkedMainGamePlayerConnected e) {

            foreach (TradingItemInfo info in currentSellItemLists) {
                if (info.currentItem != null) {
                    this.SendEvent<OnServerPlanetGenerateSellItem>(new OnServerPlanetGenerateSellItem()
                    {
                        GeneratedItem = info.currentItemGameObject,
                        ParentPlanet = this.gameObject,
                        Price = info.currentItemPrice,
                        CountTowardsGlobalIItemList = false
                    });
                }
              
            }

            foreach (TradingItemInfo info in currentBuyItemLists)
            {
                if (info.currentItem != null)
                {
                    this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem()
                    {
                        GeneratedItem = info.currentItemGameObject,
                        ParentPlanet = this.gameObject,
                        Price = info.currentItemPrice,
                        CountTowardsGlobalIItemList = false
                    });
                }

            }
        }

        private void Update() {
            foreach (TradingItemInfo buyItem in currentBuyItemLists) {
                buyItem.buyTime -= Time.deltaTime;
            }

            currentBuyItemLists.Where((info => info.buyTime <= 0)).ToList()
                .ForEach((info => SwitchBuyItem(info, info.currentItem.GoodRarity)));
        }

       

        [ServerCallback]
        private void SwitchSellItem(TradingItemInfo switchedItem)
        {
            if (switchedItem != null) {
                currentSellItemLists.Remove(switchedItem);
            }


            if (currentSellItemLists.Count < sellItemCount) {
                List<GoodsConfigure> rawMaterials = planetModel.GetSellItemsWithRarity(GoodsRarity.RawResource);
                List<GoodsConfigure> secondaryMaterials = planetModel.GetSellItemsWithRarity(GoodsRarity.Secondary);

                List<GoodsConfigure> targetList = secondaryMaterials;


                if (rawMaterials.Any() && secondaryMaterials.Any()) {
                    float chance = Random.Range(0f, 1f);
                    if (chance <= itemRarity[0]) {
                        targetList = rawMaterials;
                    }
                    else {
                        targetList = secondaryMaterials;
                    }
                }
                else {
                    targetList = secondaryMaterials;
                }


                //Remove all items in targetList that are already in currentSellItemLists
                targetList.RemoveAll(item => currentSellItemLists.Any(item2 =>
                    item2.currentItem.Name == item.Good.Name || item.Good.Name == switchedItem?.currentItem.Name));

                if (targetList.Count == 0) {
                    targetList.Add(currentSellItemLists[Random.Range(0, currentSellItemLists.Count)].currentItemConfig);
                }



                GoodsConfigure selectedGoodsConfigure = null;
                selectedGoodsConfigure = this.GetSystem<IGlobalTradingSystem>().PlanetRequestSellItem(targetList);

                
                GameObject previousSellingItem = switchedItem?.currentItemGameObject;


                GoodsConfigure currentSellingItemConfig = selectedGoodsConfigure;
                GameObject currentSellingItemObject = Instantiate(currentSellingItemConfig.GoodPrefab, transform.position,
                    Quaternion.identity);
                NetworkServer.Spawn(currentSellingItemObject);
                IGoods currentSellingItem = currentSellingItemObject.GetComponent<IGoods>();
                currentSellingItem.IsSell = true;
                currentSellingItem.TransactionFinished = false;
                int basePrice = currentSellingItemConfig.RealPriceOffset + currentSellingItem.BasicSellPrice;
                int offset = Mathf.RoundToInt(basePrice * 0.1f);
                int currentSellingItemPrice = Random.Range(basePrice - offset, basePrice + offset + 1);
                currentSellingItemPrice = Mathf.Max(currentSellingItemPrice, 1);
                currentSellingItem.RealPrice = currentSellingItemPrice;
                currentSellItemLists.Add(new TradingItemInfo(currentSellingItemConfig, currentSellingItem,
                    currentSellingItemObject, currentSellingItemPrice));
                

                this.SendEvent<OnServerPlanetGenerateSellItem>(new OnServerPlanetGenerateSellItem()
                {
                    GeneratedItem = currentSellingItemObject,
                    ParentPlanet = this.gameObject,
                    Price = currentSellingItemPrice,
                    CountTowardsGlobalIItemList = true,
                    PreviousItem = previousSellingItem
                });
            }
            
        }





        [ServerCallback]
        private void SwitchBuyItem(TradingItemInfo switchedItem, GoodsRarity rarity)
        {
            if (switchedItem != null) {
                currentBuyItemLists.Remove(switchedItem);
            }


            List<GoodsConfigure> targetList = planetModel.GetBuyItemsWithRarity(rarity);
            GoodsRarity realRarity = rarity;
            if (targetList.Count == 0) {
                targetList = planetModel.GetBuyItemsWithRarity(GoodsRarity.Secondary);
                realRarity = GoodsRarity.Secondary;
            }

            if (realRarity != GoodsRarity.Compound) {
                targetList.RemoveAll(item => currentBuyItemLists.Any(item2 =>
                    item2.currentItem.Name == item.Good.Name || item.Good.Name == switchedItem?.currentItem.Name));
            }
            
            if (targetList.Count == 0) {
                var allItems =  planetModel.GetBuyItemsWithRarity(realRarity);
                targetList.Add(allItems[Random.Range(0, allItems.Count)]);
            }
            
            GoodsConfigure selectedGoodsConfigure = null;
            selectedGoodsConfigure = this.GetSystem<IGlobalTradingSystem>().PlanetRequestBuyItem(targetList);

            
            IGoods previousBuyingItem = null;
            GameObject previousBuyingItemGameObject = null;
            string previousBuyItemName = "";
            
            if (switchedItem != null && !switchedItem.currentItem.TransactionFinished)
            {
                //NetworkServer.Destroy(currentBuyingItemObject);
                previousBuyingItem = switchedItem.currentItem;
                previousBuyingItemGameObject = switchedItem.currentItemGameObject;
                previousBuyItemName = switchedItem.currentItem.Name;
            }

            
            GoodsConfigure currentBuyingItemConfig = selectedGoodsConfigure;
            GameObject currentBuyingItemObject = Instantiate(currentBuyingItemConfig.GoodPrefab, transform.position,
                Quaternion.identity);
            NetworkServer.Spawn(currentBuyingItemObject);
            IGoods currentBuyingItem = currentBuyingItemObject.GetComponent<IGoods>();
            currentBuyingItem.IsSell = false;
            currentBuyingItem.TransactionFinished = false;

            int basePrice = currentBuyingItemConfig.RealPriceOffset + currentBuyingItem.BasicBuyPrice;
            int offset = Mathf.RoundToInt(basePrice * 0.1f);
            int currentBuyingItemPrice = Random.Range(basePrice - offset, basePrice + offset + 1);
            currentBuyingItemPrice = Mathf.Max(1, currentBuyingItemPrice);
            currentBuyingItem.RealPrice = currentBuyingItemPrice;
            int buyTime = Random.Range(BuyItemMaxTime - 5, BuyItemMaxTime + 6);
            currentBuyItemLists.Add(new TradingItemInfo(currentBuyingItemConfig, currentBuyingItem, currentBuyingItemObject, currentBuyingItemPrice) {
                buyTime = buyTime
            });

            this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem()
            {
                GeneratedItem = currentBuyingItemObject,
                ParentPlanet = this.gameObject,
                Price = currentBuyingItemPrice,
                CountTowardsGlobalIItemList = true,
                PreviousItem = previousBuyingItemGameObject
            });

            RpcOnPlanetSwitchBuyItem(previousBuyItemName, currentBuyingItem.Name, buyTime );

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
        private void RpcOnPlanetSwitchBuyItem(string oldBuyItemName, string newBuyItemName, int maxBuyTime)
        {
            this.SendEvent<OnClientPlanetSwitchBuyItem>(new OnClientPlanetSwitchBuyItem() {
                OldBuyItemName = oldBuyItemName,
                NewBuyItemName = newBuyItemName,
                TargetPlanet = gameObject,
                MaxBuyTime = maxBuyTime
            });
        }
    }

    struct OnClientPlanetSwitchBuyItem {
        public string OldBuyItemName;
        public string NewBuyItemName;
        public GameObject TargetPlanet;
        public int MaxBuyTime;
    }
}

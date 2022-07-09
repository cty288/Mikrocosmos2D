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
        public float MaxTime;
    }

    public struct OnServerPlanetDestroySellItem {
        public GameObject ParentPlanet;
        public GameObject Item;
    }

    public struct OnServerPlanetDestroyBuyItem
    {
        public GameObject ParentPlanet;
        public GameObject Item;
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
        public GameObject Planet;
        public int TeamNumber;
    }


    public interface IPlanetTradingSystem : ISystem {
        /// <summary>
        /// A number between 0-1. 
        /// </summary>
        /// <param name="team">Team number. Either 1 or 2</param>
        /// <returns></returns>
        float GetAffinityWithTeam(int team);

        void SwitchBuyItem(TradingItemInfo switchedItem, GoodsRarity rarity);

        void SwitchSellItem(TradingItemInfo switchedItem, bool triggerCountdown);

        void DestroyBuyItem(int index);

        // IGoods ServerGetCurrentBuyItem();
    }

    [Serializable]
    public class TradingItemInfo {
        public GoodsConfigure currentItemConfig;
        public IGoods currentItem;
        public GameObject currentItemGameObject;
        public int currentItemPrice;
        public float buyTime;
        public bool sellTimeCountdownTriggered;
        public float sellTimeCountdown;

        public TradingItemInfo(GoodsConfigure currentItemConfig, IGoods currentItem, GameObject currentItemGameObject, int currentItemPrice, bool sellTimeCountdownTriggered = false, float sellTimeCountdown = 3f)
        {
            this.currentItemConfig = currentItemConfig;
            this.currentItem = currentItem;
            this.currentItemGameObject = currentItemGameObject;
            this.currentItemPrice = currentItemPrice;
            this.sellTimeCountdownTriggered = false;
            this.sellTimeCountdown = sellTimeCountdown;
            this.sellTimeCountdownTriggered = sellTimeCountdownTriggered;
        }
    }
    public class PlanetTradingSystem : AbstractNetworkedSystem, IPlanetTradingSystem
    {
        private ICanBuyPackage buyPackageModel;
        private ICanSellPackage sellPackageModel;

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

       

       

        [SyncVar, SerializeField] private int sellItemCount = 1;
        [SyncVar, SerializeField] private int buyItemCount = 2;
        [SerializeField] private float sellItemMaxCountdown = 3f;

        [SerializeField]
        private List<TradingItemInfo> currentBuyItemLists = new List<TradingItemInfo>();

        [SerializeField]
        private List<TradingItemInfo> currentSellItemLists = new List<TradingItemInfo>();

     
        private void Awake()
        {
            buyPackageModel = GetComponent<ICanBuyPackage>();
            sellPackageModel = GetComponent<ICanSellPackage>();

        }


        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnPlayerJoinGame)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerTrySellItem>(OnServerTrySellItem);
            this.RegisterEvent<OnServerTryBuyItem>(OnServerTryBuyItem);

            for (int i = 0; i < buyItemCount; i++) {
                SwitchBuyItem(null, (GoodsRarity)(i%2));
            }

            for (int i = 0; i < sellItemCount; i++) {
                SwitchSellItem(null, false);
            }
          
            
        }

        private bool CheckItemExistsInTradingItemList(List<TradingItemInfo> list, IGoods good, out TradingItemInfo info) {
            var l = list.Where(item => item.currentItem == good).ToList();
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
                
                IPlayerTradingSystem spaceship = e.HookedByIdentity.GetComponent<IPlayerTradingSystem>();
                //Debug.Log($"Buy: {currentSellingItemPrice}, {currentSellingItemObject.name}");
                int playerTeam = e.HookedByIdentity.GetComponent<PlayerSpaceship>().connectionToClient.identity
                    .GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
                
                IGoods currentSellingItem = e.RequestingGoods;
                if (!currentSellingItem.TransactionFinished)
                {
                    spaceship.SpendMoney(currentSellingItem.RealPrice);
                }

                currentSellingItem.TransactionFinished = true;
                this.SendEvent<OnServerTransactionFinished>(new OnServerTransactionFinished()
                {
                    GoodsModel = currentSellingItem,
                    IsSell = true,
                    Price = currentSellingItem.RealPrice,
                    Planet = gameObject,
                    TeamNumber = playerTeam
                });

                currentSellItemLists.Remove(info);
                SwitchSellItem(info, true);
                ChangeAffinity(playerTeam, e.HookedByIdentity.GetComponent<IBuffSystem>(), false, currentSellingItem.RealPrice);
                TargetOnBuyItemSuccess(e.HookedByIdentity.connectionToClient);
            }

        }


        //Planet BUY item, player SELL item
        [ServerCallback]
        private void OnServerTrySellItem(OnServerTrySellItem e)
        {

            if (CheckItemExistsInTradingItemList(currentBuyItemLists, e.DemandedByPlanet, out TradingItemInfo info)) {
                if (e.HookedByIdentity.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem spaceship)) {

                    spaceship.ReceiveMoney(info.currentItemPrice);
                    int playerTeam = e.HookedByIdentity.GetComponent<PlayerSpaceship>().connectionToClient.identity
                        .GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
                    // currentBuyItemLists.Remove(info);
                    NetworkServer.Destroy(e.RequestingGoodsGameObject);
                    this.SendEvent<OnServerTransactionFinished>(new OnServerTransactionFinished()
                    {
                        GoodsModel = info.currentItem,
                        IsSell = false,
                        Price = info.currentItemPrice,
                        Planet = gameObject,
                        TeamNumber = playerTeam
                    });

                    //SwitchBuyItem();

                    ChangeAffinity(playerTeam, e.HookedByIdentity.GetComponent<IBuffSystem>(), true, info.currentItemPrice);

                    //this.SendEvent<OnServerPlayerMoneyNotEnough>(new OnServerPlayerMoneyNotEnough() {
                    // PlayerIdentity = e.HookedByIdentity
                    //});
                    TargetOnSellItemSuccess(e.HookedByIdentity.connectionToClient);
                }
            }
        }







        [ServerCallback]
        private void ChangeAffinity(int teamNumber, IBuffSystem buffSystem, bool isPlanetBuy, int price)
        {
            Debug.Log($"Team {teamNumber} completed a transaction");
            float affinityIncreasment = this.GetSystem<IGlobalTradingSystem>()
                .CalculateAffinityIncreasmentForOneTrade(GetAffinityWithTeam(teamNumber), isPlanetBuy, price);

            if (buffSystem != null) {
                if (buffSystem.HasBuff<PermanentAffinityBuff>(out PermanentAffinityBuff affinityBuff)) {
                    affinityIncreasment += affinityIncreasment * affinityBuff.AdditionalAffinityAdditionPercentage * affinityBuff.CurrentLevel;
                }
                
            }
            
            
            if (teamNumber == 1) {
                float previousAffinity = affinityWithTeam1;
                affinityWithTeam1 = Mathf.Clamp(affinityWithTeam1 + affinityIncreasment, 0f, 1f);
                float realChangeAmount = affinityWithTeam1 - previousAffinity;
                
                this.SendEvent<OnServerAffinityWithTeam1Changed>(new OnServerAffinityWithTeam1Changed() {
                    ChangeAmount = realChangeAmount
                });
            }
            else {
                float previousAffinity = affinityWithTeam1;
                affinityWithTeam1 = Mathf.Clamp(affinityWithTeam1 - affinityIncreasment, 0f, 1f);
                float realChangeAmount = previousAffinity - affinityWithTeam1;

                this.SendEvent<OnServerAffinityWithTeam1Changed>(new OnServerAffinityWithTeam1Changed()
                {
                    ChangeAmount = -realChangeAmount
                });
            }

            affinityWithTeam1 = Mathf.Clamp(affinityWithTeam1, 0f, 1f);
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
                        CountTowardsGlobalIItemList = false,
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
                        CountTowardsGlobalIItemList = false,
                        MaxTime = info.buyTime
                    });
                    RpcOnPlanetGenerateBuyItem("", info.currentItem.Name, info.buyTime);
                }

            }
        }

        private void Update() {
            foreach (TradingItemInfo buyItem in currentBuyItemLists) {
                buyItem.buyTime -= Time.deltaTime;
            }

            foreach (TradingItemInfo info in currentSellItemLists) {
                if (info.sellTimeCountdownTriggered) {
                    info.sellTimeCountdown -= Time.deltaTime;
                    if (info.sellTimeCountdown <= 0) {
                        SwitchSellItem(info, false);
                        break;
                    }
                }
            }

            currentBuyItemLists.Where((info => info.buyTime <= 0)).ToList()
                .ForEach((info => SwitchBuyItem(info, info.currentItem.GoodRarity)));
        }

       

        [ServerCallback]
        public void SwitchSellItem(TradingItemInfo switchedItem, bool triggerCountdown)
        {
            if (switchedItem != null) {
                currentSellItemLists.Remove(switchedItem);
            }
            GoodsConfigure selectedGoodsConfigure = null;

            if (currentSellItemLists.Count < sellItemCount) {
              
                if (!triggerCountdown) {
                    List<GoodsConfigure> rawMaterials = sellPackageModel.GetSellItemsWithRarity(GoodsRarity.RawResource);
                    List<GoodsConfigure> secondaryMaterials = sellPackageModel.GetSellItemsWithRarity(GoodsRarity.Secondary);

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
                    else
                    {
                        targetList = rawMaterials;
                    }


                    //Remove all items in targetList that are already in currentSellItemLists
                    targetList.RemoveAll(item => currentSellItemLists.Any(item2 =>
                        item2.currentItem.Name == item.Good.Name || item.Good.Name == switchedItem?.currentItem.Name));

                    if (targetList.Count == 0)
                    {
                        targetList.Add(currentSellItemLists[Random.Range(0, currentSellItemLists.Count)].currentItemConfig);
                    }



                    

                    while (selectedGoodsConfigure == null || selectedGoodsConfigure.Good.Name == switchedItem?.currentItem.Name)
                    {
                        if (targetList.Count == 1)
                        {
                            selectedGoodsConfigure = targetList[0];
                            break;
                        }

                        selectedGoodsConfigure = this.GetSystem<IGlobalTradingSystem>().PlanetRequestSellItem(targetList);
                        targetList.Remove(selectedGoodsConfigure);
                    }
                }
                else {
                    selectedGoodsConfigure = switchedItem.currentItemConfig;
                }
               
                

            
              
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
                    currentSellingItemObject, currentSellingItemPrice, triggerCountdown));
                

                this.SendEvent<OnServerPlanetGenerateSellItem>(new OnServerPlanetGenerateSellItem()
                {
                    GeneratedItem = currentSellingItemObject,
                    ParentPlanet = this.gameObject,
                    Price = currentSellingItemPrice,
                    CountTowardsGlobalIItemList = true,
                    PreviousItem = previousSellingItem
                });

                if (switchedItem!=null && !switchedItem.currentItem.TransactionFinished) {
                    NetworkServer.Destroy(previousSellingItem);
                }
            }
            else if(switchedItem!=null){
                this.SendEvent<OnServerPlanetDestroySellItem>(new OnServerPlanetDestroySellItem() {
                    Item = switchedItem.currentItemGameObject,
                    ParentPlanet = gameObject
                });
            }
            
        }

        public void DestroyBuyItem(int index) {
            DestroyBuyItem(currentBuyItemLists[index]);
        }


        [ServerCallback]
        public void SwitchBuyItem(TradingItemInfo switchedItem, GoodsRarity rarity)
        {
            if (switchedItem != null) {
                currentBuyItemLists.Remove(switchedItem);
            }

            if (currentBuyItemLists.Count < buyItemCount) {
                List<GoodsConfigure> targetList = buyPackageModel.GetBuyItemsWithRarity(rarity);
                GoodsRarity realRarity = rarity;
                if (targetList.Count == 0)
                {
                    targetList = buyPackageModel.GetBuyItemsWithRarity(GoodsRarity.Secondary);
                    realRarity = GoodsRarity.Secondary;
                }

                if (realRarity != GoodsRarity.Compound)
                {
                    targetList.RemoveAll(item => currentBuyItemLists.Any(item2 =>
                        item2.currentItem.Name == item.Good.Name || item.Good.Name == switchedItem?.currentItem.Name));
                }

                if (targetList.Count == 0)
                {
                    var allItems = buyPackageModel.GetBuyItemsWithRarity(realRarity);
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

                if (switchedItem == null)
                {
                    buyTime = Random.Range(10, BuyItemMaxTime + 6);
                }
                currentBuyItemLists.Add(new TradingItemInfo(currentBuyingItemConfig, currentBuyingItem, currentBuyingItemObject, currentBuyingItemPrice)
                {
                    buyTime = buyTime
                });

                this.SendEvent<OnServerPlanetGenerateBuyingItem>(new OnServerPlanetGenerateBuyingItem()
                {
                    GeneratedItem = currentBuyingItemObject,
                    ParentPlanet = this.gameObject,
                    Price = currentBuyingItemPrice,
                    CountTowardsGlobalIItemList = true,
                    PreviousItem = previousBuyingItemGameObject,
                    MaxTime = buyTime
                });

                RpcOnPlanetGenerateBuyItem(previousBuyItemName, currentBuyingItem.Name, buyTime);

                if (previousBuyingItem != null)
                {
                    NetworkServer.Destroy(previousBuyingItemGameObject);
                }
            }
            else if(switchedItem!=null) {
                DestroyBuyItem(switchedItem);
            }
        }

        private void DestroyBuyItem(TradingItemInfo switchedItem) {
            if (currentBuyItemLists.Contains(switchedItem)) {
                currentBuyItemLists.Remove(switchedItem);
            }
          
            if (switchedItem.currentItemGameObject) {
                this.SendEvent<OnServerPlanetDestroyBuyItem>(new OnServerPlanetDestroyBuyItem()
                {
                    Item = switchedItem.currentItemGameObject,
                    ParentPlanet = gameObject
                });

                RpcOnPlanetGenerateBuyItem(switchedItem.currentItem.Name, "", 0);
                NetworkServer.Destroy(switchedItem.currentItemGameObject);
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
        private void RpcOnPlanetGenerateBuyItem(string oldBuyItemName, string newBuyItemName, float maxBuyTime) {
            this.SendEvent<OnClientPlanetGenerateBuyItem>(new OnClientPlanetGenerateBuyItem() {
                OldBuyItemName = oldBuyItemName,
                NewBuyItemName = newBuyItemName,
                TargetPlanet = gameObject,
                MaxBuyTime = maxBuyTime
            });
        }

        [TargetRpc]
        private void TargetOnBuyItemSuccess(NetworkConnection connection) {
            this.GetSystem<IAudioSystem>().PlaySound("Buy", SoundType.Sound2D);
        }

        [TargetRpc]
        private void TargetOnSellItemSuccess(NetworkConnection connection)
        {
            this.GetSystem<IAudioSystem>().PlaySound("Sell", SoundType.Sound2D);
        }

    }

    struct OnClientPlanetGenerateBuyItem {
        public string OldBuyItemName;
        public string NewBuyItemName;
        public GameObject TargetPlanet;
        public float MaxBuyTime;
        public GameObject PointerPrefab;
    }

    public struct OnServerAffinityWithTeam1Changed {
        public float ChangeAmount;
    }
}

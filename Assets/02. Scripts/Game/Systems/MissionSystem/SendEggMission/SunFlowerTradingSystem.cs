using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class SunFlowerTradingSystem : AbstractNetworkedSystem, IPlanetTradingSystem {
        private ICanBuyPackage buyPackageModel;
        
        private float affinityWithTeam1 = 0.5f;

        [SerializeField, SyncVar]
        private int BuyItemMaxTime = 120;



        


        [SyncVar, SerializeField] private int buyItemCount = 1;


        [SerializeField]
        private List<TradingItemInfo> currentBuyItemLists = new List<TradingItemInfo>();

       

        private void Awake() {
            buyPackageModel = GetComponent<ICanBuyPackage>();

        }


        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnPlayerJoinGame)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerTrySellItem>(OnServerTrySellItem);
            /*
            for (int i = 0; i < buyItemCount; i++) { 
                SwitchBuyItem(null, (GoodsRarity)(i % 2));
            }*/
        }

        private bool CheckItemExistsInTradingItemList(List<TradingItemInfo> list, IGoods good, out TradingItemInfo info)
        {
            var l = list.Where(item => item.currentItem == good).ToList();
            if (l.Count > 0)
            {
                info = l.FirstOrDefault();
                return true;
            }

            info = null;
            return false;
        }


        public void StartBuyItem() {
            if (isServer) {
                SwitchBuyItem(null, GoodsRarity.RawResource);
            }
            
        }
        //Planet BUY item, player SELL item
        [ServerCallback]
        private void OnServerTrySellItem(OnServerTrySellItem e)
        {

            if (CheckItemExistsInTradingItemList(currentBuyItemLists, e.DemandedByPlanet, out TradingItemInfo info))
            {
                if (e.HookedByIdentity.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem spaceship)) {
                    spaceship.ReceiveMoney(info.currentItemPrice);
                    // currentBuyItemLists.Remove(info);
                    int playerTeam = e.HookedByIdentity.GetComponent<PlayerSpaceship>().connectionToClient.identity
                        .GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
                    
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

                    ChangeAffinity(playerTeam, e.HookedByIdentity.GetComponent<IBuffSystem>());

                    //this.SendEvent<OnServerPlayerMoneyNotEnough>(new OnServerPlayerMoneyNotEnough() {
                    // PlayerIdentity = e.HookedByIdentity
                    //});
                    TargetOnSellItemSuccess(e.HookedByIdentity.connectionToClient);                    
                    DestroyBuyItem(info);
                }
            }
        }

        [ServerCallback]
        private void ChangeAffinity(int teamNumber, IBuffSystem buffSystem)
        {
            Debug.Log($"Team {teamNumber} completed a transaction");
            float affinityIncreasment = this.GetSystem<IGlobalTradingSystem>()
                .CalculateAffinityIncreasmentForOneTrade(GetAffinityWithTeam(teamNumber));

            if (buffSystem != null)
            {
                if (buffSystem.HasBuff<PermanentAffinityBuff>(out PermanentAffinityBuff affinityBuff)) {
                    affinityIncreasment += affinityIncreasment * affinityBuff.AdditionalAffinityAdditionPercentage * affinityBuff.CurrentLevel;
                }
            }

            if (teamNumber == 1)
            {
                this.SendEvent<OnServerAffinityWithTeam1Changed>(new OnServerAffinityWithTeam1Changed() {
                    ChangeAmount = affinityIncreasment
                });
                affinityWithTeam1 += affinityIncreasment;
            }
            else
            {
                affinityWithTeam1 -= affinityIncreasment;
                this.SendEvent<OnServerAffinityWithTeam1Changed>(new OnServerAffinityWithTeam1Changed() {
                    ChangeAmount = -affinityIncreasment
                });
            }

            affinityWithTeam1 = Mathf.Clamp(affinityWithTeam1, 0f, 1f);
        }

        //initialization
        private void OnPlayerJoinGame(OnNetworkedMainGamePlayerConnected e) {


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
        
        [ServerCallback]
        public void SwitchBuyItem(TradingItemInfo switchedItem, GoodsRarity rarity)
        {
            if (switchedItem != null)
            {
                currentBuyItemLists.Remove(switchedItem);
            }

            if (currentBuyItemLists.Count < buyItemCount)
            {
                List<GoodsConfigure> targetList = buyPackageModel.GetBuyItemsWithRarity(GoodsRarity.RawResource);
             

                GoodsConfigure selectedGoodsConfigure = null;
                selectedGoodsConfigure = this.GetSystem<IGlobalTradingSystem>().PlanetRequestBuyItem(targetList);


                IGoods previousBuyingItem = null;
                GameObject previousBuyingItemGameObject = null;
                string previousBuyItemName = "";

                if (switchedItem != null && !switchedItem.currentItem.TransactionFinished) {
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
                int currentBuyingItemPrice = basePrice;
                currentBuyingItemPrice = Mathf.Max(1, currentBuyingItemPrice);
                currentBuyingItem.RealPrice = currentBuyingItemPrice;

                int buyTime = BuyItemMaxTime;

            
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

                if (previousBuyingItem != null) {
                    NetworkServer.Destroy(previousBuyingItemGameObject);
                }
            }
            else if (switchedItem != null) {
                DestroyBuyItem(switchedItem);
            }
        }
        public void DestroyBuyItem(int index)
        {
            DestroyBuyItem(currentBuyItemLists[index]);
        }
        public void SwitchSellItem(TradingItemInfo switchedItem) {
            
        }

        private void DestroyBuyItem(TradingItemInfo switchedItem)
        {
            if (currentBuyItemLists.Contains(switchedItem))
            {
                currentBuyItemLists.Remove(switchedItem);
            }

            if (switchedItem.currentItemGameObject)
            {
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


        [SerializeField] private GameObject pointerPrefab;
        

        [ClientRpc]
        private void RpcOnPlanetGenerateBuyItem(string oldBuyItemName, string newBuyItemName, float maxBuyTime)
        {
            this.SendEvent<OnClientPlanetGenerateBuyItem>(new OnClientPlanetGenerateBuyItem()
            {
                OldBuyItemName = oldBuyItemName,
                NewBuyItemName = newBuyItemName,
                TargetPlanet = gameObject,
                MaxBuyTime = maxBuyTime,
                PointerPrefab = pointerPrefab
            });
        }

        [TargetRpc]
        private void TargetOnSellItemSuccess(NetworkConnection connection)
        {
            this.GetSystem<IAudioSystem>().PlaySound("Sell", SoundType.Sound2D);
        }
    }
}

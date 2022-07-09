using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public struct OnServerGoodsCraftSuccess {
        public IGoods Item1;
        public IGoods Item2;
        public GameObject CraftedResource;
    }
    [Serializable]
    public class CompoundResourceRecipe {
        public GameObject Item1;
        public GameObject Item2;
        public GameObject CraftedResource;

        public bool IsInRecipe(string name) {
            return Item1.name == name || Item2.name == name || Item1.GetComponent<IGoods>().Name == name ||
                   Item2.GetComponent<IGoods>().Name == name;
        }
    }

    
    public interface IGlobalTradingSystem : ISystem {
        AnimationCurve TradingCurve { get; set; }
        float CalculateAffinityIncreasmentForOneTrade(float currentAffinityPrecent, bool IsPlanetBuy, int price);

        float GetTotalAffinityWithTeam(int team);

        float MinimumCompositeSpeedForCraftingCompounds { get; }

        bool ServerRequestCraftGoods(IGoods item1, IGoods item2, Vector2 position);

        GoodsConfigure PlanetRequestBuyItem(List<GoodsConfigure> possibleGoods);

        GoodsConfigure PlanetRequestSellItem(List<GoodsConfigure> possibleGoods);

        List<GameObject> AllGoodsPrefabsInThisGame { get; }
    }


    public class GlobalTradingSystem : AbstractNetworkedSystem, IGlobalTradingSystem {
        private List<IPlanetTradingSystem> allPlanets;

        [SerializeField] private List<CompoundResourceRecipe> compoundResourceRecipes;

        [SerializeField] private GameObject craftingEffect;
        [SerializeField] private List<GameObject> allGoodsPrefabsInThisGame;

        public List<GameObject> AllGoodsPrefabsInThisGame => allGoodsPrefabsInThisGame;


        private Dictionary<string, int> allCirculatingBuyItems = new Dictionary<string, int>();

        private Dictionary<string, int> allCirculatingSellItems = new Dictionary<string, int>();

        private float totalAffinityWithTeam1;
        private float totalAffinityWithTeam2;

        private void Awake() {
            Mikrocosmos.Interface.RegisterSystem<IGlobalTradingSystem>(this);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            //register self to the system on the server

            List<IPlanetModel> planetModels = new List<IPlanetModel>();

            GameObject[] allPlanetObjects = GameObject.FindGameObjectsWithTag("Planet");
            
             allPlanetObjects.Select((o => o.GetComponent<IPlanetModel>())).ToList()
                    .ForEach(p => planetModels.Add(p));

             HashSet<GameObject> tempAllGoodsSet = new HashSet<GameObject>();
             
            foreach (var planet in planetModels) {
                foreach (var sellItem in planet.SellItemList) {
                    tempAllGoodsSet.Add(sellItem.GoodPrefab);
                }
            }

            allGoodsPrefabsInThisGame = tempAllGoodsSet.ToList();
             
             allPlanets = new List<IPlanetTradingSystem>();
             allPlanetObjects.Select((o => o.GetComponent<IPlanetTradingSystem>())).ToList()
                .ForEach(p => allPlanets.Add(p));

            this.RegisterEvent<OnServerPlanetGenerateSellItem>(OnPlanetGenerateSellItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnServerPlanetGenerateBuyingItem>(OnPlanetGenerateBuyItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnServerAffinityWithTeam1Changed>(OnAffinityWithTeam1Changed).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnAffinityWithTeam1Changed(OnServerAffinityWithTeam1Changed e) {
            totalAffinityWithTeam1 += e.ChangeAmount;
            totalAffinityWithTeam2 -= e.ChangeAmount;
        }

        private void OnPlanetGenerateBuyItem(OnServerPlanetGenerateBuyingItem e) {
            if (e.CountTowardsGlobalIItemList) {
                IGoods generatedItem = e.GeneratedItem.GetComponent<IGoods>();
                string generatedItemName = generatedItem.Name;
                if (allCirculatingBuyItems.ContainsKey(generatedItemName)) {
                    allCirculatingBuyItems[generatedItemName]++;
                }
                else {
                    allCirculatingBuyItems.Add(generatedItemName, 1);
                }

                if (e.PreviousItem) {
                    IGoods previousItem = e.GeneratedItem.GetComponent<IGoods>();
                    string previousItemName = previousItem.Name;
                    if (allCirculatingBuyItems.ContainsKey(previousItemName)) {
                        allCirculatingBuyItems[previousItemName]--;
                        if (allCirculatingBuyItems[previousItemName] <= 0) {
                            allCirculatingBuyItems.Remove(previousItemName);
                        }
                    }
                }
            }
        }

        private void OnPlanetGenerateSellItem(OnServerPlanetGenerateSellItem e) {
            if (e.CountTowardsGlobalIItemList)
            {
                IGoods generatedItem = e.GeneratedItem.GetComponent<IGoods>();
                string generatedItemName = generatedItem.Name;
                if (allCirculatingSellItems.ContainsKey(generatedItemName)) {
                    allCirculatingSellItems[generatedItemName]++;
                }
                else
                {
                    allCirculatingSellItems.Add(generatedItemName, 1);
                }

                if (e.PreviousItem)
                {
                    IGoods previousItem = e.GeneratedItem.GetComponent<IGoods>();
                    string previousItemName = previousItem.Name;
                    if (allCirculatingSellItems.ContainsKey(previousItemName))
                    {
                        allCirculatingSellItems[previousItemName]--;
                        if (allCirculatingSellItems[previousItemName] <= 0) {
                            allCirculatingSellItems.Remove(previousItemName);
                        }
                    }
                }
            }
        }

        [field: SerializeField][Tooltip("The Trading Curve Indicates the Marginal Affinity increasment vs." +
                                        "Current Affinity for each completed trade. ")]
        public AnimationCurve TradingCurve { get; set; }

        [SerializeField] private int standardPrice = 30;
        [ServerCallback]
        public float CalculateAffinityIncreasmentForOneTrade(float currentAffinityPrecent, bool IsPlanetBuy, int price) {
            float baseValue = TradingCurve.Evaluate(currentAffinityPrecent);
            baseValue =  Mathf.Clamp((price / (float) standardPrice), 0.2f, 2f) * baseValue;

            if (IsPlanetBuy) {
               // baseValue *= 1.5f;
            }
            return baseValue;
        }


        [ServerCallback]
        public float GetTotalAffinityWithTeam(int team) {
            if (team == 1) {
                return totalAffinityWithTeam1;
            }

            return totalAffinityWithTeam2;
        }

        /// <summary>
        /// Recommended Composite Speed Calculation: (item1.velocity - item2.velocity).magnitude 
        /// </summary>
        [field: SerializeField]
        public float MinimumCompositeSpeedForCraftingCompounds { get; protected set; } = 30;


        /// <summary>
        /// Request to craft items, only check if the two goods are in recipe, not checking if their speeds meet requirements, etc.
        /// Will also spawn a crafted resource if succeed, but will not delete original items if succeed
        /// Return if craft success or not
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        [ServerCallback]
        public bool ServerRequestCraftGoods(IGoods item1, IGoods item2, Vector2 position) {
            CompoundResourceRecipe targetRecipe = null;
            if (item1.Name == item2.Name) {
                return false;
            }
            foreach (CompoundResourceRecipe recipe in compoundResourceRecipes) {
                if (recipe.IsInRecipe(item1.Name) && recipe.IsInRecipe(item2.Name)) {
                    targetRecipe = recipe;
                    break;
                }
            }

            if (targetRecipe == null) {
                return false;
            }

            GameObject craftedResource = Instantiate(targetRecipe.CraftedResource, position, Quaternion.identity);
            NetworkServer.Spawn(craftedResource);
            this.SendEvent<OnServerGoodsCraftSuccess>(new OnServerGoodsCraftSuccess() {
                CraftedResource = craftedResource,
                Item1 =  item1,
                Item2 = item2
            });
            RpcSpawnCraftingEffect(position);
            return true;
        }

        public GoodsConfigure PlanetRequestBuyItem(List<GoodsConfigure> possibleGoods) {
            //find item in possibleGoods that doesn't exist in allCirculatingBuyItems
            List<GoodsConfigure> possibleGoodsNotInCirculating = possibleGoods.Where(g => !allCirculatingBuyItems.ContainsKey(g.Good.Name)).ToList();
            //if possibleGoodsNotInCirculating is not empty, randomly return one; otherwise, just randomly select one from possibleGoods
            if (possibleGoodsNotInCirculating.Count > 0) {
                return possibleGoodsNotInCirculating[Random.Range(0, possibleGoodsNotInCirculating.Count)];
            }
            else
            {
                return possibleGoods[Random.Range(0, possibleGoods.Count)];
            }
        }

        public GoodsConfigure PlanetRequestSellItem(List<GoodsConfigure> possibleGoods) {
            //find item in possibleGoods that doesn't exist in allCirculatingBuyItems
            List<GoodsConfigure> possibleGoodsNotInCirculating = possibleGoods.Where(g => !allCirculatingSellItems.ContainsKey(g.Good.Name)).ToList();
            //if possibleGoodsNotInCirculating is not empty, randomly return one; otherwise, just randomly select one from possibleGoods
            if (possibleGoodsNotInCirculating.Count > 0) {
                return possibleGoodsNotInCirculating[Random.Range(0, possibleGoodsNotInCirculating.Count)];
            }
            else {
                return possibleGoods[Random.Range(0, possibleGoods.Count)];
            }
        }


        [ClientRpc]
        private void RpcSpawnCraftingEffect(Vector2 position) {
            GameObject.Instantiate(craftingEffect, position, Quaternion.identity);
        }
    }
}

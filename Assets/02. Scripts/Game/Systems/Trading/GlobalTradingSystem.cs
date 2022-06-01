using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

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
        float CalculateAffinityIncreasmentForOneTrade(float currentAffinityPrecent);

        float GetTotalAffinityWithTeam(int team);

        float MinimumCompositeSpeedForCraftingCompounds { get; }

        bool ServerRequestCraftGoods(IGoods item1, IGoods item2, Vector2 position);
    }
    public class GlobalTradingSystem : AbstractNetworkedSystem, IGlobalTradingSystem {
        private List<IPlanetTradingSystem> allPlanets;

        [SerializeField] private List<CompoundResourceRecipe> compoundResourceRecipes;

        [SerializeField] private GameObject craftingEffect;

        private void Awake() {
            Mikrocosmos.Interface.RegisterSystem<IGlobalTradingSystem>(this);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            //register self to the system on the server
            

            allPlanets = new List<IPlanetTradingSystem>();
            GameObject.FindGameObjectsWithTag("Planet").Select((o => o.GetComponent<IPlanetTradingSystem>())).ToList()
                .ForEach(p => allPlanets.Add(p));
        }

        [field: SerializeField][Tooltip("The Trading Curve Indicates the Marginal Affinity increasment vs." +
                                        "Current Affinity for each completed trade. ")]
        public AnimationCurve TradingCurve { get; set; }

        [ServerCallback]
        public float CalculateAffinityIncreasmentForOneTrade(float currentAffinityPrecent) {
            return TradingCurve.Evaluate(currentAffinityPrecent);
        }


        [ServerCallback]
        public float GetTotalAffinityWithTeam(int team) {
            return allPlanets.Sum(p => p.GetAffinityWithTeam(team));
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


        [ClientRpc]
        private void RpcSpawnCraftingEffect(Vector2 position) {
            GameObject.Instantiate(craftingEffect, position, Quaternion.identity);
        }
    }
}

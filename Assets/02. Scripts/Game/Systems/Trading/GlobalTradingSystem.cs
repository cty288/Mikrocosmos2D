using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IGlobalTradingSystem : ISystem {
        AnimationCurve TradingCurve { get; set; }
        float CalculateAffinityIncreasmentForOneTrade(float currentAffinityPrecent);

        float GetTotalAffinityWithTeam(int team);
    }
    public class GlobalTradingSystem : AbstractNetworkedSystem, IGlobalTradingSystem {
        private List<IPlanetTradingSystem> allPlanets;
        public override void OnStartServer() {
            base.OnStartServer();
            //register self to the system on the server
            Mikrocosmos.Interface.RegisterSystem<IGlobalTradingSystem>(this);

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

        
    }
}

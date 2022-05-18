using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IGlobalTradingSystem : ISystem {
        AnimationCurve TradingCurve { get; set; }
        float CalculateAffinityIncreasmentForOneTrade(float currentAffinityPrecent);
    }
    public class GlobalTradingSystem : AbstractNetworkedSystem, IGlobalTradingSystem {
        
        public override void OnStartServer() {
            base.OnStartServer();
            //register self to the system on the server
            Mikrocosmos.Interface.RegisterSystem<IGlobalTradingSystem>(this);
        }

        [field: SerializeField][Tooltip("The Trading Curve Indicates the Marginal Affinity increasment vs." +
                                        "Current Affinity for each completed trade. ")]
        public AnimationCurve TradingCurve { get; set; }

        [ServerCallback]
        public float CalculateAffinityIncreasmentForOneTrade(float currentAffinityPrecent) {
            return TradingCurve.Evaluate(currentAffinityPrecent);
        }
    }
}

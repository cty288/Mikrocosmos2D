using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class ValveModel : BasicGoodsModel {
        [SerializeField] private int money = 50;
        private bool hasHooked = false;
        public override void OnServerHooked() {
            if (!hasHooked) {
                hasHooked = true;
                base.OnServerHooked();
                if (HookedByIdentity.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem playerTradingSystem))
                {
                    playerTradingSystem.ReceiveMoney(money);
                }
                UnHook(false);
            }
             
        }
    }
}

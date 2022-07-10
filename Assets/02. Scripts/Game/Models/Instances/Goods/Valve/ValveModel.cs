using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class ValveModel : BasicGoodsModel {
        [SerializeField] private int money = 50;
        public override void OnServerHooked()
        {
            base.OnServerHooked();
            if (HookedByIdentity.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem playerTradingSystem)) {
                playerTradingSystem.ReceiveMoney(money);
            }
            UnHook();
        }
    }
}

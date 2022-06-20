using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class PickaxeOreModel : BasicGoodsModel {
        protected override bool ServerCheckCanHook(NetworkIdentity hookedBy) {
            int money = this.SendQuery<int>(new ServerGetPlayerMoneyQuery(hookedBy));
            if (money - BasicBuyPrice < 0) {
                this.SendEvent<OnServerPlayerMoneyNotEnough>(new OnServerPlayerMoneyNotEnough()
                {
                    PlayerIdentity = hookedBy
                });
                return false;
            }
            else {
                if (hookedBy.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem playerTradingSystem)) {
                    playerTradingSystem.SpendMoney(BasicBuyPrice);
                }
                return true;
            }
        }

        public override void OnServerHooked() {
            base.OnServerHooked();
            UnHook();
        }
    }
}

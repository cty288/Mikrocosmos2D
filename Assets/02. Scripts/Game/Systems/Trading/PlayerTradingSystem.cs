using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos {

    public struct OnClientMoneyChange {
        public int OldMoney;
        public int NewMoney;
    }
    public interface IPlayerTradingSystem : ISystem {
        public int Money { get; set; }
    }

    public class PlayerTradingSystem : AbstractNetworkedSystem, IPlayerTradingSystem {
        [field: SyncVar(hook  = nameof(OnClientMoneyChange))]
        public int Money { get; set; } = 5000;

        [ClientCallback]
        private void OnClientMoneyChange(int oldMoney, int newMoney){
            if (hasAuthority) {
                this.SendEvent<OnClientMoneyChange>(new OnClientMoneyChange() {
                    OldMoney = oldMoney,
                    NewMoney = newMoney
                });
            }
        }
    }
}

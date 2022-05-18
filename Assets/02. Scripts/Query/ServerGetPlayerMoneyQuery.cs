using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class ServerGetPlayerMoneyQuery : AbstractQuery<int> {
        private NetworkIdentity _spaceshipIdentity;
        public ServerGetPlayerMoneyQuery(NetworkIdentity spaceshipIdentity) {
            this._spaceshipIdentity = spaceshipIdentity;
        }
        protected override int OnDo() {
            return _spaceshipIdentity.GetComponent<IPlayerTradingSystem>().Money;
        }
    }
}

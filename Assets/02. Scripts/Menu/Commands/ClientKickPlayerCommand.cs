using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public class ClientKickPlayerCommand : AbstractCommand<ClientKickPlayerCommand> {
        private int idToKick;

        public ClientKickPlayerCommand() {
        }

        public ClientKickPlayerCommand(int idToKick) {
            this.idToKick = idToKick;
        }
        protected override void OnExecute() {
             this.GetSystem<IRoomMatchSystem>().ClientKickPlayer(idToKick);
        }
    }
}

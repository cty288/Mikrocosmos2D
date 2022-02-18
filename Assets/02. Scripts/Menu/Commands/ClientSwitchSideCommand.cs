using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientRequestChangeTeam { }
    public class ClientSwitchSideCommand : AbstractCommand<ClientSwitchSideCommand> {
        protected override void OnExecute() {
            this.SendEvent<OnClientRequestChangeTeam>();
        }
    }
}

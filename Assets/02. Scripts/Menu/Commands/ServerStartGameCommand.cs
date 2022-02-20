using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnServerStartGame {

    }
    public class ServerStartGameCommand : AbstractCommand<ServerStartGameCommand> {
        protected override void OnExecute() {
            NetworkRoomManager.singleton.ServerChangeScene(((NetworkRoomManager)NetworkRoomManager.singleton)
                .GameplayScene);
            this.SendEvent<OnServerStartGame>();
        }
    }
}

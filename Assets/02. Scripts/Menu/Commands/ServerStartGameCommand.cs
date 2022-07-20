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
        private GameMode gameMode;
        protected override void OnExecute() {
            // NetworkRoomManager.singleton.OnStartServer();
            ((NetworkedRoomManager) NetworkRoomManager.singleton).ServerChangeGameModeScene(gameMode);
            
            
          NetworkRoomManager.singleton.ServerChangeScene(((NetworkRoomManager)NetworkRoomManager.singleton)
                .GameplayScene);
            this.SendEvent<OnServerStartGame>();
        }

        public ServerStartGameCommand() {

        }

        public ServerStartGameCommand(GameMode gameMode) {
            this.gameMode = gameMode;
        }
    }
}

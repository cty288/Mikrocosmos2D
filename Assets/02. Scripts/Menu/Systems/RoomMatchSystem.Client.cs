using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos {
    public struct OnClientGameModeChanged {
        public GameMode NewGameMode;
    }

    public struct OnClientReadyToEnterGameplayScene{}

    public partial interface IRoomMatchSystem : ISystem {
        void ClientKickPlayer(int kickedId);
    }
    public partial class RoomMatchSystem : AbstractNetworkedSystem, IRoomMatchSystem {
        [SerializeField]
        private PlayerMatchInfo clientSelfMatchInfo;
        public void ClientRecordMatchInfoCopy(PlayerMatchInfo matchInfo) {
            this.clientSelfMatchInfo = matchInfo.Clone();
        }

        public void ClientKickPlayer(int kickedId) {
            CmdRequestKickPlayer(kickedId, NetworkClient.localPlayer);
        }

     
        public PlayerMatchInfo ClientGetMatchInfoCopy() {
            return clientSelfMatchInfo;
        }

        public override void OnStartClient() {
            base.OnStartClient();
            if (isClientOnly) {
                this.GetSystem<ITimeSystem>().AddDelayTask(0.5f, () => {
                    this.SendEvent<OnClientGameModeChanged>(new OnClientGameModeChanged()
                    {
                        NewGameMode = gameMode
                    });
                });
            }
        }

        [ClientRpc]
        private void RpcOnGameModeChange(GameMode oldGameMode, GameMode newGameMode) {
            if (isClientOnly) {
                this.SendEvent<OnClientGameModeChanged>(new OnClientGameModeChanged() {
                    NewGameMode = newGameMode
                });
            }
           
        }

        [ClientRpc]
        private void RpcReadyToEnterGameScene() {
            //4 secs
            this.SendEvent<OnClientReadyToEnterGameplayScene>(new OnClientReadyToEnterGameplayScene());
        }
       
    }
}

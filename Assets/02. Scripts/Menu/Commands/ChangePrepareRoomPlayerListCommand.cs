using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientPrepareRoomPlayerListChange {
        public List<PlayerMatchInfo> MatchInfos;
        public PlayerMatchInfo SelfInfo;
        public bool IsHost;
    }
    public class ChangePrepareRoomPlayerListCommand : AbstractCommand<ChangePrepareRoomPlayerListCommand> {
        private List<PlayerMatchInfo> MatchInfos;
        private PlayerMatchInfo selfInfo;
        private bool isHost;

        public ChangePrepareRoomPlayerListCommand() {
        }

        public ChangePrepareRoomPlayerListCommand(List<PlayerMatchInfo> mathInfos, PlayerMatchInfo selfInfo, bool isHost) {
            MatchInfos = mathInfos;
            this.selfInfo = selfInfo;
            this.isHost = isHost;
        }
        protected override void OnExecute() {
            this.SendEvent<OnClientPrepareRoomPlayerListChange>(new OnClientPrepareRoomPlayerListChange() {
                MatchInfos = MatchInfos,
                SelfInfo =  selfInfo,
                IsHost = isHost
            });
        }
    }
}

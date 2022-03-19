using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
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


       
    }
}

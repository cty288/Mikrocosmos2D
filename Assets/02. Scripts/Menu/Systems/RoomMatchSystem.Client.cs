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
    public partial class RoomMatchSystem : AbstractNetworkedSystem, IRoomMatchSystem
    {
        public void ClientKickPlayer(int kickedId) {
            CmdRequestKickPlayer(kickedId, NetworkClient.localPlayer);
        }
    }
}

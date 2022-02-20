using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnStopNetworkDiscovery{

    }
    public class CmdRequestStopNetworkDiscoveryCommand : AbstractCommand<CmdRequestStopNetworkDiscoveryCommand>
    {
        protected override void OnExecute() {
            (NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).OnServerFound.RemoveAllListeners();
            (NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).StopDiscovery();
            this.SendEvent<OnStopNetworkDiscovery>();
        }
    }
}

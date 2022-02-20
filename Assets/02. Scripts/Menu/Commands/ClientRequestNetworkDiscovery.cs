using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnStartNetworkDiscovery {
        public MenuServerFoundUnityEvent FoundEvent;
    }
    public class ClientRequestNetworkDiscoveryCommand : AbstractCommand<ClientRequestNetworkDiscoveryCommand>
    {
        protected override void OnExecute() {
            (NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).OnServerFound.RemoveAllListeners();
            this.SendEvent<OnStartNetworkDiscovery>(new OnStartNetworkDiscovery()
                { FoundEvent = (NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).OnServerFound });
            
           // (NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).StartDiscovery();
            

        }
    }
}

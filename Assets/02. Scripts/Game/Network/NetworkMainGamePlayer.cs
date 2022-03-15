using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnNetworkedMainGamePlayerConnected {
        public NetworkConnection connection;
    }

    public struct OnClientMainGamePlayerConnected {
        public GameObject playerSpaceship;
    }

    public partial class NetworkMainGamePlayer : AbstractNetworkedController<Mikrocosmos>, ICanSendEvent {
        [SerializeField] private GameObject spaceshipPrefab;
        

       public PlayerMatchInfo matchInfo = null;

        [SyncVar]
        public NetworkIdentity ControlledSpaceship;
        public override void OnStartServer() {
            base.OnStartServer();
            //spawn spaceship
            GameObject spaceship =  Instantiate(spaceshipPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(spaceship, connectionToClient);
            ControlledSpaceship = spaceship.GetComponent<NetworkIdentity>();
            this.SendEvent<OnNetworkedMainGamePlayerConnected>();
        }

        public override void OnStartAuthority() {
            base.OnStartAuthority();
            this.SendEvent<OnClientMainGamePlayerConnected>(new OnClientMainGamePlayerConnected() {
                playerSpaceship =  ControlledSpaceship.gameObject
            });
        }
    }
}

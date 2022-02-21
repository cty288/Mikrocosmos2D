using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
   
    public partial class NetworkMainGamePlayer : AbstractNetworkedController<Mikrocosmos>, ICanSendEvent {
        [SerializeField] private GameObject spaceshipPrefab;

        [SyncVar]
        public NetworkIdentity ControlledSpaceship;
        public override void OnStartServer() {
            base.OnStartServer();
            //spawn spaceship
            GameObject spaceship =  Instantiate(spaceshipPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(spaceship, connectionToClient);
            ControlledSpaceship = spaceship.GetComponent<NetworkIdentity>();
        }

        public override void OnStartClient() {
            base.OnStartClient();
            if (isLocalPlayer && NetworkServer.active) {
                GetComponentInChildren<Camera>().depth = 1;
            }
            if (!hasAuthority && !NetworkServer.active) { 
                GetComponentInChildren<Camera>().gameObject.SetActive(false);
            }
        }
    }
}

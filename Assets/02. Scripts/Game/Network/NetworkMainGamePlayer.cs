using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class NetworkMainGamePlayer : AbstractNetworkedController<Mikrocosmos> {
        [SerializeField] private GameObject spaceshipPrefab;

        public override void OnStartServer() {
            base.OnStartServer();
            //spawn spaceship
            GameObject spaceship =  Instantiate(spaceshipPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(spaceship, connectionToClient);

        }
    }
}

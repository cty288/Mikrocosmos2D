using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class IgnoreCollisionWithPlayers : AbstractMikroController<Mikrocosmos> {
        private void Awake() {
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnPlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnPlayerConnected(OnNetworkedMainGamePlayerConnected e) {
            if (NetworkServer.active) {
                Collider2D collider = e.connection
                    .identity.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship
                    .GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collider);
            }
           
        }
    }
}

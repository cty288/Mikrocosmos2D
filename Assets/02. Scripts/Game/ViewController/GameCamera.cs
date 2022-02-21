using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class GameCamera : AbstractNetworkedController<Mikrocosmos> {
        [SerializeField]
        private GameObject following;

        [SerializeField] private float lerp = 0.1f;

        private GameObject cameraGo;

        public override void OnStartServer() {
            base.OnStartServer();
            cameraGo = GetComponentInChildren<Camera>().gameObject;
        }

        private void Update() {
            if (isServer) {
                if (!following) {
                    following = transform.parent.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship
                        .gameObject;
                }
            }
        }

        [ServerCallback]
        private void FixedUpdate() {
            
            if (NetworkServer.active && following) {
                cameraGo.transform.position = Vector3.Lerp(cameraGo.transform.position, new Vector3( following.transform.position.x, following.transform.position.y, -10), lerp);
            }
        }
    }
}

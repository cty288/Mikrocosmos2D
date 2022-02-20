using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class GameCamera : AbstractMikroController<Mikrocosmos> {
        [SerializeField]
        private GameObject following;

        [SerializeField] private float lerp = 0.1f;
        private void Awake() {
            this.RegisterEvent<OnLocalPlayerEnterGame>(OnInit).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnInit(OnLocalPlayerEnterGame obj) {
            if (NetworkClient.active) {
                following = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship
                    .gameObject;
            }
        }

        private void FixedUpdate() {
            
            
            if (NetworkClient.active && following) {
                transform.position = Vector3.Lerp(transform.position, new Vector3( following.transform.position.x, following.transform.position.y, -10), lerp);
            }
        }
    }
}

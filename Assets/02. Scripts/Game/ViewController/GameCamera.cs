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
        public GameObject following;

        [SerializeField] private float lerp = 0.1f;



        private void FixedUpdate() {
            
            if (following) {
                transform.position = Vector3.Slerp(transform.position, new Vector3( following.transform.position.x, following.transform.position.y, -10), lerp * Time.deltaTime);
            }
        }
    }
}

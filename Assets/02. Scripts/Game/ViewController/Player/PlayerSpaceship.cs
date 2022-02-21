using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship : AbstractNetworkedController<Mikrocosmos> {
        private Rigidbody2D rigidbody;

        private void Awake() {
            rigidbody = GetComponent<Rigidbody2D>();
        }



        [Command]
        private void CmdAddForce(Vector2 forceDirection) {
            if (rigidbody.velocity.sqrMagnitude <= Mathf.Pow(this.GetModel<ISpaceshipConfigurationModel>().MaxSpeed, 2)) {
                rigidbody.AddForce(forceDirection * this.GetModel<ISpaceshipConfigurationModel>().MoveForce);
            }

           
        }

        [Command]
        private void CmdOnUpdateRotation(Vector2 mousePos) {
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - mousePos;
            float angle = Mathf.Atan2(dir.y, dir.x) * (180 / Mathf.PI) + 90;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.1f);
        }
       
    }
}

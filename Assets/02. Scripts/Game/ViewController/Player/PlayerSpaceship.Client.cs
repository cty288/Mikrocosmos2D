using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship  : AbstractNetworkedController<Mikrocosmos> {
        [SerializeField]
        private bool isControlling = false;

        private void Update() {
            if (hasAuthority && isClient) {
                RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
               
                if (Input.GetMouseButtonDown(0)) {
                    isControlling = true;
                }
                if (Input.GetMouseButtonUp(0)) {
                    isControlling = false;
                }
            }

        }

        private void FixedUpdate() {
            if (hasAuthority && isClient) {
                Debug.Log("Hasauthority");
                if (isControlling) {
                    CmdAddForce((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
                }

                CmdOnUpdateRotation(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }

        public override void OnStartAuthority() {
            base.OnStartAuthority();
           // Camera.main.GetComponent<GameCamera>().following = this.gameObject;
        }
    }
}

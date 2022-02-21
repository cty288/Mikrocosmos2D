using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship  : AbstractNetworkedController<Mikrocosmos> {
        private bool isControlling = false;

        private void Update() {
            if (hasAuthority && isClient) {
                RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
               
                if (Input.GetMouseButtonDown(0)) {
                    isControlling = true;
                }
                

                if (isControlling) {
                    Debug.Log("IsControlling");
                    CmdAddForce((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
                }

                CmdOnUpdateRotation(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (Input.GetMouseButtonUp(0)) {
                    isControlling = false;
                }
            }

        }
    }
}

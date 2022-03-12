using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship : BasicEntityViewController<SpaceshipModel> {
        [SerializeField]
        private bool isControlling = false;
        private Rigidbody2D rigidbody;
    
        protected override void Awake() {
            base.Awake();
            rigidbody = GetComponent<Rigidbody2D>();
            this.RegisterEvent<OnMassChanged>(OnMassChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnMassChanged(OnMassChanged e)
        {
            Debug.Log(e.newMass);
        }

        private void Update() {
            if (hasAuthority && isClient) {
                RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
               
                if (Input.GetMouseButtonDown(0)) {
                    isControlling = true;
                }

                if (Input.GetMouseButtonDown(1))
                {
                    CmdChangeMoveForce(model.MoveForce+1);
                }
              
              

                if (Input.GetMouseButtonUp(0)) {
                    isControlling = false;
                }
            }

        }

        private void FixedUpdate() {
            if (hasAuthority && isClient) {
                //Debug.Log("Hasauthority");
                if (isControlling) {
                    //CmdAddForce((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
                    Vector2 forceDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)
                        .normalized;
                    if (rigidbody.velocity.sqrMagnitude <= Mathf.Pow(model.MaxSpeed, 2))
                    {
                        rigidbody.AddForce(forceDir * model.MoveForce);
                    }
                }

                UpdateRotation(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }

        private void UpdateRotation(Vector2 mousePos)
        {
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - mousePos;
            float angle = Mathf.Atan2(dir.y, dir.x) * (180 / Mathf.PI) + 90;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.2f);
        }

    }
}

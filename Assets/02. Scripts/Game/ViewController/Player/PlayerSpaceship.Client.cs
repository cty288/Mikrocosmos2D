using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship : BasicEntityViewController<SpaceshipModel> {
        [SerializeField]
        private bool isControlling = false;
      
        private Trigger2DCheck hookTrigger;

        protected override void Awake() {
            base.Awake();
          
            hookTrigger = GetComponentInChildren<Trigger2DCheck>();
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
                    if (Model.HookState == HookState.Freed) {
                        isControlling = true;
                    }
                    else {
                        isControlling  = false;
                    }
                }

                //take item & put item (not shoot)
                if (Input.GetKeyDown(KeyCode.Space)) {
                    CmdTryUseHook();
                }

                if (Input.GetMouseButtonDown(1)) {
                    CmdChangeMoveForce(Model.MoveForce+1);
                }
              
              

                if (Input.GetMouseButtonUp(0)) {
                    isControlling = false;
                }
            }

        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (isClient && !hasAuthority && Model.HookState == HookState.Freed) {
                if (Model.HookedItem != null) {
                    GetComponent<NetworkTransform>().syncPosition = false;
                }
                else {
                    GetComponent<NetworkTransform>().syncPosition = true;
                }
            }

            if (hasAuthority && isClient) {
                //Debug.Log("Hasauthority");
                if (isControlling) {
                    //CmdAddForce((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
                    Vector2 forceDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)
                        .normalized;
                    if (rigidbody.velocity.sqrMagnitude <= Mathf.Pow(Model.MaxSpeed, 2))
                    {
                        rigidbody.AddForce(forceDir * Model.MoveForce);
                    }
                }

                UpdateRotation(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
        //
        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player")) {
                float x = rigidbody.velocity.magnitude / Model.MaxSpeed;
                float normalizedForce = 0.8f * (1 - Mathf.Pow(1 - x, 2.6f));

               // rigidbody.AddForce( normalizedForce * -1f * rigidbody.velocity *  Model.Bounceness, ForceMode2D.Impulse);
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

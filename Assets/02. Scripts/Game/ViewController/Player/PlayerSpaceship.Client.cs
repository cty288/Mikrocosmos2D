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
            hookSystem = GetComponent<IHookSystem>();
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
            ClientUpdateSync();
            

            
          

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

        private void ClientUpdateSync() {
          
            if (isClient) {
                if (Model.HookState == HookState.Hooked) {
                    Transform hookedByTr = Model.ClientHookedByTransform;
                    if (hookedByTr) {
                        if (!hasAuthority)
                        {
                            GetComponent<NetworkTransform>().syncPosition = false;
                        }
                        rigidbody.MovePosition(Vector2.Lerp(transform.position, hookedByTr.position, 0.5f));
                        transform.rotation = hookedByTr.rotation;
                        // rigidbody.velocity = hookedByTr.parent.GetComponent<Rigidbody2D>().velocity;
                    }

                }
                else
                {
                    if (isClient && !hasAuthority && Model.HookState == HookState.Freed)
                    {
                        NetworkTransform nt = GetComponent<NetworkTransform>();
                        if (hookSystem.IsHooking) {
                            GetComponent<NetworkTransform>().syncPosition = true;
                        }
                        else
                        {
                            GetComponent<NetworkTransform>().syncPosition = true;
                        }
                    }
                    else
                    {
                        //if (!hasAuthority) {
                        GetComponent<NetworkTransform>().syncPosition = true;
                        //}
                    }
                }
            }
        }

        IEnumerator SyncHookerPos() {
            GetComponent<NetworkTransform>().syncPosition = true;
            yield return null;
            GetComponent<NetworkTransform>().syncPosition = false;
        }
        //

        private void UpdateRotation(Vector2 mousePos)
        {
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - mousePos;
            float angle = Mathf.Atan2(dir.y, dir.x) * (180 / Mathf.PI) + 90;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.2f);
        }

    }
}

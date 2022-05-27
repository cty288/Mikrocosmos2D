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
    public partial class PlayerSpaceship : BasicEntityViewController {
        [SerializeField]
        private bool isControlling = false;

       // [SyncVar()] public PlayerMatchInfo MatchInfo;
       private Animator selfMotionAnimator;

       public override IEntity Model { get; protected set; }

        private ISpaceshipConfigurationModel GetModel() {
            return GetModel<ISpaceshipConfigurationModel>();
        }

        protected override void Awake() {
            base.Awake();
            hookSystem = GetComponent<IHookSystem>();
            this.RegisterEvent<OnMassChanged>(OnMassChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            selfMotionAnimator = transform.Find("VisionControl/SelfSprite").GetComponent<Animator>();

        }

        

        private void OnMassChanged(OnMassChanged e) {
            Debug.Log(e.newMass);
        }


       
        protected override void Update() {
            base.Update();
            if (hasAuthority ) {
                RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
               
                if (Input.GetMouseButtonDown(0)) {
                    if (Model.HookState == HookState.Freed) {
                        isControlling = true;
                        
                    }
                    else {

                        isControlling  = false;
                        GetModel().CmdIncreaseEscapeCounter();
                       
                    }
                }

                //take item & put item (not shoot)
                if (Input.GetKey(KeyCode.Space)) {
                   hookSystem.CmdHoldHookButton();
                }

                if (Input.GetKeyUp(KeyCode.Space)) {
                   hookSystem.CmdReleaseHookButton();
                }

             
                if (Input.GetMouseButtonUp(0)) {
                    isControlling = false;
                }

              
                
            }


            if (hasAuthority && isClient)
            {
                //Debug.Log("Hasauthority");
                if (isControlling)
                {
                    selfMotionAnimator.SetBool("Controlling", true);
                    //CmdAddForce((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
                    CmdMove(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                   
                }
                else
                {
                    selfMotionAnimator.SetBool("Controlling", false);
                }
                if (Model.HookState == HookState.Freed) {
                    CmdRotate(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                }
            }

            if (isServer) {
                OnServerUpdate();
            }


          
        }

       

        protected override void FixedUpdate() {
          
            
            
        }

        

      

        //

        private void UpdateRotation(Vector2 mousePos)
        {
            
        }

        
    }
}

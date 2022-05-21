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
       

       // [SyncVar()] public PlayerMatchInfo MatchInfo;
       private Animator selfMotionAnimator;

       private float minHookPressTimeInterval = 0.4f;
       private float minHookPressTimer = 0f;

        private ISpaceshipConfigurationModel GetModel() {
            return GetModel<ISpaceshipConfigurationModel>();
        }

        protected override void Awake() {
            base.Awake();
            hookSystem = GetComponent<IHookSystem>();
            this.RegisterEvent<OnMassChanged>(OnMassChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            selfMotionAnimator = transform.Find("VisionControl/SelfSprite").GetComponent<Animator>();
            inventorySystem = GetComponent<IPlayerInventorySystem>();
        }

        

        private void OnMassChanged(OnMassChanged e) {
            Debug.Log(e.newMass);
        }


       
        protected override void Update() {
            base.Update();
            if (hasAuthority && isClient ) {
                minHookPressTimer += Time.deltaTime;
                if (Input.GetMouseButtonDown(1)) {
                    if (Model.HookState == HookState.Freed) {
                        CmdUpdateCanControl(true);
                    }
                    else {
                        CmdUpdateCanControl(false);
                        GetModel().CmdIncreaseEscapeCounter();
                    }
                }

                if (Input.GetMouseButtonDown(0)) {
                    CmdUpdateUsing(true);
                }

                if (Input.GetMouseButtonUp(0)) {
                    CmdUpdateUsing(false);
                }


                //take item & put item (not shoot)
                if (Input.GetKey(KeyCode.Space)) {
                    if (minHookPressTimer > minHookPressTimeInterval) {
                        hookSystem.CmdHoldHookButton();
                    }
                 
                }

                if (Input.GetKeyUp(KeyCode.Space)) {
                   
                   if (minHookPressTimer > minHookPressTimeInterval) {
                       minHookPressTimer = 0;
                       hookSystem.CmdReleaseHookButton();
                    }
                }

             
                if (Input.GetMouseButtonUp(1)) {
                    CmdUpdateCanControl(false);

                }

                ClientCheckMouseScroll();


            }


            if (hasAuthority && isClient) {
                if (Model.HookState == HookState.Freed) {
                    CmdUpdateMousePosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }

                if (isControlling && Model.HookState == HookState.Freed) {
                    selfMotionAnimator.SetBool("Controlling", true);
                }
                else
                {
                    selfMotionAnimator.SetBool("Controlling", false);
                }
            }

           
        }

        private float lastScroll = 0;
        private void ClientCheckMouseScroll() {
             float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

                if (lastScroll == 0)
                {
                    if (scrollWheel > 0f) { //up
                        CmdScrollMouseWhell(true);
                    }

                    if (scrollWheel < -0f) { //down
                    CmdScrollMouseWhell(false);
                    }
                }
                lastScroll = scrollWheel;
        }


        //

        private void UpdateRotation(Vector2 mousePos)
        {
            
        }

        
    }
}

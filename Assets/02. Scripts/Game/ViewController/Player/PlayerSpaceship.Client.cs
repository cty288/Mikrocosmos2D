using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship : AbstractDamagableViewController
    {


        // [SyncVar()] public PlayerMatchInfo MatchInfo;
        private Animator selfMotionAnimator;

        private float minHookPressTimeInterval = 0.8f;
        private float minHookPressTimer = 0f;

        private ISpaceshipConfigurationModel GetModel()
        {
            return GetModel<ISpaceshipConfigurationModel>();
        }

        protected override void Awake()
        {
            base.Awake();
            hookSystem = GetComponent<IHookSystem>();
            this.RegisterEvent<OnMassChanged>(OnMassChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            selfMotionAnimator = transform.Find("VisionControl/SelfSprite").GetComponent<Animator>();
            inventorySystem = GetComponent<IPlayerInventorySystem>();
            selfSprites.Add(transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>());
            selfSprites.AddRange(selfSprites[0].GetComponentsInChildren<SpriteRenderer>());
            buffSystem = GetComponent<IBuffSystem>();
        }


        
        private void OnMassChanged(OnMassChanged e)
        {
            Debug.Log(e.newMass);
        }

        
        public override void OnStartClient()
        {
            base.OnStartClient();
            this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => {
                if (this.GetSystem<IRoomMatchSystem>().ClientGetMatchInfoCopy().Team ==
                    ThisSpaceshipTeam)
                {
                    transform.Find("MapPlayer").GetComponent<SpriteRenderer>().sprite = mapSprites[0];
                }
                else
                {
                    transform.Find("MapPlayer").GetComponent<SpriteRenderer>().sprite = mapSprites[1];
                }
            });
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
        }

        protected override void Update()
        {
            base.Update();
            if (hasAuthority && isClient)
            {
                minHookPressTimer += Time.deltaTime;
                if (Input.GetMouseButtonDown(1))
                {
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
                if (Input.GetKey(KeyCode.Space))
                {
                    if (minHookPressTimer > minHookPressTimeInterval) {
                        hookSystem.CmdHoldHookButton();
                    }

                }

                if (Input.GetKeyUp(KeyCode.Space))
                {

                    if (minHookPressTimer > minHookPressTimeInterval)
                    {
                        minHookPressTimer = 0;
                        hookSystem.CmdReleaseHookButton();
                    }
                }


                if (Input.GetMouseButtonUp(1))
                {
                    CmdUpdateCanControl(false);

                }

                ClientCheckMouseScroll();

                for (int i = 48; i <= 57; i++) {
                    if (Input.GetKeyDown((KeyCode) i)) {
                        //48 - 0 -> slot 9
                        //49 - 1 -> slot 0
                        //50 - 2 -> slot 1
                        int index = 0;
                        if (i == 48) {
                            index = 9;
                        }
                        else {
                            index = i - 49;
                        }

                        CmdPressShortCut(index);
                    }
                }

            }


           

            if (hasAuthority && isClient) {
                if (Model.HookState == HookState.Freed) {
                    CmdUpdateMousePosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }

                if (isControlling && Model.HookState == HookState.Freed) {
                    selfMotionAnimator.SetBool("Controlling", true);
                }
                else {
                    selfMotionAnimator.SetBool("Controlling", false);
                }
            }


        }

        private float lastScroll = 0;
        private void ClientCheckMouseScroll()
        {
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

            if (lastScroll == 0)
            {
                if (scrollWheel > 0f)
                { //up
                    CmdScrollMouseWhell(true);
                }

                if (scrollWheel < -0f)
                { //down
                    CmdScrollMouseWhell(false);
                }
            }
            lastScroll = scrollWheel;
        }


        //

    

        [ClientRpc]
        private void RpcOnDizzyBuff(BuffStatus e, BuffClientMessage message) {
            if (e == BuffStatus.OnStart) {
                selfSprites.ForEach((spriteRenderer => spriteRenderer.DOColor(new Color(0.4f, 0.4f, 0.4f), 0.5f).SetLoops(-1, LoopType.Yoyo)));
            }else if (e == BuffStatus.OnEnd) {
                selfSprites.ForEach(spriteRenderer => {
                    spriteRenderer.DOKill(false);
                    spriteRenderer.DOColor(Color.white, 0.5f);
                });
            }
        }

        [ClientRpc]
        private void RpcOnInvincibleBuff(BuffStatus e, BuffClientMessage message) {
            if (e == BuffStatus.OnStart) {
                selfSprites.ForEach((spriteRenderer => spriteRenderer.DOFade(0.4f, 0.5f).SetLoops(-1, LoopType.Yoyo)));
            }
            else if (e == BuffStatus.OnEnd)
            {
                selfSprites.ForEach(spriteRenderer => {
                    spriteRenderer.DOKill(false);
                    spriteRenderer.DOFade(1f, 0.5f);
                });
            }
        }

        
    }
}

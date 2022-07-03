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
using UnityEngine.Networking.Types;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship : AbstractDamagableViewController
    {


        // [SyncVar()] public PlayerMatchInfo MatchInfo;
        private Animator selfMotionAnimator;

        private float minHookPressTimeInterval = 0.8f;
        private float minHookPressTimer = 0f;
        private Vector2 clientPreviousMousePosition = Vector2.zero;
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

        private bool hookWhenEmptyReleased = true;
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

        [SerializeField] private List<ParticleSystem> particles;
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
                if (Input.GetKeyDown(KeyCode.Space))
                {
                  
                        if (!hookSystem.HookedNetworkIdentity) {
                            hookWhenEmptyReleased = false;
                          //  minHookPressTimer = 0;
                        }
                        hookSystem.CmdPressHookButton();
                    

                }

                if (Input.GetKeyUp(KeyCode.Space))
                {
                    
                    if (!hookWhenEmptyReleased) {
                        hookWhenEmptyReleased = true;
                    }
                    else {
                        if (minHookPressTimer > minHookPressTimeInterval) {
                            minHookPressTimer = 0;
                            hookSystem.CmdReleaseHookButton();
                        }
                        
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
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (Vector2.Distance(mousePosition, clientPreviousMousePosition) >= 2 ) {
                        CmdUpdateMousePosition(mousePosition);
                        clientPreviousMousePosition = mousePosition;
                    }
                }

                if (isControlling && Model.HookState == HookState.Freed) {
                    selfMotionAnimator.SetBool("Controlling", true);
                    foreach (ParticleSystem particle in particles) {
                        particle.loop = true;
                        particle.Play();

                     
                        

                    }
                }
                else {
                    selfMotionAnimator.SetBool("Controlling", false);
                    foreach (ParticleSystem particle in particles)
                    {
                         particle.loop = false;
                        //particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                        //  particle.SetActive(false);
                    }
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

        [SerializeField] private GameObject deathEffect;
    

        [ClientRpc]
        private void RpcOnDizzyBuff(BuffStatus e, BuffClientMessage message) {
            if (e == BuffStatus.OnStart) {
                selfSprites.ForEach((spriteRenderer => spriteRenderer.DOColor(new Color(0.4f, 0.4f, 0.4f), 0.5f).SetLoops(-1, LoopType.Yoyo)));
                Instantiate(deathEffect, transform);
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

        [SerializeField] private GameObject hitParticlePrefab;
        protected override void OnCollisionEnter2D(Collision2D collision) {            
            base.OnCollisionEnter2D(collision);
            if (hookSystem.HookedNetworkIdentity != collision.collider.GetComponent<NetworkIdentity>()) {
                GameObject particle = Instantiate(hitParticlePrefab, collision.GetContact(0).point, Quaternion.identity);
                particle.transform.SetParent(transform);
                if (hasAuthority) {
                    GameCamera.Singleton.OnShakeCamera(new ShakeCamera()
                    {
                        Duration = 0.25f,
                        Strength = 1.5f,
                        Viberato = 10
                    });
                }
              
            }
          
        }
    }
}

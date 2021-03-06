using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
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

        private Animator animator;

        private BindableProperty<float> clientHorizontal = new BindableProperty<float>(0);
        private BindableProperty<float> clientVertical = new BindableProperty<float>(0);

        private SpriteRenderer wantedIcon;

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
            wantedIcon = transform.Find("WantedIcon").GetComponent<SpriteRenderer>();
            selfSprites.AddRange(selfSprites[0].GetComponentsInChildren<SpriteRenderer>());
            buffSystem = GetComponent<IBuffSystem>();
            animator = GetComponent<Animator>();
            gameProgressSystem = this.GetSystem<IGameProgressSystem>();
            this.RegisterEvent<OnClientSpaceshipCriminalityUpdate>(OnCrimelityUpdate).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

       


        private void OnMassChanged(OnMassChanged e)
        {
            Debug.Log(e.newMass);
        }

        private bool hookWhenEmptyReleased = true;
        public override void OnStartClient() {
            base.OnStartClient();
            this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => {
                ChangeToNormalMapSprite();
            });
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
        }

        private void OnCrimelityUpdate(OnClientSpaceshipCriminalityUpdate e) {
            if (e.SpaceshipIdentity == netIdentity) {
                if (e.Criminality == 0) {
                    ChangeToNormalMapSprite();
                    return;
                }
                Sprite targetSprite = null;
                switch (e.BountyType) {
                    case BountyType.Opponent:
                        targetSprite = mapSpritesWhenHunted[1];
                        wantedIcon.enabled = true;
                        break;
                    case BountyType.Self:
                        targetSprite = mapSpritesWhenHunted[2];
                        break;
                    case BountyType.Teammate:
                        targetSprite = mapSpritesWhenHunted[0];
                        break;
                }
                Transform mapPlayer = transform.Find("MapPlayer");
                mapPlayer.GetComponent<SpriteRenderer>().sprite = targetSprite;
                mapPlayer.DOScale(new Vector3(15, 15, 1), 0.5f);
            }
        }
        private void ChangeToNormalMapSprite() {
            Transform mapPlayer = transform.Find("MapPlayer");
            wantedIcon.enabled = false;
            if (this.GetSystem<IRoomMatchSystem>().ClientGetMatchInfoCopy().Team ==
                ThisSpaceshipTeam) {
               
                if (hasAuthority) {
                    mapPlayer.GetComponent<SpriteRenderer>().sprite = mapSprites[2];
                    mapPlayer.DOScale(new Vector3(15, 15, 1), 0.5f);
                }
                else {
                    mapPlayer.GetComponent<SpriteRenderer>().sprite = mapSprites[0];
                    mapPlayer.DOScale(new Vector3(10, 10, 1), 0.5f);                    
                }
            }
            else {
                mapPlayer.GetComponent<SpriteRenderer>().sprite = mapSprites[1];
                mapPlayer.DOScale(new Vector3(10, 10, 1), 0.5f);
            }
        }


        public override void OnStartAuthority() {
            base.OnStartAuthority();
            clientHorizontal.RegisterOnValueChaned(CmdOnClientHorizontalChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            clientVertical.RegisterOnValueChaned(CmdOnClientVerticalChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        [SerializeField] private List<ParticleSystem> particles;

        private Dictionary<string, List<GameObject>> tempItemStorage = new Dictionary<string, List<GameObject>>();
        protected override void Update()
        {
            base.Update();
            if (isServer) {
                if (Input.GetKeyDown(KeyCode.LeftShift)) {
                    if (hookSystem.HookedItem!=null && !hookSystem.HookedItem.Model.CanBeAddedToInventory) {
                        hookSystem.HookedItem.Model.UnHook(false);
                    }
                    var slots = inventorySystem.BackpackItems;
                    foreach (BackpackSlot slot in slots) {
                        if (slot.Count <= 0) {
                            continue;
                        }
                        if (!tempItemStorage.ContainsKey(slot.PrefabName)) {
                            tempItemStorage.Add(slot.PrefabName, new List<GameObject>());
                        }

                        while (slot.Count > 0) {
                            GameObject obj = inventorySystem.ServerRemoveFromBackpack(slot.PrefabName);
                            tempItemStorage[slot.PrefabName].Add(obj);
                            NetworkServer.UnSpawn(obj);
                            obj.SetActive(false);
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.RightShift)) {
                    foreach (string key in tempItemStorage.Keys) {
                        foreach (GameObject o in tempItemStorage[key]) {
                            inventorySystem.ServerAddToBackpack(key, o, (obj) => {
                                obj.SetActive(true);
                                obj.transform.position = transform.position;
                                NetworkServer.Spawn(obj);
                            });
                        }
                    }
                    tempItemStorage.Clear();
                }
            }
            if (hasAuthority && isClient)
            {
                
                    if (Input.GetMouseButtonDown(1)) {
                        if (Model.HookState == HookState.Freed) {
                            CmdUpdateCanControl(true);
                        }
                        else {
                            CmdUpdateCanControl(false);
                            GetModel().CmdIncreaseEscapeCounter();
                        }
                    }


                    if (Model.HookState == HookState.Freed) {
                        clientHorizontal.Value = Input.GetAxis("Horizontal");
                        clientVertical.Value = Input.GetAxis("Vertical");
                    }
                    else {
                        clientHorizontal.Value = 0;
                        clientVertical.Value = 0;
                    }
               
                    if (Input.GetMouseButtonDown(0)) {
                        CmdUpdateUsing(true);
                    }

                    if (Input.GetMouseButtonUp(0)) {
                        CmdUpdateUsing(false);
                    }


                    //take item & put item (not shoot)
                    if (Input.GetKeyDown(KeyCode.Space) && !DebugCanvas.IsOpening) {
                        if (gameProgressSystem.GameState != GameState.InGame) {
                            return;
                        }

                        if (!hookSystem.HookedNetworkIdentity) {
                        
                            hookWhenEmptyReleased = false;
                            //  minHookPressTimer = 0;
                            if (animator.GetCurrentAnimatorStateInfo(0).IsName("UnHooking") && DamagableModel.CurrentHealth > 0) {
                                if (ThisSpaceshipTeam == 2) {
                                    this.GetSystem<IAudioSystem>().PlaySound("Team2Hook", SoundType.Sound2D);
                                }
                            }

                        }
                   
                        hookSystem.CmdPressHookButton();
                    }

                    if (Input.GetKeyUp(KeyCode.Space) && !DebugCanvas.IsOpening)
                    {
                        if (gameProgressSystem.GameState != GameState.InGame) {
                            return;
                        }
                        if (!hookWhenEmptyReleased) {
                            hookWhenEmptyReleased = true;
                        }
                        else {
                            //if (minHookPressTimer > minHookPressTimeInterval) {
                            hookSystem.CmdReleaseHookButton();
                            // }
                        
                        }
                    }

                    if (Input.GetMouseButtonUp(1)) {
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
                    if (gameProgressSystem.GameState != GameState.InGame) {
                        return;
                    }
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (Vector2.Distance(mousePosition, clientPreviousMousePosition) >= 2 ) {
                        CmdUpdateMousePosition(mousePosition);
                        clientPreviousMousePosition = mousePosition;
                    }
                }

                if (gameProgressSystem.GameState== GameState.InGame && isControlling && Model.HookState == HookState.Freed) {
                    selfMotionAnimator.SetBool("Controlling", true);
                    foreach (ParticleSystem particle in particles) {
                        particle.loop = true;
                        particle.Play();
                    }
                }
                else {
                    selfMotionAnimator.SetBool("Controlling", false);
                    foreach (ParticleSystem particle in particles) {
                         particle.loop = false;
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
                  
                }
              
            }
          
        }

        protected override DescriptionItem GetDescription() {
            return null;
        }
    }
}

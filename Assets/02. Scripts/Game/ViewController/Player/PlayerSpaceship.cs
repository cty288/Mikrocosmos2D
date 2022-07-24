using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{

    

    public partial class PlayerSpaceship : AbstractDamagableViewController
    {
        private IHookSystem hookSystem;
        [SerializeField, SyncVar]
        private bool isControlling = false;

        public bool IsControlling
        {
            get
            {
                return isControlling;
            }
        }

        [field: SyncVar]
        public bool CanControl { get; set; } = true;

        [field: SyncVar, SerializeField]
        public PlayerMatchInfo matchInfo { get; private set; }

        [SerializeField, SyncVar]
        private bool isUsing = false;

        [SyncVar]
        private Vector2 mousePosition;

        private int escapeCounter = 0;

        [SerializeField]
        private List<Sprite> teamSprites;

        [SerializeField] private List<Sprite> mapSprites;
        [SerializeField] private List<Sprite> mapSpritesWhenHunted;

        [SyncVar]
        public string Name;

        [SyncVar] public int ThisSpaceshipTeam;

        private IPlayerInventorySystem inventorySystem;

        private List<SpriteRenderer> selfSprites = new List<SpriteRenderer>();

        private IBuffSystem buffSystem;

        [SyncVar]
        private int teamIndex;

        private IGameProgressSystem gameProgressSystem;

        private float serverHorizontal = 0;
        private float serverVertical = 0;


        [ServerCallback]
        public void SetPlayerDisplayInfo(PlayerMatchInfo info) {
            this.matchInfo = info;
            selfSprites[0].sprite = teamSprites[teamIndex];

            this.teamIndex = info.TeamIndex;

            this.Name = info.Name;
            ThisSpaceshipTeam = info.Team;
            RpcSetTeamSprite(teamIndex);
        }

        
        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnPlayerDie>(OnServerPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
            buffSystem.ServerRegisterCallback<DieBuff, BuffClientMessage>(RpcOnDizzyBuff);
            buffSystem.ServerRegisterCallback<InvincibleBuff, BuffClientMessage>(RpcOnInvincibleBuff);
           
        }


        
        [ServerCallback]
        private void OnServerPlayerDie(OnPlayerDie e) {
         
            if (e.SpaceshipIdentity == netIdentity) {
                float dieTime = GetModel().DieDizzyTime;
                float invincibleTime = GetModel().RespawnInvincibleTime;
                
                buffSystem.AddBuff<DieBuff>(new DieBuff(dieTime, () => {
                    DamagableModel.AddHealth(DamagableModel.MaxHealth);
                    buffSystem.AddBuff<InvincibleBuff>(new InvincibleBuff(invincibleTime));
                }));
            }
            
        }

        [ClientRpc]
        private void RpcSetTeamSprite(int teamIndex) {
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
        }


        

        [ServerCallback]
        protected override void OnServerUpdate() {
            if (gameProgressSystem!=null && gameProgressSystem.GameState != GameState.InGame) {
                return;
            }

            if (isUsing) {
                hookSystem.OnServerPlayerHoldUseButton();
            }
        }



        [Command]
        private void CmdUpdateCanControl(bool isControl) {
           
            isControlling = isControl;
        }

        [Command]
        private void CmdUpdateUsing(bool isUsing) {
            
            this.isUsing = isUsing;
            if (!isUsing) {
                hookSystem.OnServerPlayerReleaseUseButton();
            }
        }



        [Command]
        private void CmdUpdateMousePosition(Vector2 mousePos)
        {
            if (gameProgressSystem!=null && gameProgressSystem.GameState != GameState.InGame) {
                return;
            }
            mousePosition = mousePos;
        }

        [ServerCallback]
        private void ServerMove(Vector3 mousePos) {
            if (Vector3.Distance(mousePos, transform.position) < 5 && (serverHorizontal==0)) {
                return;
            }

            
            Vector2 forceDir = Vector2.zero;
            if (isControlling) {
                forceDir = (mousePos - transform.position).normalized;
            }
           
            Vector2 transformRight = transform.right;
            forceDir += transformRight * serverHorizontal;

            /*if (serverHorizontal != 0 || serverVertical != 0) {
                forceDir = new Vector2(serverHorizontal, serverVertical);
            }*/

            
            Vector2 targetAddedVelocity = forceDir * GetModel().Acceleration * Time.fixedDeltaTime;
            


            if (gameProgressSystem!=null && gameProgressSystem.GameState != GameState.InGame) {
                targetAddedVelocity = Vector2.zero;
            }

            if (rigidbody.velocity.magnitude <= Model.MaxSpeed || (rigidbody.velocity + targetAddedVelocity).magnitude <
                rigidbody.velocity.magnitude) {
                //rigidbody.AddForce(forceDir * GetModel().MoveForce);
                rigidbody.velocity += targetAddedVelocity;
            }
        }

        
        [ServerCallback]
        private void ServerRotate(Vector2 mousePos)
        {
            if (Vector3.Distance(mousePos, transform.position) < 5) {
                return;
            }
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - mousePos;
            float angle = Mathf.Atan2(dir.y, dir.x) * (180 / Mathf.PI) + 90;
            if (gameProgressSystem!=null && gameProgressSystem.GameState != GameState.InGame)
            {
                return;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.4f);
            // rigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.2f));
        }

        [Command]
        private void CmdScrollMouseWhell(bool up)
        {
            if (gameProgressSystem.GameState != GameState.InGame)
            {
                return;
            }
            int currentSlot = inventorySystem.GetCurrentSlot();
            inventorySystem.ServerSwitchSlot(currentSlot + (up ? -1 : 1));
        }

        /// <summary>
        /// index from 0-9
        /// </summary>
        /// <param name="index"></param>
        [Command]
        private void CmdPressShortCut(int index)
        {
            if (gameProgressSystem.GameState != GameState.InGame)
            {
                return;
            }            
            int backpackCapacity = inventorySystem.GetSlotCount();
            if (index < backpackCapacity && index!= inventorySystem.GetCurrentSlot()) {
                inventorySystem.ServerSwitchSlot(index);
            }
        }

        public void RecoverCanControl(float time) {
            StartCoroutine(RecoverCanControlCoroutine(time));
        }

        IEnumerator RecoverCanControlCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
            CanControl = true;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isServer)
            {
                if (gameProgressSystem != null && gameProgressSystem.GameState != GameState.InGame) {
                    return;
                }
                
                if (buffSystem.HasBuff<DieBuff>()) {
                    return;
                }
                
                //Debug.Log("Hasauthority");
                if ((isControlling ||  serverHorizontal!=0 ) && Model.HookState == HookState.Freed && CanControl) {
                    ServerMove(mousePosition);
                }

                if (Model.HookState == HookState.Freed)
                {
                    ServerRotate(mousePosition);
                }

                if (Model.HookState == HookState.Freed)
                {
                    rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, GetModel().MaxMaxSpeed);
                }
            }
        }
        

        [SerializeField] private GameObject hurtEffect;
        [ClientRpc]
        public override void RpcOnClientHealthChange(int oldHealth, int newHealth) {
            Debug.Log($"Health Received: {newHealth}");
            if (newHealth < oldHealth) {
                StartCoroutine(PlayHurtEffect());
                Instantiate(hurtEffect, transform);
                if (hasAuthority) {
                    int damage = Mathf.Abs(oldHealth - newHealth);
                    //get the percentile of damage between 0 to 20
                    float strength = Mathf.Clamp((damage / 20f), 0.3f, 1f) * 10f;

                    if (newHealth > 0) {
                        GameCamera.Singleton.OnShakeCamera(new OnShakeCamera()
                        {
                            Duration = 0.25f,
                            Strength = strength,
                            Viberato = 10
                        });
                    }
                    else {
                        GameCamera.Singleton.OnShakeCamera(new OnShakeCamera()
                        {
                            Duration = 0.5f,
                            Strength = 30,
                            Viberato = 20
                        });
                    }
                   
                }
            }
        }


        private IEnumerator PlayHurtEffect() {
            foreach (SpriteRenderer sprite in selfSprites) {
                sprite.color = Color.red;
            }

            yield return new WaitForSeconds(0.2f);

            foreach (SpriteRenderer sprite in selfSprites)
            {
                sprite.color = Color.white;
            }
            
           
        }

        [Command]
        private void CmdOnClientHorizontalChanged(float oldHorizontal, float horizontal) {
            serverHorizontal = horizontal;
        }

        [Command]
        private void CmdOnClientVerticalChanged(float arg1, float vertical) {
            serverVertical = vertical;
        }
    }

}


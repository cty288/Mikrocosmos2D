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

        

        [SerializeField, SyncVar]
        private bool isUsing = false;

        [SyncVar]
        private Vector2 mousePosition;

        private int escapeCounter = 0;

        [SerializeField]
        private List<Sprite> teamSprites;

        [SerializeField] private List<Sprite> mapSprites;

        [SyncVar]
        public string Name;

        [SyncVar] public int ThisSpaceshipTeam;

        private IPlayerInventorySystem inventorySystem;

        private List<SpriteRenderer> selfSprites = new List<SpriteRenderer>();

        private IBuffSystem buffSystem;

        [SyncVar]
        private int teamIndex;
        
      
        
        [ServerCallback]
        public void SetPlayerDisplayInfo(int team, int teamIndex, string name)
        {
           
            selfSprites[0].sprite = teamSprites[teamIndex];

            this.teamIndex = teamIndex;

            this.Name = name;
            ThisSpaceshipTeam = team;
            RpcSetTeamSprite(teamIndex);
        }

        
        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnPlayerDie>(OnServerPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);

            buffSystem.ServerRegisterClientCallback<DizzyTimedBuff>(RpcOnDizzyBuff);
            buffSystem.ServerRegisterClientCallback<InvincibleTimedBuff>(RpcOnInvincibleBuff);
        }


        
        [ServerCallback]
        private void OnServerPlayerDie(OnPlayerDie e) {
         
            if (e.SpaceshipIdentity == netIdentity) {
                float dieTime = GetModel().DieDizzyTime;
                float invincibleTime = GetModel().RespawnInvincibleTime;
                
                buffSystem.AddBuff<DizzyTimedBuff>(new DizzyTimedBuff(dieTime, dieTime, buffSystem, () => {
                    DamagableModel.AddHealth(DamagableModel.MaxHealth);
                    buffSystem.AddBuff<InvincibleTimedBuff>(new InvincibleTimedBuff(invincibleTime, invincibleTime, buffSystem));
                }));
            }
            
        }

        [ClientRpc]
        private void RpcSetTeamSprite(int teamIndex) {
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
        }


        

        [ServerCallback]
        protected override void OnServerUpdate() {
            if (isUsing) {
                hookSystem.OnServerPlayerHoldUseButton();
            }
        }



        [Command]
        private void CmdUpdateCanControl(bool isControl)
        {
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
            mousePosition = mousePos;
        }

        [ServerCallback]
        private void ServerMove(Vector3 mousePos)
        {
            Vector2 forceDir = (mousePos - transform.position)
                .normalized;
            Vector2 targetAddedVelocity = forceDir * GetModel().Acceleration * Time.fixedDeltaTime;
            if (rigidbody.velocity.magnitude <= Model.MaxSpeed || (rigidbody.velocity + targetAddedVelocity).magnitude <
                rigidbody.velocity.magnitude)
            {
                //rigidbody.AddForce(forceDir * GetModel().MoveForce);
                rigidbody.velocity += targetAddedVelocity;
            }
        }
        [ServerCallback]
        private void ServerRotate(Vector2 mousePos)
        {
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - mousePos;
            float angle = Mathf.Atan2(dir.y, dir.x) * (180 / Mathf.PI) + 90;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.4f);
            // rigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.2f));
        }

        [Command]
        private void CmdScrollMouseWhell(bool up)
        {
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
            int backpackCapacity = inventorySystem.GetSlotCount();
            if (index < backpackCapacity)
            {
                inventorySystem.ServerSwitchSlot(index);
            }
        }



        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isServer)
            {
                if (buffSystem.HasBuff<DizzyTimedBuff>()) {
                    return;
                }
                
                //Debug.Log("Hasauthority");
                if (isControlling && Model.HookState == HookState.Freed && CanControl) {
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


        [ClientRpc]
        public override void RpcOnClientHealthChange(int oldHealth, int newHealth) {
            Debug.Log($"Health Received: {newHealth}");
            if (newHealth < oldHealth) {
                StartCoroutine(PlayHurtEffect());
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
    }

}


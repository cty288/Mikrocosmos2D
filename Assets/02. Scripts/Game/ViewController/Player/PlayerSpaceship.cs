using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
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

        [field: SyncVar] public bool CanControl { get; set; } = true;

        [SerializeField, SyncVar]
        private bool isUsing = false;

        [SyncVar]
        private Vector2 mousePosition;

        private int escapeCounter = 0;

        [SerializeField]
        private List<Sprite> teamSprites;

        [SyncVar]
        public string Name;

        private IPlayerInventorySystem inventorySystem;
        [ServerCallback]
        public void SetPlayerDisplayInfo(int teamIndex, string name)
        {
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
            this.Name = name;
            RpcSetTeamSprite(teamIndex);
        }


        [ClientRpc]
        private void RpcSetTeamSprite(int teamIndex)
        {
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
        }

        [ServerCallback]
        protected override void OnServerUpdate()
        {
            if (Model.HookState == HookState.Freed)
            {
                rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, GetModel().MaxMaxSpeed);
            }

            if (isUsing)
            {
                hookSystem.OnServerPlayerHoldUseButton();
            }
        }



        [Command]
        private void CmdUpdateCanControl(bool isControl)
        {
            isControlling = isControl;
        }

        [Command]
        private void CmdUpdateUsing(bool isUsing)
        {
            this.isUsing = isUsing;
            if (!isUsing)
            {
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
            Vector2 targetAddedVelocity = forceDir * GetModel().Acceleration * Time.deltaTime;
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
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.2f);
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
            if (index < backpackCapacity) {
                inventorySystem.ServerSwitchSlot(index);
            }
        }



        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isServer)
            {
                //Debug.Log("Hasauthority");
                if (isControlling && Model.HookState == HookState.Freed && CanControl)
                {
                    ServerMove(mousePosition);
                }

                if (Model.HookState == HookState.Freed)
                {
                    ServerRotate(mousePosition);
                }
            }
        }


        [ClientRpc]
        public override void RpcOnClientHealthChange(int newHealth)
        {
            Debug.Log($"Health Received: {newHealth}");
        }
    }

}


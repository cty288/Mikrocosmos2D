using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
   
    
    public partial class PlayerSpaceship : BasicEntityViewController {
        private IHookSystem hookSystem;

       
        private int escapeCounter = 0;

        [SerializeField] 
        private List<Sprite> teamSprites;

        [SyncVar]
        public string Name;

        [ServerCallback]
        public void SetPlayerDisplayInfo(int teamIndex, string name)
        {
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
            this.Name = name;
            RpcSetTeamSprite(teamIndex);
        }


        [ClientRpc]
        private void RpcSetTeamSprite(int teamIndex) {
            transform.Find("VisionControl/SelfSprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
        }

        private void OnServerUpdate() {
            
            rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, GetModel().MaxMaxSpeed);
        }

        [Command]
        private void CmdMove(Vector3 mousePos) {
            Vector2 forceDir = (mousePos - transform.position)
                .normalized;
            if (rigidbody.velocity.sqrMagnitude <= Mathf.Pow(Model.MaxSpeed, 2))
            {
                //rigidbody.AddForce(forceDir * GetModel().MoveForce);
                rigidbody.velocity += forceDir * GetModel().Acceleration * Time.deltaTime;
            }
        }
        [Command]
        private void CmdRotate(Vector2 mousePos)
        {
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - mousePos;
            float angle = Mathf.Atan2(dir.y, dir.x) * (180 / Mathf.PI) + 90;
           // transform.rotation = 
            rigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.2f));
        }

    }

    }


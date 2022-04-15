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
           
        }

      
        
    }

    }


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

        [ServerCallback]
        public void SetTeamSprite(int teamIndex)
        {
            transform.Find("VisionControl/Sprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
            RpcSetTeamSprite(teamIndex);
        }


        [ClientRpc]
        private void RpcSetTeamSprite(int teamIndex) {
            transform.Find("VisionControl/Sprite").GetComponent<SpriteRenderer>().sprite = teamSprites[teamIndex];
        }
        private void OnServerUpdate() {
           
        }

      
        
    }

    }


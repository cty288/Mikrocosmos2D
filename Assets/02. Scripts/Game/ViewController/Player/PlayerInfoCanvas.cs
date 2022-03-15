using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class PlayerInfoCanvas : MonoBehaviour {
        [SerializeField] private TMP_Text playerNameText;

        private PlayerMatchInfo matchInfo;
     
        private void Update() {
            if (matchInfo!=null) {
                playerNameText.text = matchInfo.Name;
            }
            else {
                matchInfo = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().matchInfo;
            }
           
        }
    }
}

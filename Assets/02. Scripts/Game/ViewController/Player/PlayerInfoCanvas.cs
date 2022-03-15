using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class PlayerInfoCanvas : AbstractMikroController<Mikrocosmos> {
        [SerializeField] private TMP_Text playerNameText;

        private PlayerMatchInfo matchInfo;
     
        private void Update() {
            if (matchInfo!=null) {
                playerNameText.text = matchInfo.Name;
            }
            else {
                matchInfo =this.GetSystem<IRoomMatchSystem>().ClientGetMatchInfoCopy();
            }
           
        }
    }
}

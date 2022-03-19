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

        private string name;
     
        private void Update() {
            name = GetComponentInParent<PlayerSpaceship>().Name;
            playerNameText.text = name;
        }
    }
}

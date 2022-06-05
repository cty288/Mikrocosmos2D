using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlayerInfoCanvas : AbstractMikroController<Mikrocosmos> {
        [SerializeField] private Text playerNameText;

        private string name;


        private void Awake() {
            playerNameText.text = "";
        }

        private void Start() {
            UntilAction untilAction = UntilAction.Allocate(() => NetworkClient.active);
            untilAction.OnEndedCallback += () => {
                playerNameText.text = GetComponentInParent<PlayerSpaceship>().Name;
            };
            untilAction.Execute();
        }
        
    }
}

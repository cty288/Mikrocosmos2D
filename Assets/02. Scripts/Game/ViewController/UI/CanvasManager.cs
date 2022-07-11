using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;

namespace Mikrocosmos
{
    public class CanvasManager : AbstractMikroController<Mikrocosmos> {
        [SerializeField] private List<GameObject> panelToCloseWhenGameEnds = new List<GameObject>();

        private void Awake() {
            this.RegisterEvent<OnClientGameEnd>(OnGameEnds).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnGameEnds(OnClientGameEnd obj) {
            foreach (GameObject panel in panelToCloseWhenGameEnds) {
                panel.SetActive(false);
            }
        }
    }
}

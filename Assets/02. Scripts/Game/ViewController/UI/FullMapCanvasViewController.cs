using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnFullMapCanvasOpen {

    }

    public struct OnFullMapCanvasClose
    {

    }
    public class FullMapCanvasViewController : AbstractMikroController<Mikrocosmos>, ICanSendEvent {
        private GameObject fullMapPanel;

        private void Awake() {
            fullMapPanel = transform.Find("FullMap").gameObject;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                fullMapPanel.SetActive(true);
                this.SendEvent<OnFullMapCanvasOpen>();
            }

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                fullMapPanel.SetActive(false);
                this.SendEvent<OnFullMapCanvasClose>();
            }
        }
    }
}

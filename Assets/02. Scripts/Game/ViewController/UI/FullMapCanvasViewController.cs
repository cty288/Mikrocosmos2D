using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class FullMapCanvasViewController : MonoBehaviour{
        private GameObject fullMapPanel;

        private void Awake() {
            fullMapPanel = transform.Find("FullMap").gameObject;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                fullMapPanel.SetActive(true);
            }

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                fullMapPanel.SetActive(false);
            }
        }
    }
}

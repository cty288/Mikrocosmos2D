using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class DebugCanvas : MonoBehaviour {
        private GameObject debugPanel;
        public static bool IsOpening = false;
        private void Awake() {
            debugPanel = transform.Find("DebugPanel").gameObject;
        }

        private void Update() {
            if (Input.GetKeyUp(KeyCode.T)) {
                
                    IsOpening = true;
                    debugPanel.gameObject.SetActive(true);
                
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (IsOpening) {
                    IsOpening = false;
                    debugPanel.gameObject.SetActive(false);
                }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class DebugCanvas : MonoBehaviour {
        private GameObject debugPanel;
        private bool isOpen = false;
        private void Awake() {
            debugPanel = transform.Find("DebugPanel").gameObject;
        }

        private void Update() {
            if (Input.GetKeyUp(KeyCode.F5)) {
                isOpen = !isOpen;
                debugPanel.gameObject.SetActive(isOpen);
            }
        }
    }
}

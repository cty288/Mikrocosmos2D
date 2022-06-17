using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class InfoElementText : MonoBehaviour {
        private TMP_Text titleText;
        private TMP_Text descriptionText;

        private void Awake() {
            titleText = transform.Find("Title").GetComponent<TMP_Text>();
            if (transform.Find("Description") != null) {
                descriptionText = transform.Find("Description").GetComponent<TMP_Text>();
            }
        }

        public void SetInfo(string titleText, string description) {
            this.titleText.text = titleText;
            if (this.descriptionText != null) {
                this.descriptionText.text = description;
            }
        }
    }
}

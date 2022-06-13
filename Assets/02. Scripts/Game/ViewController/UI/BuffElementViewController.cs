using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class BuffElementViewController : MonoBehaviour {
        private TMP_Text titleText;
        private TMP_Text descriptionText;
        private BuffIconViewController iconViewController;
        private void Awake() {
            titleText = transform.Find("TitleText").GetComponent<TMP_Text>();
            descriptionText = transform.Find("DescriptionText").GetComponent<TMP_Text>();
        }

        public void SetBuffInfo(BuffInfo info) {
            if (!iconViewController) {
                iconViewController = GetComponentInChildren<BuffIconViewController>();
            }

            titleText.text = info.LocalizedName;
            descriptionText.text = info.LocalizedDescription;
            iconViewController.SetIconInfo(info);
        }

    }
}

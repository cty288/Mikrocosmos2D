using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class ChatMessage : MonoBehaviour {
        private TMP_Text nameText;
        private TMP_Text messageText;
        private GameObject avatarElement;

        private void Awake() {
            nameText = GetComponent<TMP_Text>();
            messageText = transform.Find("Message").GetComponent<TMP_Text>();
            avatarElement = transform.Find("AvatarElement").gameObject;
        }

       
    }
}

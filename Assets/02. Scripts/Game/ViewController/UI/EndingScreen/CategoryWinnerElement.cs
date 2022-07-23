using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class CategoryWinnerElement : MonoBehaviour {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        private AvatarElementViewController avatarElement;

        private Avatar avatar;
        private void Awake() {
            avatarElement = GetComponentInChildren<AvatarElementViewController>(true);
        }

        private void Start() {
            if (avatar != null) {
                avatarElement.SetAvatar(avatar);
            }
        }


        public void SetInfo(string name, string descriptionText, Avatar avatar) {
            this.avatar = avatar;
          
            nameText.text = name;
            this.descriptionText.text = descriptionText;
           
        }
    }
}

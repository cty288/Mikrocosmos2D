using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class CreateAvatarPanel : AbstractMikroController<Mikrocosmos> {
        private AvatarElementSelectionPanel selectionPanel;
        private IClientAvatarModel avatarModel;
        private ChangeAvatarPanelAvatarShowcase showcase;
        private AvatarElementViewController showcaseAvatar;
        private void Awake() {
            selectionPanel = ObjCreateAvatarPanel.transform.Find("AvatarSelectionPanel")
                .GetComponent<AvatarElementSelectionPanel>();
            avatarModel = this.GetModel<IClientAvatarModel>();
            BtnRandom.onClick.AddListener(OnRandomAvatarButtonClicked);
            BtnSaveLook.onClick.AddListener(OnSaveLookButtonClicked);
            showcase = GetComponentInChildren<ChangeAvatarPanelAvatarShowcase>();
            showcaseAvatar = showcase.GetComponentInChildren<AvatarElementViewController>();
        }

        private void OnSaveLookButtonClicked() {
            Avatar avatar = showcase.RecordedExistingAvatar;
            foreach (var avatarSubElement in showcaseAvatar.SubElements) {
                if (avatarSubElement.gameObject.activeInHierarchy) {
                    RectTransform transform = avatarSubElement.GetComponent<RectTransform>();
                    Vector2 offset = new Vector2(transform.offsetMin.x, -transform.offsetMax.y);
                    avatar.UpdateOffset(avatarSubElement.Index, offset);
                }
            }
            avatarModel.SaveAvatar(showcase.RecordedExistingAvatar);
            BtnBack.interactable = true;
        }

        public void SaveAvatar() {

        }
        private void OnRandomAvatarButtonClicked() {
            selectionPanel.RandomSelect();
        }

       
        private void Start() {
            selectionPanel.StartFill(avatarModel.Avatar);
            if (avatarModel.Avatar.Elements.Count == 0) {
                BtnBack.interactable = false;
            }
            else {
                BtnBack.interactable = true;
            }
        }
    }
}
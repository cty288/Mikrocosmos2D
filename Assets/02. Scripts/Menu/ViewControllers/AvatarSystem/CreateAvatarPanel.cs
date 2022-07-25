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
        private ILocalPlayerInfoModel localPlayerInfo;
        private void Awake() {
            selectionPanel = ObjCreateAvatarPanel.transform.Find("AvatarSelectionPanel")
                .GetComponent<AvatarElementSelectionPanel>();
            avatarModel = this.GetModel<IClientAvatarModel>();
            BtnRandom.onClick.AddListener(OnRandomAvatarButtonClicked);
            BtnSaveLook.onClick.AddListener(OnSaveLookButtonClicked);
            showcase = GetComponentInChildren<ChangeAvatarPanelAvatarShowcase>(true);
            showcaseAvatar = showcase.GetComponentInChildren<AvatarElementViewController>(true);
            localPlayerInfo = this.GetModel<ILocalPlayerInfoModel>();
            BtnBack.onClick.AddListener(OnClosePanel);
            InputName.onValueChanged.AddListener(OnInputFieldNameChanged);
        }

        private void OnClosePanel() {
            GetComponentInParent<MenuUI>().CloseAvatarPanel();
        }

        private void OnInputFieldNameChanged(string name) {
            localPlayerInfo.ChangeName(name);
            CheckCanEnableBackButton();
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
            CheckCanEnableBackButton();
        }

        private void CheckCanEnableBackButton() {
            BtnBack.interactable = (!String.IsNullOrEmpty(InputName.text) && avatarModel.Avatar.Elements.Count > 0);
        }

     
        private void OnRandomAvatarButtonClicked() {
            selectionPanel.RandomSelect();
        }

        private void OnEnable() {
            InputName.text = localPlayerInfo.NameInfo.Value;
            selectionPanel.SelectElementFromExistingAvatar(avatarModel.Avatar);
        }

        private void Start() {
            selectionPanel.StartFill(avatarModel.Avatar);
            CheckCanEnableBackButton();
        }
    }
}
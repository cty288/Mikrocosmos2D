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

        private void Awake() {
            selectionPanel = ObjCreateAvatarPanel.transform.Find("AvatarSelectionPanel")
                .GetComponent<AvatarElementSelectionPanel>();
            avatarModel = this.GetModel<IClientAvatarModel>();
        }

        private void Start() {
            selectionPanel.StartFill(avatarModel.Avatar);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;

namespace Mikrocosmos
{
    public class ChangeAvatarPanelAvatarShowcase : AbstractMikroController<Mikrocosmos> {
        private IClientAvatarModel avatarModel;

        private Avatar recordedExistingAvatar = new Avatar();

        private AvatarElementViewController avatarElement;
        
        private void Awake() {
            avatarModel = this.GetModel<IClientAvatarModel>();
            avatarElement = transform.Find("AvatarElement").GetComponent<AvatarElementViewController>();
            this.RegisterEvent<OnAvatarSingleElementSelected>(OnAvatarSingleElementSelected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnAvatarSingleElementSelected(OnAvatarSingleElementSelected e) {
            if (e.ReplacedIndex >= 0) {
                recordedExistingAvatar.RemoveElement(e.ReplacedIndex);
            }

            recordedExistingAvatar.AddElement(new AvatarElement(e.Index, Vector2.zero, e.ElementLayer));
            avatarElement.SetAvatar(recordedExistingAvatar);
        }

        private void OnEnable() {
            recordedExistingAvatar = new Avatar();
            foreach (var avatarElement in avatarModel.Avatar.Elements) {
                recordedExistingAvatar.AddElement(avatarElement.Clone());
            }
            avatarElement.SetAvatar(recordedExistingAvatar);
        }
    }
}

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

        public Avatar RecordedExistingAvatar => recordedExistingAvatar;

        private AvatarElementViewController avatarElement;
        
        private void Awake() {
            avatarModel = this.GetModel<IClientAvatarModel>();
            avatarElement = transform.Find("AvatarElement").GetComponent<AvatarElementViewController>();
            this.RegisterEvent<OnAvatarSingleElementSelected>(OnAvatarSingleElementSelected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnAvatarSingleElementSelected(OnAvatarSingleElementSelected e) {
            if (e.RefreshShowcase) {
                if (e.ReplacedIndex >= 0) {
                    recordedExistingAvatar.RemoveElement(e.ReplacedIndex);
                }
                recordedExistingAvatar.AddElement(new AvatarElement(e.Index, Vector2.zero, e.ElementLayer));
                avatarElement.SetAvatar(recordedExistingAvatar);
            }
        }

        public void SavePosition() {
            Avatar avatar = RecordedExistingAvatar;
            foreach (var avatarSubElement in avatarElement.SubElements)
            {
                if (avatarSubElement.gameObject.activeInHierarchy) {
                    RectTransform transform = avatarSubElement.GetComponent<RectTransform>();
                    Vector2 offset = new Vector2(transform.offsetMin.x, -transform.offsetMax.y);
                    avatar.UpdateOffset(avatarSubElement.Index, offset);
                }
            }
        }
        
        private void OnEnable() {
            recordedExistingAvatar = new Avatar();
            foreach (var avatarElement in avatarModel.Avatar.Elements.Values) {
                recordedExistingAvatar.AddElement(avatarElement.Clone());
            }
            avatarElement.SetAvatar(recordedExistingAvatar);
        }

        
    }
}

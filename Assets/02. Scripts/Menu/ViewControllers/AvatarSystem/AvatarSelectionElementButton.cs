using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class AvatarSelectionElementButton : AbstractMikroController<Mikrocosmos>, IPointerDownHandler, IPointerUpHandler, ICanSendEvent {
        private Image image;
        private AvatarSingleElement element;
        private AvatarElementTypeLayout layout;

        private void Awake() {
            image = GetComponent<Image>();
            element = GetComponentInParent<AvatarSingleElement>();
            layout = GetComponentInParent<AvatarElementTypeLayout>();
        }

        public void OnPointerDown(PointerEventData eventData) {
            image.DOFade(0.25f, 0.4f);
        }
        
        public void OnPointerUp(PointerEventData eventData) {
            image.DOFade(0f, 0.4f);
            layout.SelectElement(element.AssetIndex, true);
        }
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class DroppableElement : MonoBehaviour, IDragHandler {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas canvas;
        private ChangeAvatarPanelAvatarShowcase showcase;
        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
            showcase = GetComponentInParent<ChangeAvatarPanelAvatarShowcase>();
            
        }

        public void OnDrag(PointerEventData eventData) {
          //  Debug.Log($"Drag Position: {eventData.position}");
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            showcase.SavePosition();
        }
    }
}

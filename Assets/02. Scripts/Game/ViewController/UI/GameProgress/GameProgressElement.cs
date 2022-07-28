using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class GameProgressElement : MonoBehaviour {
        private RectTransform transform;
        private Slider slider;
        private Tween fillTween;
        private Image fillImage;
        private void Awake() {
            transform = GetComponent<RectTransform>();
            slider = GetComponent<Slider>();
        }

        public void StartProgress(float duration) {
            transform.sizeDelta = new Vector2(2.667f * duration, transform.sizeDelta.y);
            fillTween = slider.DOValue(1, duration).SetEase(Ease.Linear);
            fillImage = slider.fillRect.GetComponent<Image>();
        }
        public void EndProgress(Color targetColor, Action onComplete) {
            if (fillTween != null) {
                fillTween.Pause();
                fillTween.Kill(false);
            }
            if (slider.value < 1f) {
                slider.DOValue(1, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
                    fillImage.DOColor(targetColor, 0.7f).OnComplete((() => onComplete?.Invoke()));
                });
            }
            else {
                fillImage.DOColor(targetColor, 0.7f).OnComplete((() => onComplete?.Invoke())); ;
            }
            
            
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class GameProgressElement : AbstractMikroController<Mikrocosmos> {
        private RectTransform transform;
        private Slider slider;
        private Image fillImage;
        private Vector2 progressStartEnd = Vector2.zero;

        [SerializeField] private Color[] progressColors;
        private void Awake() {
            transform = GetComponent<RectTransform>();
            slider = GetComponent<Slider>();
            fillImage = slider.fillRect.GetComponent<Image>();
        }


        public void SetUpInfo(float progressStartAt, float progressEndAt) {
            progressStartEnd = new Vector2(progressStartAt, progressEndAt);
        }

        public void UpdateProgress(float totalProgress, float affinityWithTeam1) {
            //total progress is between progressStartEnd.x and progressStartEnd.y, so we need to scale it to 0 and 1
            float progress = (totalProgress - progressStartEnd.x) / (progressStartEnd.y - progressStartEnd.x);
            progress = Mathf.Clamp01(progress);
            slider.DOValue(progress, 0.3f);
            float a = fillImage.color.a;
            float scaledAffinity = (affinityWithTeam1 - 0.5f) * 2;
            if (affinityWithTeam1 < 0.5f) {
                scaledAffinity = affinityWithTeam1 * 2;
            }

            Color targetColor;
            //lerp the color between progressColors[0], white, and progressColors[1] according to affinityWithTeam1. When affinityWithTeam1 is 1, the color is progressColors[0], and when affinityWithTeam1 is 0, the color is progressColors[1]
            if (affinityWithTeam1 > 0.5f) {
                targetColor = progressColors[0];
                fillImage.DOColor(Color.Lerp(new Color(1,1,1, a), new Color(targetColor.r, targetColor.g, targetColor.b, a), scaledAffinity), 0.3f);
            }
            else {
                targetColor = progressColors[1];
                fillImage.DOColor(Color.Lerp(new Color(1, 1, 1, a), new Color(targetColor.r, targetColor.g, targetColor.b, a), scaledAffinity), 0.3f);
            }

        }

        
    }
}

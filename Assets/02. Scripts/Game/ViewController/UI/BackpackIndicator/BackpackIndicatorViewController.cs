using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class BackpackIndicatorViewController : AbstractMikroController<Mikrocosmos> {
        private Image backpackSlider;
        [SerializeField] private List<float> phrasePoints = new List<float>();

        private float increasmentPerMass = -1;
        
        private void Awake() {
            backpackSlider = transform.Find("BG/Slider").GetComponent<Image>();
            this.RegisterEvent<OnClientMassUpdated>(OnClientMassUpdated).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientMassUpdated(OnClientMassUpdated e) {
            if (increasmentPerMass < 0) {
                increasmentPerMass = (phrasePoints[2] - phrasePoints[0]) / e.OverWeightThreshold;
            }

            float targetProgress = phrasePoints[0] + e.SelfMass * increasmentPerMass;
            targetProgress = Mathf.Clamp(targetProgress, phrasePoints[0], phrasePoints[3]);
            backpackSlider.DOFillAmount(targetProgress, 0.3f);
        }
    }
}

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
    public class TempChargeSlider : AbstractMikroController<Mikrocosmos> {
        private Slider slider;
        private void Awake() {
            this.RegisterEvent<OnClientChargePercentChanged>(OnClientChargePercentChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            slider = GetComponent<Slider>();
        }

        private void OnClientChargePercentChanged(OnClientChargePercentChanged e) {
            if (e.IsLocalPlayer) {
                
                float realPercent = (e.ChargePercent * 2);
                
                if (realPercent >= 1) {
                    realPercent = -realPercent + 2;
                }

                if (realPercent == 0) {
                    slider.DOValue(0, 0.3f);
                }
                else {
                    slider.value = realPercent;
                }
               
            }
        }
    }
}

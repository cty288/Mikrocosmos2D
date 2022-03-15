using System;
using System.Collections;
using System.Collections.Generic;
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
               
                slider.value = realPercent;
            }
        }
    }
}

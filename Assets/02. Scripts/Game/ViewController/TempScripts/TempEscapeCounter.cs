using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class TempEscapeCounter : AbstractMikroController<Mikrocosmos> {
        private TMP_Text counterText;

        private void Awake() {
            counterText = GetComponent<TMP_Text>();
            this.RegisterEvent<OnEscapeCounterChanged>(OnEscapeCounterChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            

        }

        private void OnEscapeCounterChanged(OnEscapeCounterChanged e) {
            counterText.text = e.newValue.ToString();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

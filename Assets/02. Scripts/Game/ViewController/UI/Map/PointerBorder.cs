using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;

namespace Mikrocosmos
{
    public class PointerBorder : AbstractMikroController<Mikrocosmos> {

        private GameObject child;
        private Vector3 parentInitialPosition;

        private void Awake() {
            child = transform.Find("Parent").gameObject;
         //   parentInitialPosition = child.GetComponent<RectTransform>().anchoredPosition;
        }

        private void Start() {
            this.RegisterEvent<OnCameraStartShake>(OnShakeCamera).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCameraShakeEnd>(OnCameraShakeEnd).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnCameraShakeEnd(OnCameraShakeEnd obj) {
            child.gameObject.SetActive(true);
        }

        private void OnShakeCamera(OnCameraStartShake obj) {
            child.gameObject.SetActive(false);
        }

        
    }
}

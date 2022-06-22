using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos{
    public class KeepGlobalRotation : MonoBehaviour{
        [SerializeField]
        private Vector3 rotation;

        [SerializeField] private Transform positionRelativeTo;

        private Vector3 positionOffset;
        private void Awake() {
            positionOffset = positionRelativeTo.position - transform.position;
        }

        private void Update() {
            transform.rotation = Quaternion.Euler(rotation);
            
            transform.position = positionRelativeTo.position - positionOffset;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos{
    public class KeepGlobalRotation : MonoBehaviour{
        [SerializeField]
        private Vector3 rotation;

        private void Update() {
            transform.rotation = Quaternion.Euler(rotation);
        }
    }
}

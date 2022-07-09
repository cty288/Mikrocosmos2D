using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class LimitRotation : MonoBehaviour {
        [SerializeField] private float maxRotation = 90f;


        private void FixedUpdate() {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0,
                Mathf.Clamp(transform.rotation.eulerAngles.z, -maxRotation, maxRotation)));
        }
    }
}

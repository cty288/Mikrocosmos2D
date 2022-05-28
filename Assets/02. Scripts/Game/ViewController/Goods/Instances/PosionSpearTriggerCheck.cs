using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class PosionSpearTriggerCheck : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayer;
        private void OnTriggerEnter2D(Collider2D collider) {
            if (NetworkServer.active) {
                if (PhysicsUtility.IsInLayerMask(collider.gameObject, targetLayer)) {
                    if (collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum model)) {
                        GetComponentInParent<PosionSpearViewController>().OnHitObjectThisTime(collider.gameObject);
                    }
                }
            }

        }
    }
}

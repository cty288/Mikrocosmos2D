using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class LeafKnifeTrigger : MonoBehaviour {
        [SerializeField] private LayerMask targetLayer;
        private void OnTriggerEnter2D(Collider2D collider) {
            if (NetworkServer.active) {
                if (PhysicsUtility.IsInLayerMask(collider.gameObject, targetLayer)) {
                    if (collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum model)) {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                            (collider.transform.position - transform.position).normalized, 3f);


                        if (hit.collider) {
                            if (hit.collider.TryGetComponent<ICanAbsorbDamage>(out ICanAbsorbDamage absorbDamageMoodel)) {
                                if (absorbDamageMoodel.AbsorbDamage) {
                                    return;
                                }
                            }
                        }

                        GetComponentInParent<LeafKnifeViewController>().OnHitObjectThisTime(collider.gameObject);
                    }
                }
            }
        }
        
        
    }
}

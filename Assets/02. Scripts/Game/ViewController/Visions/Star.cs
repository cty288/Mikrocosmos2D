using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class Star : CanCreateVisionViewController {
        [FormerlySerializedAs("meteorBunceForce")]  [SerializeField] private float meteorBounceForce;
        [SerializeField] private LayerMask bounceAffectedLayers;
        protected override void OnClientVisionLightTurnOff() {
            
        }

        protected override void OnClientVisionLightTurnOn() {
            
        }

        private void OnCollisionEnter2D(Collision2D col) {
            if (PhysicsUtility.IsInLayerMask(col.collider.gameObject, bounceAffectedLayers)) {
                if (col.collider.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigidbody)) {

                    if (rigidbody.TryGetComponent<IGoods>(out IGoods goods)) {
                        if (goods.HookState == HookState.Freed) {
                            NetworkServer.Destroy(rigidbody.gameObject);
                        }
                    }
                    else {
                        if (rigidbody.TryGetComponent<IMeteorModel>(out IMeteorModel model))
                        {
                            Vector2 direction = rigidbody.transform.position - transform.position;
                            //get the perpendicular vector of direction
                            Vector2 ppd = Vector2.Perpendicular(direction).normalized;
                            
                            direction = direction.normalized;

                            ppd *= Random.Range(0, 2) == 1 ? 5 : -5;
                            direction += ppd;
                            rigidbody.AddForce(direction * (rigidbody.mass + rigidbody.velocity.magnitude) * meteorBounceForce, ForceMode2D.Impulse);
                        }
                    }

                    
                    
                }
            }
        }
    }
}

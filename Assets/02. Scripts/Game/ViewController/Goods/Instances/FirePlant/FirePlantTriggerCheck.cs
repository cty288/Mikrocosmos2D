using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class FirePlantTriggerCheck : MonoBehaviour
    {
        [SerializeField] private float damageCheckPeriod = 0.2f;
        [SerializeField]
        private float damageCheckTimer = 0;
        [SerializeField]
        private bool check = false;

        private FirePlantViewController mainVc;

        private void Awake() {
            mainVc = GetComponentInParent<FirePlantViewController>();
        }

        private void FixedUpdate() {
            if (NetworkServer.active) {
                check = false;
                damageCheckTimer += Time.fixedDeltaTime;
                if (damageCheckTimer >= damageCheckPeriod)
                {
                    check = true;
                    damageCheckTimer = 0;
                }
            }
          
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (NetworkServer.active) {
                if (other.TryGetComponent<IDamagable>(out IDamagable target))
                {
                    if (check) {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                            (other.transform.position - transform.position).normalized, 3f);

                        if (hit.collider) {
                            if (hit.collider.TryGetComponent<ICanAbsorbDamage>(out ICanAbsorbDamage absorbDamageMoodel)) {
                                if (absorbDamageMoodel.AbsorbDamage) {
                                    return;
                                }
                            }
                        }
                       
                        
                        Debug.Log($"Fire Planet Trigger Check: {other.gameObject.name}");
                        mainVc.DealDamageToDamagable(other.gameObject);
                    }
                }
            }
           
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class EarthBagViewController : AbstractDamagableViewController {
        private NetworkAnimator animator;

        public override void OnStartServer() {
            base.OnStartServer();
            animator = GetComponent<NetworkAnimator>();
        }

        [ServerCallback]
        public override void OnServerTakeDamage(int oldHealth, int newHealth, NetworkIdentity damageSource) {
            base.OnServerTakeDamage(oldHealth, newHealth, damageSource);
            if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                if (damageSource.transform.position.x > transform.position.x) {
                    animator.SetTrigger("HurtRight");
                }
                else {
                    animator.SetTrigger("HurtLeft");
                }
            }
        }

        [ClientRpc]
        public override void RpcOnClientHealthChange(int oldHealth, int newHealth) {
            
        }
    }
}

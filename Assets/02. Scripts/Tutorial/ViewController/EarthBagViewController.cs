using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class EarthBagViewController : AbstractDamagableViewController {
        private NetworkAnimator animator;

        private float hurtTime = 0.2f;
        private float hurtTimeLeft = 0f;

        private SpriteRenderer spriteRenderer;
        public override void OnStartServer() {
            base.OnStartServer();
            animator = GetComponent<NetworkAnimator>();
        }
        protected override void Awake()
        {
            base.Awake();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        [ServerCallback]
        public override void OnServerTakeDamage(int oldHealth, int newHealth, NetworkIdentity damageSource) {
            RpcOnClientHealthChange(oldHealth, newHealth);
            base.OnServerTakeDamage(oldHealth, newHealth, damageSource);
            if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                if (damageSource && damageSource.transform.position.x > transform.position.x) {
                    animator.SetTrigger("HurtRight");
                }
                else {
                    animator.SetTrigger("HurtLeft");
                }
            }
        }

        protected override void Update() {
            base.Update();
            if (isClient)
            {
                hurtTimeLeft -= Time.deltaTime;
                if (hurtTimeLeft < 0)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }

        [ClientRpc]
        public override void RpcOnClientHealthChange(int oldHealth, int newHealth) {
            if (newHealth < oldHealth)
            {
                hurtTimeLeft = hurtTime;
                spriteRenderer.color = Color.red;
            }
        }
    }
}

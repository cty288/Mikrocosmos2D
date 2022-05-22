using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class LeafKnifeViewController : BasicGoodsViewController {
        private NetworkAnimator animator;

        private bool hitThisTime = false;

        protected override void Awake() {
            base.Awake();
            animator = GetComponent<NetworkAnimator>();
           
        }

        [ServerCallback]
        protected override void OnServerItemUsed() {
            base.OnServerItemUsed();
            animator.SetTrigger("Use");
        }

        public void OnStopKnife() {
            if (isServer) {
                hitThisTime = false;
            }
        }

        


        [ServerCallback]
        public void OnHitObjectThisTime(GameObject gameObject) {
            if (Model.HookedByIdentity && Model.HookedByIdentity.gameObject == gameObject) {
                return;
            }
            if (!hitThisTime) {
                hitThisTime = true;
            }

            Vector2 direction = (gameObject.transform.position - transform.position).normalized;
           
            if(gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rib)) {
                rib.AddForce(GetComponent<LeafKnifeModel>().AddedForce * direction, ForceMode2D.Impulse);
            }
        }

        public void OnAnimationFinished() {
            if (isServer) {
                if (hitThisTime) {
                    GoodsModel.ReduceDurability(1);
                }
            }
        }
    }
}

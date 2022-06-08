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
        private LeafKnifeModel model;
        protected override void Awake() {
            base.Awake();
            animator = GetComponent<NetworkAnimator>();
            model = GetComponent<LeafKnifeModel>();
        }

        [ServerCallback]
        protected override void OnServerItemUsed() {
            base.OnServerItemUsed();
            animator.SetTrigger("Use");
            model.CanBeHooked = false;
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
                rib.AddForce(model.AddedForce * direction, ForceMode2D.Impulse);
            }

            if (gameObject.TryGetComponent<IDamagable>(out IDamagable damagable)) {
                damagable.TakeRawMomentum(Random.Range(12f, model.AddedMomentum),0);
                damagable.TakeRawDamage(model.AddedDamage);
            }
        }

        public void OnAnimationFinished() {
            if (isServer) {
                if (hitThisTime) {
                    Debug.Log("Leaf Knife Hit");
                    GoodsModel.ReduceDurability(1);
                    hitThisTime = false;
                    
                }
                model.CanBeHooked = true;
            }
        }
    }
}

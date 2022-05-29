using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class PosionSpearViewController : BasicGoodsViewController
    {
        private NetworkAnimator animator;

        private bool hitThisTime = false;
        private PoisonSpearModel model;
        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<NetworkAnimator>();
            model = GetComponent<PoisonSpearModel>();
        }

        [ServerCallback]
        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            animator.SetTrigger("Use");
        }


        [ServerCallback]
        public void OnHitObjectThisTime(GameObject gameObject)
        {
            if (Model.HookedByIdentity && Model.HookedByIdentity.gameObject == gameObject)
            {
                return;
            }
            if (!hitThisTime)
            {
                hitThisTime = true;
            }



            Vector2 direction = (gameObject.transform.position - transform.position).normalized;

            if (gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rib))
            {
                rib.AddForce(model.AddedForce * direction, ForceMode2D.Impulse);
            }

            if (gameObject.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                damagable.TakeRawMomentum(Random.Range(12f, model.AddedMomentum), 0);
                damagable.TakeRawDamage(model.AddedDamage);

                if(gameObject.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)){
                    buffSystem.AddBuff<PoisonBuff>(new PoisonBuff(model.PoisonTime, 1f, model.PoisonDamage, buffSystem));
                }
            }
        }

        public void OnAnimationFinished()
        {
            if (isServer)
            {
                if (hitThisTime)
                {
                    GoodsModel.ReduceDurability(1);
                    hitThisTime = false;
                }
            }
        }
    }
}

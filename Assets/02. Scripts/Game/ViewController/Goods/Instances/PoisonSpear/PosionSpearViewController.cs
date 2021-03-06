using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class PosionSpearViewController : BasicGoodsViewController
    {
        private NetworkAnimator animator;

        private bool hitThisTime = false;
        private PoisonSpearModel model;

        [SerializeField] private LayerMask affectedLayers;
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


            if (PhysicsUtility.IsInLayerMask(gameObject, affectedLayers)) {
                Vector2 direction = (gameObject.transform.position - transform.position).normalized;

                if (gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rib))
                {
                    if (rib.bodyType == RigidbodyType2D.Dynamic){
                        rib.AddForce(model.AddedForce * direction, ForceMode2D.Impulse);
                    }
                    
                }

                if (gameObject.TryGetComponent<IDamagable>(out IDamagable damagable))
                {
                    damagable.TakeRawMomentum(Random.Range(12f, model.AddedMomentum), 0);
                    DealDamage(damagable);

                    if (gameObject.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem))
                    {
                        buffSystem.AddBuff<PoisonFrequentBuff>(new PoisonFrequentBuff(model.PoisonTime, 1f, model.PoisonDamage,model.LastHookedByIdentity, buffSystem));
                    }
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

using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using UnityEngine;

namespace Mikrocosmos
{
    public class FirePlantViewController : BasicGoodsViewController{
        

        private Animator animator;

        [SerializeField] private GameObject fire1;
        [SerializeField] private GameObject fire2;

        private Trigger2DCheck fireCheck;
        private bool isUsing = false;

        [SerializeField] private int damagePer02;

      
        protected override void Awake() {
            base.Awake();
            animator = GetComponent<Animator>();
            fireCheck = GetComponentInChildren<Trigger2DCheck>();
        }

        protected override void OnServerItemUsed() {
            base.OnServerItemUsed();
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                animator.SetBool("Use", true);
                isUsing = true;
            }

            
        }


        protected override void Update()
        {
            base.Update();
          
            if (isServer)
            {
                if (!gameObject.activeSelf)
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                        animator.SetBool("Use", false);
                    }
                    return;
                }

                if (!Model.HookedByIdentity) {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                        animator.SetBool("Use", false);
                    }
                }

                if (!isUsing)
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                        animator.SetBool("Use", false);
                    }
                }

            }
        }

        public void DealDamageToDamagable(GameObject target) {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                Debug.Log($"Deal Damage to target: {target.name}");
                target.GetComponent<IDamagable>().TakeRawDamage(damagePer02);
                
            }
        }

        private void LateUpdate()
        {
            if (isServer)
            {
                isUsing = false;
            }
        }

        public void OnUsingFirePlant()
        {
            if (Model.HookedByIdentity && isServer) {
                GoodsModel.ReduceDurability(1);
            }
            fire1.gameObject.SetActive(true);
        }

        public void OpenFire2() {
            fire2.gameObject.SetActive(true);
        }
    }
}

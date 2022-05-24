using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class ThrusterViewController : BasicGoodsViewController {
        [SerializeField] private float propelForce = 50;
     

        private Animator animator;

        private bool isUsing = false;
        protected override void Awake() {
            base.Awake();
            animator = GetComponent<Animator>();
        }

        protected override void OnServerItemUsed() {
            base.OnServerItemUsed();
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("Using"))
            {
              
                    animator.SetBool("Using", true);
                    isUsing = true;

                

            }
            
            
            
        }

        
        protected override void Update() {
            base.Update();
            if (isServer) {
            
                if (!gameObject.activeSelf) {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                        animator.SetBool("Using", false);
                        
                    }
                    return;
                }

                if (!Model.HookedByIdentity) {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                        animator.SetBool("Using", false);
                    }
                }

                if (!isUsing) {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                        animator.SetBool("Using", false);
                    }
                }
            }
        }

        private void LateUpdate() {
            if (isServer) {
               isUsing = false; 
            }
        }

        public void OnUsingTruster() {
            if (Model.HookedByIdentity && isServer) {
                Model.HookedByIdentity.GetComponent<Rigidbody2D>()
                    .AddForce(propelForce  * transform.right, ForceMode2D.Impulse);
                Debug.Log($"Force: {propelForce  * transform.right}");
                GoodsModel.ReduceDurability(1);
            }
        }
    }
}

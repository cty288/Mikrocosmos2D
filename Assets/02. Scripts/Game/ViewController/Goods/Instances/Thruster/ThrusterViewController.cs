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

        [SerializeField]
        private float propellingTransformOffsetY = 2.08f;
        private float initialTransformOffsetY;
        [SerializeField] private float boostForce = 1000;
        [SerializeField]
        private float boostTime = 1f;
        private float boostTimer;
        protected override void Awake() {
            base.Awake();
            animator = GetComponent<Animator>();
            initialTransformOffsetY = HookedPositionOffset.y;
        }
        private bool boostTriggered = false;
        protected override void OnServerItemUsed() {
            base.OnServerItemUsed();
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                if (!boostTriggered) {
                    boostTriggered = true;
                    Model.HookedByIdentity.GetComponent<Rigidbody2D>()
                        .AddForce(boostForce * Model.GetTotalMass() * (Model.MaxSpeed / 30f) * transform.right, ForceMode2D.Impulse);
                    boostTimer = boostTime;
                    GoodsModel.ReduceDurability(100);
                }
                animator.SetBool("Using", true);
                isUsing = true;
                Debug.Log("Thruster ");
                HookedPositionOffset = Vector2.Lerp(HookedPositionOffset,
                    new Vector2(HookedPositionOffset.x, propellingTransformOffsetY), Time.deltaTime * 10);
            }
        }

        protected override void OnServerItemStopUsed() {
            base.OnServerItemStopUsed();
            boostTriggered = false;
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

                boostTimer -= Time.deltaTime;
                if (!isUsing) {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Using")) {
                        animator.SetBool("Using", false);
                    }
                }

                if (!isUsing) {
                    HookedPositionOffset = Vector2.Lerp(HookedPositionOffset,
                        new Vector2(HookedPositionOffset.x, initialTransformOffsetY), Time.deltaTime * 10);
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
                    .AddForce(propelForce * Model.GetTotalMass()  * (Model.MaxSpeed / 30f) * transform.right, ForceMode2D.Impulse);
                Debug.Log($"Force: {propelForce  * transform.right}");
                GoodsModel.ReduceDurability(1);
            }
        }
    }
}

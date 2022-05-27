using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnCameraViewChange {
        public int NewRadius;
    }

    public struct OnVisionRangeChange {
        public int Inner;
        public int Outer;
    }
    public class BirdCannonViewController : BasicGoodsViewController, ICanSendEvent
    {
        [SerializeField] private GameObject bullet;
        [SerializeField] private float shootForce;
        [SerializeField] private int CameraExpandRadius;

        private Transform shootPos;
        private NetworkAnimator animator;
        private NetworkIdentity previousHookedBy;

        protected override void Awake()
        {
            base.Awake();
            shootPos = transform.Find("ShootPosition");
            animator = GetComponent<NetworkAnimator>();
        }

        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                animator.SetTrigger("Shoot");
            }
        }


        public void OnBulletShoot()
        {
            if (isServer)
            {
                GameObject bullet = Instantiate(this.bullet, shootPos.transform.position, Quaternion.identity);
                bullet.GetComponent<BasicBulletViewController>().shooter = GetComponent<Collider2D>();
                bullet.GetComponent<Rigidbody2D>().AddForce(-transform.right * shootForce, ForceMode2D.Impulse);
                bullet.transform.rotation = transform.rotation;
                NetworkServer.Spawn(bullet);
            }
        }

        public void OnShootAnimationEnds() {
            if (isServer) {
                GoodsModel.ReduceDurability(1);
            }
        }


        public void OnServerStartCharge() {
            if (isServer) {
                if (Model.HookedByIdentity) {
                    previousHookedBy = Model.HookedByIdentity;
                    TargetOnStartCharge(Model.HookedByIdentity.connectionToClient);
                }
            }
        }

        public void OnServerEndCharge() {
            if (isServer) {
                if (previousHookedBy) {
                    TargetOnEndCharge(previousHookedBy.connectionToClient);
                }
            }
        }

        protected override void OnServerItemBroken() {
            base.OnServerItemBroken();
            if (previousHookedBy)
            {
                TargetOnEndCharge(previousHookedBy.connectionToClient);
            }
        }

       

        [TargetRpc]
        private void TargetOnStartCharge(NetworkConnection conn) {
            this.SendEvent<OnCameraViewChange>(new OnCameraViewChange() {
                NewRadius = CameraExpandRadius
            });
            this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
            {
                Inner = 36,
                Outer = 50
            });
        }

        [TargetRpc]
        private void TargetOnEndCharge(NetworkConnection conn) {
            this.SendEvent<OnCameraViewChange>(new OnCameraViewChange()
            {
                NewRadius = 15
            });
            this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
            {
                Inner = 18,
                Outer = 25
            });
        }

    }
}

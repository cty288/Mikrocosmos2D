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
        [SyncVar]
        private NetworkIdentity previousHookedBy;
        //private NetworkedGameObjectPool bulletPool;
        protected override void Awake()
        {
            base.Awake();
            shootPos = transform.Find("ShootPosition");
            animator = GetComponent<NetworkAnimator>();
            
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
            //bulletPool = NetworkedObjectPoolManager.Singleton.CreatePool(bullet, 10, 30);
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
                //GameObject bullet = bulletPool.Allocate();
                GameObject bullet = Instantiate(this.bullet, shootPos.transform.position, Quaternion.identity);
                bullet.transform.position = shootPos.transform.position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                
                bullet.GetComponent<BasicBulletViewController>().SetShotoer(GetComponent<Collider2D>());
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
                    Model.CanBeHooked = false;
                }
            }
        }

        public void OnServerEndCharge() {
            if (isServer) {
                if (previousHookedBy) {
                    TargetOnEndCharge(previousHookedBy.connectionToClient);
                    Model.CanBeHooked = true;
                }
            }
        }

        

        protected override void OnServerItemBroken() {
            base.OnServerItemBroken();
            if (previousHookedBy)
            {
                Debug.Log($"BirdCannon Broken: {previousHookedBy.connectionToClient}");
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
                Inner = 40,
                Outer = 55
            });
        }

        [TargetRpc]
        private void TargetOnEndCharge(NetworkConnection conn) {
            this.SendEvent<OnCameraViewChange>(new OnCameraViewChange()
            {
                NewRadius = 20
            });
            this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
            {
                Inner = 22,
                Outer = 28
            });
        }

        private void OnDestroy() {
            
            if (previousHookedBy && previousHookedBy.hasAuthority) {
                this.SendEvent<OnCameraViewChange>(new OnCameraViewChange()
                {
                    NewRadius = 20
                });
                this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
                {
                    Inner = 22,
                    Outer = 28
                });
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnCameraViewChange {
        public int RadiusAddition;
    }

    public struct OnVisionRangeChange {
        public int InnerAddition;
        public int OuterAddition;
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
              //  NetworkServer.Spawn(bullet);
                bullet.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                
                bullet.GetComponent<BasicBulletViewController>().SetShotoer(GetComponent<Collider2D>());
                bullet.GetComponent<Rigidbody2D>().AddForce(-transform.right * shootForce, ForceMode2D.Impulse);
                bullet.transform.rotation = transform.rotation;
            }

            if (isClientOnly) {
                GameObject bullet = Instantiate(this.bullet, shootPos.transform.position, Quaternion.identity);
                bullet.transform.position = shootPos.transform.position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.GetComponent<NetworkTransform>().syncPosition = false;
                //  NetworkServer.Spawn(bullet);
                bullet.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

                bullet.GetComponent<BasicBulletViewController>().SetShotoer(GetComponent<Collider2D>());
                bullet.GetComponent<Rigidbody2D>().AddForce(-transform.right * shootForce, ForceMode2D.Impulse);
                bullet.transform.rotation = transform.rotation;
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
                    //TargetOnStartCharge(Model.HookedByIdentity.connectionToClient);
                    if (Model.HookedByIdentity.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                        buffSystem.AddBuff<VisionExpansionBuff>(new VisionExpansionBuff(1f, CameraExpandRadius,
                            new Vector2(25, 25)));
                    }
                    Model.CanBeHooked = false;
                }
            }
        }

        
        public void OnServerEndCharge() {
            if (isServer) {
                if (previousHookedBy) {
                    Model.CanBeHooked = true;
                }
            }
        }

        

        /*
        protected override void OnServerItemBroken() {
            base.OnServerItemBroken();
            if (previousHookedBy)
            {
                Debug.Log($"BirdCannon Broken: {previousHookedBy.connectionToClient}");
                TargetOnEndCharge(previousHookedBy.connectionToClient);
            }
        }*/

       
        //1.25s
        
        /*
        [TargetRpc]
        private void TargetOnStartCharge(NetworkConnection conn) {
            this.SendEvent<OnCameraViewChange>(new OnCameraViewChange() {
                RadiusAddition = CameraExpandRadius
            });
            this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
            {
                InnerAddition = 25,
                OuterAddition = 25
            });
        }*/

        /*
        [TargetRpc]
        private void TargetOnEndCharge(NetworkConnection conn) {
            this.SendEvent<OnCameraViewChange>(new OnCameraViewChange()
            {
                RadiusAddition = -CameraExpandRadius
            });
            this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
            {
                InnerAddition = -25,
                OuterAddition = -25
            });
        }*/

        /*
        private void OnDestroy() {
            
            if (previousHookedBy && previousHookedBy.hasAuthority) {
                this.SendEvent<OnCameraViewChange>(new OnCameraViewChange()
                {
                    RadiusAddition = -CameraExpandRadius
                });
                this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
                {
                    InnerAddition = -25,
                    OuterAddition = -25
                });
            }
        }*/
    }
}

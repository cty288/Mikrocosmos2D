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
        [SerializeField]
        private bool startUsed = false;
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
            if (!startUsed) {
                if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                    startUsed = true;
                    animator.SetTrigger("Shoot");
                    OnServerStartCharge();
                }
            }

        }

        protected override void Update() {
            base.Update();
            if (startUsed) {
                if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Charge") || animator.animator.GetCurrentAnimatorStateInfo(0).IsName("ChargeLoop")) {
                    OnServerStartCharge();
                }
            }
        }

        protected override void OnServerItemStopUsed() {
            base.OnServerItemStopUsed();
            if (startUsed) {
              //  if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Charge") || animator.animator.GetCurrentAnimatorStateInfo(0).IsName("ChargeLoop")) {
                    startUsed = false;
                    animator.SetTrigger("ChargeEnd");
                // }
                Model.CanBeHooked = true;
            }
        }


        public void OnBulletShoot()
        {
            if (isServer)
            {
               
                GameObject bullet = Instantiate(this.bullet, shootPos.transform.position, Quaternion.identity);
                bullet.transform.position = shootPos.transform.position;
                bullet.transform.rotation = Quaternion.identity;
                NetworkServer.Spawn(bullet);
                bullet.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

                IBuffSystem buffSystem = null;
                if (Owner) {
                    Owner.TryGetComponent<IBuffSystem>(out buffSystem);
                }

                
                bullet.GetComponent<BasicBulletViewController>().SetShotoer(Model.HookedByIdentity,GetComponent<Collider2D>(), buffSystem);
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
                        buffSystem.AddBuff<VisionExpansionBuff>(new VisionExpansionBuff(0.9f, CameraExpandRadius,
                            new Vector2(25, 25), true));
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

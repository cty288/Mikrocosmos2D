using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class CactusShotGunViewController : BasicGoodsViewController
    {
        [SerializeField] private GameObject bullet;
        [SerializeField] private float shootForce;

      
        private NetworkAnimator animator;
        private NetworkedGameObjectPool bulletPool;
        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<NetworkAnimator>();
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
            bulletPool = NetworkedObjectPoolManager.Singleton.CreatePool(bullet, 20, 50);
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
            if (isServer) {
                for (int i = 0; i < transform.childCount; i++) {
                    Transform shootTransform = transform.GetChild(i);
                    GameObject bullet = bulletPool.Allocate();
                    //GameObject bullet = Instantiate(this.bullet, shootTransform.position, shootTransform.rotation);
                    bullet.transform.position = shootTransform.position;
                    bullet.transform.rotation = shootTransform.rotation;
                    bullet.GetComponentInChildren<TrailRenderer>().Clear();
                    bullet.GetComponentInChildren<TrailRenderer>().emitting = true;
                    IBuffSystem buffSystem = null;
                    if (Owner)
                    {
                        Owner.TryGetComponent<IBuffSystem>(out buffSystem);
                    }
                    
                    bullet.GetComponent<BasicBulletViewController>().SetShotoer(Model.LastHookedByIdentity, GetComponent<Collider2D>(), buffSystem);
                    bullet.GetComponent<Rigidbody2D>().AddForce(-bullet.transform.right * shootForce, ForceMode2D.Impulse);
                    NetworkServer.Spawn(bullet);
                }
            }
        }

        

        public void OnShootAnimationEnds()
        {
            if (isServer)
            {
                GoodsModel.ReduceDurability(1);
            }
        }
    }
}

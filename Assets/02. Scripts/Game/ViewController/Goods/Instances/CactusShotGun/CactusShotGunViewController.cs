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
        protected override void Awake()
        {
            base.Awake();
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
            if (isServer) {
                for (int i = 0; i < transform.childCount; i++) {
                    Transform shootTransform = transform.GetChild(i);
                    GameObject bullet = Instantiate(this.bullet, shootTransform.position, shootTransform.rotation);
                    bullet.GetComponent<BasicBulletViewController>().shooter = GetComponent<Collider2D>();
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

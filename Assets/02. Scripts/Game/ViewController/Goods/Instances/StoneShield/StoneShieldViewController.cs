using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class StoneShieldViewController : BasicGoodsViewController
    {
        [SerializeField] private GameObject wave;
        [SerializeField] private float shootForce;

        [SerializeField] private int currCharge;
        [SerializeField] private int maxCharge;

        private Transform shootPos;
        private Animator animator;

        protected override void Awake()
        {
            base.Awake();
            shootPos = transform.Find("ShootPosition");
            animator = GetComponent<Animator>();
        }

        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                animator.SetBool("Use", true);
                animator.SetBool("Shoot",false);
            }
        }

        public void OnBulletShoot()
        {
            if (isServer)
            {
                GameObject wave = Instantiate(this.wave, shootPos.transform.position, Quaternion.identity);
                wave.GetComponent<JellyBulletViewController>().shooter = GetComponent<Collider2D>();
                wave.GetComponent<Rigidbody2D>().AddForce(-transform.right * shootForce, ForceMode2D.Impulse);
                wave.transform.rotation = transform.rotation;
                NetworkServer.Spawn(wave);
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

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class EarthBagAggressiveViewController : AbstractDamagableViewController {
        private NetworkAnimator animator;
        private SpriteRenderer spriteRenderer;

        private float hurtTime = 0.2f;
        private float hurtTimeLeft = 0f;


        [SerializeField] private float attackTimeInterval = 2f;
        private float attackTimer = 0f;

        [SerializeField] private GameObject bulletPrefab;

        private Transform bulletSpawnPoint;
        private Trigger2DCheck triggerCheck;

        [SerializeField] private float shootForce = 100f;

        
        protected override void Awake() {
            base.Awake();
            spriteRenderer = GetComponent<SpriteRenderer>();
            bulletSpawnPoint = transform.Find("ShootPos");
            triggerCheck = transform.parent.Find("PlayerDetectTrigger").GetComponent<Trigger2DCheck>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            animator = GetComponent<NetworkAnimator>();
        }


        protected override void Update() {
            base.Update();
            if (isClient) {
                hurtTimeLeft -= Time.deltaTime;
                if (hurtTimeLeft < 0) {
                    spriteRenderer.color = Color.white;
                }
            }

            if (isServer) {
                if (triggerCheck.Triggered) {
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0) {
                        attackTimer = attackTimeInterval;
                        animator.SetTrigger("Attack");
                    }
                }
            }
        }


        public void OnAnimationBulletShoot() {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            bullet.GetComponent<BasicBulletViewController>().SetShotoer(netIdentity, GetComponent<Collider2D>());
            bullet.GetComponent<Rigidbody2D>().AddForce(-bullet.transform.right * shootForce, ForceMode2D.Impulse);
            NetworkServer.Spawn(bullet);
        }
        public override void RpcOnClientHealthChange(int oldHealth, int newHealth) {
            if (newHealth < oldHealth) {
                hurtTimeLeft = hurtTime;
                spriteRenderer.color = Color.red;
            }
        }
    }
}

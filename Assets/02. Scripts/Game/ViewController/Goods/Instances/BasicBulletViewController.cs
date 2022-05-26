using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class BasicBulletViewController : BasicEntityViewController
    {
        [HideInInspector] public float Force;

        [HideInInspector]
        public Collider2D shooter;

       private NetworkAnimator animator;
        protected void Start() {
            base.Awake();
            animator = GetComponent<NetworkAnimator>();
            if (shooter) {
                Physics2D.IgnoreCollision(shooter, GetComponent<Collider2D>(), true);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (isServer) {
                if (collision.collider) {
                    if (collision.collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum entity)) {
                        StartCoroutine(SetVelocityToZero());
                        animator.SetTrigger("Hit");
                        if (entity is IDamagable damagable) {
                            BulletModel model = GetComponent<BulletModel>();
                            Debug.Log("Bullet Speed: " + rigidbody.velocity.magnitude);
                            damagable.TakeRawDamage(
                                (Mathf.RoundToInt(model.Damage * (rigidbody.velocity.magnitude / model.MaxSpeed))));
                        }
                    }
                }
            }
        }

        private IEnumerator SetVelocityToZero() {
            yield return null;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        public void OnAnimationDone() {
            if (isServer) {
                NetworkServer.Destroy(this.gameObject);
            }
            
        }
    }
}

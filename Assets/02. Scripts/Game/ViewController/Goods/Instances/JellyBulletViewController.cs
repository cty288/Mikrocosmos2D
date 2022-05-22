using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class JellyBulletViewController : BasicEntityViewController
    {
        [HideInInspector] public float Force;

        [HideInInspector]
        public Collider2D shooter;

        [SerializeField] private NetworkAnimator animator;
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

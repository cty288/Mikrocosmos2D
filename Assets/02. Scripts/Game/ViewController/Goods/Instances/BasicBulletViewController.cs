using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    //[RequireComponent(typeof(PoolableNetworkedGameObject))]
    public class BasicBulletViewController : BasicEntityViewController
    {
        [HideInInspector] public float Force;

        [HideInInspector]
        public Collider2D shooter;

       private NetworkAnimator animator;

       private PoolableNetworkedGameObject poolable;

       [SerializeField] private bool destroyWhenHit = false;
        protected void Start() {
            base.Awake();
            animator = GetComponent<NetworkAnimator>();
           
            if (shooter) {
                Physics2D.IgnoreCollision(shooter, GetComponent<Collider2D>(), true);
            }
        }

        public void SetShotoer(Collider2D shooter) {
            this.shooter = shooter;
            Physics2D.IgnoreCollision(shooter, GetComponent<Collider2D>(), true);
        }
       

        public override void OnReset() {
            base.OnReset();
            if (shooter) {
                Physics2D.IgnoreCollision(shooter, GetComponent<Collider2D>(), false);
                shooter = null;
            }
            rigidbody.velocity = Vector2.zero;
        }

        public override void OnStartServer() {
            base.OnStartServer();
            poolable = GetComponent<PoolableNetworkedGameObject>();
            this.GetSystem<ITimeSystem>().AddDelayTask(30, () => {
                if (this) {
                    //poolable.RecycleToCache();
                    //NetworkServer.Destroy(gameObject);
                }
            });

        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (isServer) {
                if (collision.collider) {
                    if (collision.collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum entity)) {
                        StartCoroutine(SetVelocityToZero());
                        
                       
                        if (entity is IDamagable damagable) {
                            BulletModel model = GetComponent<BulletModel>();
                            //Debug.Log("Bullet Speed: " + rigidbody.velocity.magnitude);
                            damagable.TakeRawDamage(
                                (Mathf.RoundToInt( model.Damage * (Mathf.Min(3*rigidbody.velocity.magnitude / model.MaxSpeed,1)))));
                           
                        }
                        
                    }
                    animator.SetTrigger("Hit");
                    if (destroyWhenHit)
                    {
                        
                        // NetworkServer.Destroy(this.gameObject);
                        rigidbody.velocity = Vector2.zero;
                        if (poolable) {
                            poolable.RecycleToCache();
                        }
                        else {
                            NetworkServer.Destroy(this.gameObject);
                        }
                       
                    }
                }
            }
        }

        private IEnumerator SetVelocityToZero() {
            yield return null;
            if (this) {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
           
        }

        public void OnAnimationDone() {
            if (isServer) {
                //NetworkServer.Destroy(this.gameObject);
                rigidbody.velocity = Vector2.zero;
                if (poolable) {
                    
                    poolable.RecycleToCache();
                }
                else {
                    NetworkServer.Destroy(this.gameObject);
                }
               
            }
            
        }
    }
}

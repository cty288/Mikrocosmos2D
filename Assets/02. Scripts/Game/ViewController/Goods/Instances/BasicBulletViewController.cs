using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private Collider2D shooter;

        private List<Collider2D> shooters = new List<Collider2D>(); 

       protected NetworkAnimator animator;

       private PoolableNetworkedGameObject poolable;

       [SerializeField] private bool destroyWhenHit = false;

       [SerializeField] private bool damageReduceBySpeed = true;

       private int additionalDamageFactor = 0;
        protected void Start() {
            base.Awake();
            animator = GetComponent<NetworkAnimator>();
           
            if (shooter) {
                Physics2D.IgnoreCollision(shooter, GetComponent<Collider2D>(), true);
            }
        }

        public void SetShotoer(Collider2D shooter, IBuffSystem buffSystem = null) {
            this.shooter = shooter;
            Physics2D.IgnoreCollision(shooter, GetComponent<Collider2D>(), true);
            if (buffSystem != null) {
                if (buffSystem.HasBuff<PermanentPowerUpBuff>(out PermanentPowerUpBuff powerBuff)) {
                    additionalDamageFactor = Mathf.RoundToInt(powerBuff.CurrentLevel * powerBuff.AdditionalDamageAdditionPercentage);
                }
            }
            shooters.Add(shooter);
           
        }

        private void OnEnable() {
            hit = false;
        }

        public override void OnReset() {
            base.OnReset();
            foreach (Collider2D s in shooters) {
                if (s) {
                    Physics2D.IgnoreCollision(s, GetComponent<Collider2D>(), false);
                }
             
            }

            hit = false;
            shooter = null;
            shooters.Clear();
            
            rigidbody.velocity = Vector2.zero;
        }

        public override void OnStartServer() {
            base.OnStartServer();
            poolable = GetComponent<PoolableNetworkedGameObject>();
        }


        protected bool hit = false;
        protected override  void OnCollisionEnter2D(Collision2D collision) {
            if (isServer) {
                if (collision.collider ) {
                   
                    if (collision.collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum entity)) {
                        //StartCoroutine(SetVelocityToZero());
                        
                        BulletModel model = GetComponent<BulletModel>();
                        int damage = (Mathf.RoundToInt(model.Damage *
                                                       (Mathf.Min(  rigidbody.velocity.magnitude / model.MaxSpeed,
                                                           2))));

                        if (!damageReduceBySpeed) {
                            damage = model.Damage;
                        }

                        damage += (damage * additionalDamageFactor);
                        if (entity is IDamagable damagable) {

                            //Debug.Log("Bullet Speed: " + rigidbody.velocity.magnitude);
                            damagable.TakeRawDamage(damage);
                        }


                        bool dealDamageToOwner = true;
                        if (entity is ICanAbsorbDamage canAbsorbDamage) {
                            if (canAbsorbDamage.AbsorbDamage) {
                                dealDamageToOwner = false;
                                canAbsorbDamage.OnAbsorbDamage(damage);
                            }
                        }
                        
                        if (entity is IHookable hookable && dealDamageToOwner) {
                            if (hookable.CanBeHooked && hookable.HookedByIdentity) {
                                hookable.HookedByIdentity.GetComponent<IDamagable>().TakeRawDamage(damage);
                            }
                            
                        }

                    }
                    Debug.Log($"Bullet Destroy {gameObject.name}");
                   
                }
                animator.SetTrigger("Hit");
                DestroySelf();
            }
        }

        protected override void Update() {
            base.Update();
            if (isServer) {
                if (rigidbody.velocity.magnitude < 1f && animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                    animator.SetTrigger("Hit");
                   
                    DestroySelf();
                }
            }
        }

        /*
        private IEnumerator DestroySelf() {
            yield return new WaitForSeconds(0.2f);
            if (destroyWhenHit)
            {

                // NetworkServer.Destroy(this.gameObject);
                rigidbody.velocity = Vector2.zero;
                if (poolable)
                {
                    poolable.RecycleToCache();
                    NetworkServer.UnSpawn(gameObject);
                }
                else
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }
        }*/
        private void DestroySelf()
        {
            if (destroyWhenHit)
            {

                // NetworkServer.Destroy(this.gameObject);
                rigidbody.velocity = Vector2.zero;
                if (poolable)
                {
                    poolable.RecycleToCache();
                    NetworkServer.UnSpawn(gameObject);
                }
                else
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }
            else {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }




        /*
        private void LateUpdate() {
       
            List<Collider2D> colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f).ToList();
           
            foreach (Collider2D collider in colliders) {
                if (!collider) {
                    continue;
                }
                if (collider.CompareTag("Bullet")) {
                    continue;
                }
                
                
                if (collider != shooter && !collider.isTrigger) {
                    
                    NetworkIdentity ownerPlayer = Model.HookedByIdentity;

                    if (ownerPlayer == null || collider != ownerPlayer.GetComponent<Collider2D>()) {


                        if (collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum entity))
                        {
                            StartCoroutine(SetVelocityToZero());

                            BulletModel model = GetComponent<BulletModel>();
                            int damage = (Mathf.RoundToInt(model.Damage *
                                                           (Mathf.Min(3 * rigidbody.velocity.magnitude / model.MaxSpeed,
                                                               2))));

                            if (!damageReduceBySpeed)
                            {
                                damage = model.Damage;
                            }

                            if (entity is IDamagable damagable)
                            {

                                //Debug.Log("Bullet Speed: " + rigidbody.velocity.magnitude);
                                damagable.TakeRawDamage(damage);
                            }


                            bool dealDamageToOwner = true;
                            if (entity is ICanAbsorbDamage canAbsorbDamage)
                            {
                                if (canAbsorbDamage.AbsorbDamage)
                                {
                                    dealDamageToOwner = false;
                                    canAbsorbDamage.OnAbsorbDamage(damage);
                                }
                            }

                            if (entity is IHookable hookable && dealDamageToOwner)
                            {
                                if (hookable.CanBeHooked && hookable.HookedByIdentity)
                                {
                                    hookable.HookedByIdentity.GetComponent<IDamagable>().TakeRawDamage(damage);
                                }

                            }

                        }
                        Debug.Log($"Bullet Destroy {gameObject.name}");
                        


                        animator.SetTrigger("Hit");
                        if (destroyWhenHit) {
                            // NetworkServer.Destroy(this.gameObject);
                            rigidbody.velocity = Vector2.zero;
                            if (poolable) {
                                poolable.RecycleToCache();
                                NetworkServer.UnSpawn(gameObject);
                            }
                            else {
                                NetworkServer.Destroy(this.gameObject);
                            }
                        }
                        break;
                    }
                    
                }
            }

        }*/


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
                    NetworkServer.UnSpawn(gameObject);
                }
                else {
                    NetworkServer.Destroy(this.gameObject);
                }
               
            }
            
        }
    }
}

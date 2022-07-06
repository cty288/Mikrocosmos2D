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
       
       
      
      

        private Transform shootPos;
        private NetworkAnimator animator;

        
        
        private StoneShieldModel model;

        private NetworkedGameObjectPool bulletPool;


        protected override void Awake()
        {
            base.Awake();
            shootPos = transform.Find("ShootPosition");
            animator = GetComponent<NetworkAnimator>();
           
            model = GetComponent<StoneShieldModel>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            bulletPool = NetworkedObjectPoolManager.Singleton.CreatePool(wave, 5, 10);
        }
        
        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
           // GoodsModel.ReduceDurability(1); //ReduceDurability while using
            OnShieldExpanded();
        }

        private bool isUsing = false;
        public void OnShieldExpanded()
        {
            
            
           // Debug.Log("OnExpansionB");
            if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                Debug.Log("OnExpansion");
                animator.SetTrigger("Use");
                animator.ResetTrigger("NoDamage");
                animator.ResetTrigger("Shoot");
            }
            else {
                Model.CanBeHooked = false;
                model.AbsorbDamage = true;
                animator.ResetTrigger("Use");
                isUsing = true;
            }
        }

        public void OnWaveShoot()
        {
            if (isServer) {
                if (!animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Expanded")) {
                    animator.SetTrigger("NoDamage");
                }
                else {
                    Debug.Log("StoneShieldMouseUp Effective");
                    if (model.CurrCharge / 2 > 0)
                    {
                        Debug.Log("Charged. Shoot");
                        animator.SetTrigger("Shoot");
                        
                        GameObject wave = bulletPool.Allocate();
                        ///GameObject wave = Instantiate(this.wave);
                        wave.transform.position = shootPos.position;
                        wave.transform.rotation = transform.rotation;
                        IBuffSystem buffSystem = null;
                        if (Owner)
                        {
                            Owner.TryGetComponent<IBuffSystem>(out buffSystem);
                        }
                        
                    

                        wave.GetComponent<BasicBulletViewController>()
                            .SetShotoer(model.HookedByIdentity, GetComponent<Collider2D>(), buffSystem);

                        
                        wave.GetComponent<StoneShieldBulletViewController>().Damage = model.CurrCharge / 2;
                        NetworkServer.Spawn(wave);
                    }
                    else
                    {
                        animator.SetTrigger("NoDamage");
                    }
                }
                
              
              
                model.CurrCharge = 0;
            }
        }

        protected override void OnCollisionEnter2D(Collision2D collision) {
            base.OnCollisionEnter2D(collision);
            if (isServer) {
                if (isUsing &&  collision.collider.TryGetComponent<PlayerSpaceship>(out var spaceship)) {
                    model.ReduceDurability(20);
                    spaceship.GetComponent<Rigidbody2D>().AddForce(-transform.right * model.BounceForce, ForceMode2D.Impulse);
                }
            }
        }


        protected override void Update() {
            base.Update();
            
        }

     

        protected override void OnServerItemStopUsed() {
            base.OnServerItemStopUsed();
            model.AbsorbDamage = false;
            OnWaveShoot();
            isUsing = false;
            Model.CanBeHooked = true;            
        }

        
       
    }
}

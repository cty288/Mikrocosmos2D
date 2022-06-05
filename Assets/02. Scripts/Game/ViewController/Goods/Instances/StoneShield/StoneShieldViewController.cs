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

        private bool isUsing = false;
        private bool mouseUpTriggered = false;

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
            isUsing = true;
            mouseUpTriggered = false;
        }

        public void OnShieldExpanded()
        {
            
            
           // Debug.Log("OnExpansionB");
            if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                Debug.Log("OnExpansion");
                animator.SetTrigger("Use");
            }
            else {
                model.AbsorbDamage = true;
                animator.ResetTrigger("Use");
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
                        if (model.HookedByIdentity) {
                            wave.GetComponent<BasicBulletViewController>()
                                .SetShotoer(model.HookedByIdentity.GetComponent<Collider2D>());
                        }
                        
                        wave.GetComponent<StoneShieldBulletViewController>().shooter = GetComponent<Collider2D>();
                        wave.GetComponent<StoneShieldBulletViewController>().Damage = model.CurrCharge / 2;
                        NetworkServer.Spawn(wave);
                    }
                    else
                    {
                        animator.SetTrigger("NoDamage");
                    }
                }
                
              
                isUsing = false;
                mouseUpTriggered = true;
                model.CurrCharge = 0;
            }
        }

        protected override void Update() {
            base.Update();
            
        }

        private void LateUpdate() {
            if (isServer) {
                if (!isUsing && !mouseUpTriggered) {
                    mouseUpTriggered = true;
                    //TODO:
                    OnItemUseMouseUp();
                }
                isUsing = false;
            }
        }

        [ServerCallback]
        private void OnItemUseMouseUp() {
            model.AbsorbDamage = false;
            OnWaveShoot();
           
        }
    }
}

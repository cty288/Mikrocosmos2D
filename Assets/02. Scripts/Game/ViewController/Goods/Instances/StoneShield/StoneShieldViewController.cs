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
        [SerializeField] private int threshold;

        [SerializeField] private GameObject childTrigger;
        [SerializeField] private StoneShieldTrigger SHT;

        private Transform shootPos;
        private Animator animator;

        private bool isUsing = false;
        private bool mouseUpTriggered = false;

        /// <summary>
        /// 按下鼠标左键展开护盾--OnShieldExpanded
        /// 松开鼠标左键时进行判定 若达到阈值发射冲击波--OnWaveShoot
        /// </summary>


        protected override void Awake()
        {
            base.Awake();
            shootPos = transform.Find("ShootPosition");
            animator = GetComponent<Animator>();
            childTrigger = this.gameObject.transform.GetChild(0).gameObject;
            SHT = childTrigger.GetComponent<StoneShieldTrigger>();
        

        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            GoodsModel.ReduceDurability(1); //ReduceDurability while using
            OnShieldExpanded();
            isUsing = true;
            mouseUpTriggered = false;
        }

        public void OnShieldExpanded()
        {
            currCharge = SHT.GetCurrCharge();
           // Debug.Log("OnExpansionB");
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                Debug.Log("OnExpansion");
                animator.SetBool("Use", true);
                animator.SetBool("Shoot", false);
            }
        }

        public void OnWaveShoot()
        {
            if (isServer)
            {                
                Debug.Log("Charged. Shoot");
                GameObject wave = Instantiate(this.wave, shootPos.transform.position, Quaternion.identity);
                wave.GetComponent<JellyBulletViewController>().shooter = GetComponent<Collider2D>();
                wave.GetComponent<Rigidbody2D>().AddForce(-transform.right * shootForce, ForceMode2D.Impulse);
                wave.transform.rotation = transform.rotation;
                NetworkServer.Spawn(wave);
                isUsing = false;
                mouseUpTriggered = true;
            }
        }

        private void LateUpdate() {
            if (isServer) {
                if (!isUsing && !mouseUpTriggered) {
                    mouseUpTriggered = true;
                    //TODO:
                    if (Model.HookedByIdentity != null) {
                        OnItemUseMouseUp();
                    }
                  
                }
                
                isUsing = false;
            }
        }

        private void OnItemUseMouseUp() {
            OnWaveShoot();
        }
    }
}

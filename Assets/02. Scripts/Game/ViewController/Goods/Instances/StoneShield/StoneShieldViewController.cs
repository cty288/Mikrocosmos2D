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
      

        private Transform shootPos;
        private NetworkAnimator animator;

        private bool isUsing = false;
        private bool mouseUpTriggered = false;

        private StoneShieldModel model;

        /// <summary>
        /// 按下鼠标左键展开护盾--OnShieldExpanded
        /// 松开鼠标左键时进行判定 若达到阈值发射冲击波--OnWaveShoot
        /// </summary>


        protected override void Awake()
        {
            base.Awake();
            shootPos = transform.Find("ShootPosition");
            animator = GetComponent<NetworkAnimator>();
            childTrigger = this.gameObject.transform.GetChild(0).gameObject;
            model = GetComponent<StoneShieldModel>();
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
            currCharge = model.CurrCharge;
            
           // Debug.Log("OnExpansionB");
            if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                Debug.Log("OnExpansion");
                animator.SetTrigger("Use");
            }
            else {
                animator.ResetTrigger("Use");
            }
        }

        public void OnWaveShoot()
        {
            if (isServer)
            {                
                //Debug.Log("Charged. Shoot");
                animator.SetTrigger("Shoot");
                GameObject wave = Instantiate(this.wave, shootPos.transform.position, Quaternion.identity);
                wave.GetComponent<BasicBulletViewController>().shooter = GetComponent<Collider2D>();
                wave.GetComponent<Rigidbody2D>().AddForce(-transform.right * shootForce, ForceMode2D.Impulse);
                wave.transform.rotation = transform.rotation;
                NetworkServer.Spawn(wave);
                isUsing = false;
                mouseUpTriggered = true;
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
            OnWaveShoot();
        }
    }
}

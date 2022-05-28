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

        /// <summary>
        /// ����������չ������--OnShieldExpanded
        /// �ɿ�������ʱ�����ж� ���ﵽ��ֵ��������--OnWaveShoot
        /// </summary>

        protected override void Awake()
        {
            base.Awake();
            shootPos = transform.Find("ShootPosition");
            animator = GetComponent<Animator>();
            childTrigger = this.gameObject.transform.GetChild(0).gameObject;
            SHT = childTrigger.GetComponent<StoneShieldTrigger>();
        }

        private void Update()
        {
            if (hasAuthority)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (currCharge >= threshold)
                    {
                        OnWaveShoot();
                    }
                }
            }
        }

        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            GoodsModel.ReduceDurability(1); //ReduceDurability while using
            OnShieldExpanded();
        }

        public void OnShieldExpanded()
        {
            currCharge = SHT.GetCurrCharge();
            Debug.Log("OnExpansionB");
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
            }
        }
    }
}

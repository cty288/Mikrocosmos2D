using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{

    public struct OnMeteorDestroyed {
        public GameObject Meteor;
    }
    //obj pool
    public class MeteorViewController : AbstractCanCreateShadeEntity, IDamagableViewController
    {
        private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject damageParticle;
        [SerializeField] private GameObject dieParticle;

       
        protected override void Awake()
        {
            base.Awake();
            DamagableModel = GetComponent<IDamagable>();
            spriteRenderer = GetComponent<SpriteRenderer>();
          
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnEntityTakeDamage>(OnEntityTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEntityTakeDamage(OnEntityTakeDamage e)
        {
            if (e.Entity == Model) {
                OnServerTakeDamage(e.OldHealth, e.NewHealth);
            }
        }


        [ServerCallback]
        public virtual void OnServerTakeDamage(int oldHealth, int newHealth) {
            RpcOnClientTakeDamage(oldHealth, newHealth);
            if (newHealth <= 0) {
                if (Model.HookedByIdentity) {
                    Model.UnHook();
                }

                if (GetComponent<PoolableNetworkedGameObject>().Pool != null) {
                    NetworkedObjectPoolManager.Singleton.Recycle(gameObject);
                }else {
                    NetworkServer.Destroy(gameObject);
                }
               
            }
        }

        public override void OnStopClient() {
            base.OnStopClient();
           // if (DamagableModel.CurrentHealth <= 0) {
                GameObject.Instantiate(dieParticle, transform.position, Quaternion.identity);
          //  }
        }

        [ClientRpc]
        public virtual void RpcOnClientTakeDamage(int oldHealth, int newHealth) {
            if (newHealth < oldHealth) {
                int damage =Mathf.Abs(newHealth - oldHealth);
                spriteRenderer.DOBlendableColor(new Color(0.6f, 0.6f, 0.6f), 0.1f).SetLoops(2, LoopType.Yoyo);
                if (damage >= 5) {
                    GameObject.Instantiate(damageParticle, transform.position, Quaternion.identity);
                }

               
              
            }
              
        }


        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Border")) {
                this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => {
                    GetComponent<Collider2D>().isTrigger = false;
                });

            }
        }

        

        public IDamagable DamagableModel { get; protected set; }
    }
}

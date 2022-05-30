using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{
    
    public class MeteorViewController : AbstractCanCreateShadeEntity, IDamagableViewController
    {
        private SpriteRenderer spriteRenderer;
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
        public virtual void OnServerTakeDamage(int oldHealth, int newHealth)
        {
            RpcOnClientTakeDamage(oldHealth, newHealth);
        }


        [ClientRpc]
        public virtual void RpcOnClientTakeDamage(int oldHealth, int newHealth) {
            if (newHealth < oldHealth) {
                spriteRenderer.DOBlendableColor(new Color(0.6f, 0.6f, 0.6f), 0.1f).SetLoops(2, LoopType.Yoyo);
            }
              
        }

        

        public IDamagable DamagableModel { get; protected set; }
    }
}

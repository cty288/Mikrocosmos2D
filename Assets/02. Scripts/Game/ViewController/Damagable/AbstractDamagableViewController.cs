using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public abstract class AbstractDamagableViewController : BasicEntityViewController, IDamagableViewController {
        protected override void Awake() {
            base.Awake();
            DamagableModel = GetComponent<IDamagable>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnEntityTakeDamage>(OnEntityTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEntityTakeDamage(OnEntityTakeDamage e) {
            if (e.Entity == Model) {
                OnServerHealthChange(e.NewHealth);
            }
        }


        [ServerCallback]
        public virtual void OnServerHealthChange(int newHealth) {
            RpcOnClientHealthChange(newHealth);
        }


        
        public abstract void RpcOnClientHealthChange(int newHealth);

        public IDamagable DamagableModel { get; protected set; } 
    }
}
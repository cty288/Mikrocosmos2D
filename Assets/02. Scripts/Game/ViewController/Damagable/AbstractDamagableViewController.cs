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
                OnServerHealthChange(e.OldHealth, e.NewHealth);
                
            }
        }


        [ServerCallback]
        public virtual void OnServerHealthChange(int oldHealth, int newHealth) {
            RpcOnClientHealthChange(oldHealth, newHealth);
        }


        
        public abstract void RpcOnClientHealthChange(int oldHealth, int newHealth);

        public IDamagable DamagableModel { get; protected set; } 
    }
}

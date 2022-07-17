using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

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
                OnServerTakeDamage(e.OldHealth, e.NewHealth, e.DamageSource);
                
            }
        }


        [ServerCallback]
        public virtual void OnServerTakeDamage(int oldHealth, int newHealth, NetworkIdentity damageSource) {
            RpcOnClientHealthChange(oldHealth, newHealth);
        }


        
        public abstract void RpcOnClientHealthChange(int oldHealth, int newHealth);

        public IDamagable DamagableModel { get; protected set; }
        [field:SerializeField]
        public Vector2 DamageTextSpawnOffset { get; protected set; }
    }
}

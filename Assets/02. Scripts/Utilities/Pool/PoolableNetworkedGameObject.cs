using System.Collections;
using System.Collections.Generic;
using MikroFramework.Pool;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Mikrocosmos
{
    public class PoolableNetworkedGameObject : PoolableGameObject {
        public UnityEvent OnInitialize;
        public UnityEvent OnRecycle;
        public override void OnInit() {
           OnInitialize?.Invoke();
        }

        public override void OnRecycled() {
            OnRecycle?.Invoke();
            if (NetworkServer.active) {
                if (TryGetComponent<IEntityViewController>(out var entityViewController)) {
                    entityViewController.ResetViewController();
                }
                
                if (TryGetComponent<IEntity>(out var entity)) {
                    entity.ResetEntity();
                }
            }
        }
    }
}

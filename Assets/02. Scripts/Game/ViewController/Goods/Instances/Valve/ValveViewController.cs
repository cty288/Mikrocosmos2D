using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class ValveViewController : BasicGoodsViewController
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnServerObjectHookStateChanged>(OnHookStatusChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnHookStatusChanged(OnServerObjectHookStateChanged e)
        {
            if (e.Identity == netIdentity && e.HookState == HookState.Freed) {
                StartCoroutine(DestroySelf());
                ClientMessagerForDestroyedObjects.Singleton.ServerSpawnParticleOnClient(transform.position, 0);
            }
        }

        private IEnumerator DestroySelf()
        {
            yield return new WaitForEndOfFrame();
            NetworkServer.Destroy(gameObject);
        }
    }
}

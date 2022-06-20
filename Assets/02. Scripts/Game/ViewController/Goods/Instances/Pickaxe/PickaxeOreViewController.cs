using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class PickaxeOreViewController : BasicGoodsViewController {
        [SerializeField] private GameObject pickaxePrefab;
        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnServerObjectHookStateChanged>(OnHookStatusChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnHookStatusChanged(OnServerObjectHookStateChanged e) {
            if (e.Identity == netIdentity && e.HookState == HookState.Freed) {
                
                GameObject pickaxe = Instantiate(pickaxePrefab, transform.position, Quaternion.identity);
                StartCoroutine(HookPickaxeAndDestroySelf(pickaxe, e.HookedByIdentity));
                ClientMessagerForDestroyedObjects.Singleton.ServerSpawnParticleOnClient(transform.position, 0);
            }
        }

        private IEnumerator HookPickaxeAndDestroySelf(GameObject pickaxe, NetworkIdentity identity) {
            yield return new WaitForEndOfFrame();
            NetworkServer.Spawn(pickaxe);
            if (identity.TryGetComponent<IHookSystem>(out IHookSystem hookSystem))
            {
                hookSystem.Hook(pickaxe);
            }

            NetworkServer.Destroy(gameObject);
        }
    }
}

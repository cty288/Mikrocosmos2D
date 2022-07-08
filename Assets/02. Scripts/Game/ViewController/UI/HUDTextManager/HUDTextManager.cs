using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Pool;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class HUDTextManager : AbstractNetworkedController<Mikrocosmos> {
        [SerializeField] private GameObject hurtTextPrefab;
        private GameObjectPool hurtTextPool;
        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnEntityTakeDamage>(OnEntityTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEntityTakeDamage(OnEntityTakeDamage e) {
            if (e.DamageSource.GetComponent<ISpaceshipConfigurationModel>() != null) {
                Vector3 spawnOffset = Vector3.zero;
                if (e.EntityIdentity.TryGetComponent<IDamagableViewController>(out IDamagableViewController damagableViewController)) {
                    spawnOffset = damagableViewController.DamageTextSpawnOffset;
                }
                RpcSpawnText( e.EntityIdentity.transform.position + spawnOffset, e.OldHealth - e.NewHealth);
            }
        }

        public override void OnStartClient() {
            base.OnStartClient();
            hurtTextPool = GameObjectPoolManager.Singleton.CreatePool(hurtTextPrefab, 20, 30);
        }

        [ClientRpc]
        public void RpcSpawnText(Vector2 position, int damage) {
            GameObject hurtTextObj = hurtTextPool.Allocate();
            hurtTextObj.transform.position = position + new Vector2(Random.Range(-2f, 2f), Random.Range(0f, 2f));
            hurtTextObj.transform.localScale = Vector3.one;
            hurtTextObj.GetComponent<DamageTextViewController>().StartAnimate(damage);
        }
    }
}

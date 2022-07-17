using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    //TODO: 以后改成动态加载，提供ISpawnOnWeaponRack接口
    public class WeaponRack : AbstractNetworkedController<Mikrocosmos> {
        [SerializeField] private GameObject goodsToSpawn;
        private Transform spawnPoint;

        private GameObject currentGoods;
        private void Awake() {
            spawnPoint = transform.Find("ItemSpawnPosition");
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnServerObjectHookStateChanged>(OnItemHooked)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            SpawnNewItem();
        }

        private void OnItemHooked(OnServerObjectHookStateChanged e) {
            if (e.Identity.gameObject == currentGoods) {
                if (currentGoods.TryGetComponent<IEntity>(out IEntity entity)) {
                    entity.SetFrozen(false);
                }
                SpawnNewItem();
            }
        }

        private void SpawnNewItem() {
            currentGoods = GameObject.Instantiate(goodsToSpawn, spawnPoint.position, spawnPoint.rotation);
            currentGoods.transform.localScale = Vector3.zero;
            if (currentGoods.TryGetComponent<IEntity>(out IEntity entity)) {
                entity.SetFrozen(true);
            }
            NetworkServer.Spawn(currentGoods);
            currentGoods.transform.DOScale(Vector3.one, 0.5f);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Pool;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class HUDTextManager : AbstractNetworkedController<Mikrocosmos> {
        [SerializeField] private GameObject hurtTextPrefab;
        [SerializeField] private GameObject moneyTextPrefab;
        
        private GameObjectPool hurtTextPool;
        private GameObjectPool moneyTextPool;

        private float moneyChangeWaitTime = 0.05f;
        private float moneyChangeTimer = 0;
        private int totalMoneyChangeOneTime = 0;

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnEntityTakeDamage>(OnEntityTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
            
        }

        private void OnEntityTakeDamage(OnEntityTakeDamage e) {
            if (e.DamageSource &&  e.DamageSource.GetComponent<ISpaceshipConfigurationModel>() != null) {
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
            moneyTextPool = GameObjectPoolManager.Singleton.CreatePool(moneyTextPrefab, 20, 30);
            this.RegisterEvent<OnClientMoneyChange>(OnClientMoneyChange).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        [ClientCallback]
        private void OnClientMoneyChange(OnClientMoneyChange e) {
            moneyChangeTimer = moneyChangeWaitTime;
            totalMoneyChangeOneTime += (e.NewMoney - e.OldMoney);
        }


        private void Update() {
            if (NetworkClient.active) {
                moneyChangeTimer -= Time.deltaTime;
                if (moneyChangeTimer <= 0 && totalMoneyChangeOneTime != 0) {
                    GameObject hurtTextObj = moneyTextPool.Allocate();
                    GameObject spaceship = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship
                        .gameObject;
                    hurtTextObj.transform.SetParent(spaceship.transform);
                    hurtTextObj.transform.position = spaceship.transform.position + (new Vector3(2, 2, 0) + new Vector3(0, Random.Range(0f, 1f), 0));
                    hurtTextObj.transform.localScale = Vector3.one;
                    hurtTextObj.GetComponent<MoneyChangeText>().StartAnimate(totalMoneyChangeOneTime);
                    totalMoneyChangeOneTime = 0;
                }
            }
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

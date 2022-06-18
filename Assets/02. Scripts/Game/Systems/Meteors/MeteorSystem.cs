using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public interface IMeteorSystem : ISystem {
        void ResetMeteor(GameObject meteor);
    }
    public class MeteorSystem : AbstractNetworkedSystem, IMeteorSystem{
        private List<GameObject> activeMeteors = new List<GameObject>();
        [SerializeField]
        private List<GameObject> meteorPrefabs;

        private List<Transform> meteorSpawnPositions;

        [SerializeField] private int meteorMinimumCount = 5;
        [SerializeField] private int meteorSpawnInterval = 10;
        private void Awake() {
            NetworkedObjectPoolManager.AutoCreatePoolWhenAllocating = true;
            meteorSpawnPositions = transform.GetComponentsInChildren<Transform>().ToList();
        }

        private IEnumerator CheckSpawnMeteor() {
            while (true) {
                yield return new WaitForSeconds(meteorSpawnInterval);
                if (activeMeteors.Count < meteorMinimumCount) {
                    SpawnMeteor();
                }
            }
        }
        public override void OnStartServer() {
            base.OnStartServer();
            Mikrocosmos.Interface.RegisterSystem<IMeteorSystem>(this);
            activeMeteors = GameObject.FindGameObjectsWithTag("Meteor").ToList();
            this.RegisterEvent<OnMeteorDestroyed>(OnMeteorDestroyed).UnRegisterWhenGameObjectDestroyed(gameObject);

            StartCoroutine(CheckSpawnMeteor());
        }

        private void OnMeteorDestroyed(OnMeteorDestroyed e) {
            activeMeteors.Remove(e.Meteor);
            /*
            if (activeMeteors.Count < meteorMinimumCount) {
                int spawnCount = meteorMinimumCount - activeMeteors.Count;
                for (int i = 0; i < spawnCount; i++) {
                    SpawnMeteor();
                }
            }*/
        }

        private void SpawnMeteor() {
            Transform spawnPosition = meteorSpawnPositions[Random.Range(0, meteorSpawnPositions.Count)];
            var meteor = Instantiate(meteorPrefabs[Random.Range(0, meteorPrefabs.Count)], spawnPosition.position,
                Quaternion.identity);
            
            meteor.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            meteor.GetComponent<Rigidbody2D>().velocity = spawnPosition.up * Random.Range(25f, 35f);
            meteor.GetComponent<Collider2D>().isTrigger = true;
            
            this.GetSystem<ITimeSystem>().AddDelayTask(5f, () => {
                meteor.GetComponent<Collider2D>().isTrigger = false;
            });
            
            activeMeteors.Add(meteor);
            NetworkServer.Spawn(meteor);
        }


        public void ResetMeteor(GameObject meteor) {
            Transform spawnPosition = meteorSpawnPositions[Random.Range(0, meteorSpawnPositions.Count)];
            meteor.transform.position = spawnPosition.position;
        }
    }

    
}

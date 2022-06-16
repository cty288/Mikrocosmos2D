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
        private void Awake() {
            NetworkedObjectPoolManager.AutoCreatePoolWhenAllocating = true;
            meteorSpawnPositions = transform.GetComponentsInChildren<Transform>().ToList();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            Mikrocosmos.Interface.RegisterSystem<IMeteorSystem>(this);
            activeMeteors = GameObject.FindGameObjectsWithTag("Meteor").ToList();
            this.RegisterEvent<OnMeteorDestroyed>(OnMeteorDestroyed).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnMeteorDestroyed(OnMeteorDestroyed e) {
            activeMeteors.Remove(e.Meteor);
            if (activeMeteors.Count < meteorMinimumCount) {
                int spawnCount = meteorMinimumCount - activeMeteors.Count;
                for (int i = 0; i < spawnCount; i++) {
                    SpawnMeteor();
                }
            }
        }

        private void SpawnMeteor() {
            var meteor = Instantiate(meteorPrefabs[Random.Range(0, meteorPrefabs.Count)]);
            Transform spawnPosition = meteorSpawnPositions[Random.Range(0, meteorSpawnPositions.Count)];
            meteor.transform.position = spawnPosition.position;
            meteor.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            meteor.GetComponent<Rigidbody2D>().velocity = spawnPosition.up * Random.Range(20f, 40f);
            meteor.GetComponent<Collider2D>().isTrigger = true;
            
            this.GetSystem<ITimeSystem>().AddDelayTask(10f, () => {
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

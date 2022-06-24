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

        private List<BoxCollider2D> meteorSpawnPositions;

        [SerializeField] private int meteorMinimumCount = 5;
        [SerializeField] private int meteorSpawnInterval = 10;
        private void Awake() {
            NetworkedObjectPoolManager.AutoCreatePoolWhenAllocating = true;
            meteorSpawnPositions = transform.GetComponentsInChildren<BoxCollider2D>().ToList();
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
            Collider2D spawnPositionArea = meteorSpawnPositions[Random.Range(0, meteorSpawnPositions.Count)];
            Vector2 spawnPosition = new Vector2(
                Random.Range(spawnPositionArea.bounds.min.x, spawnPositionArea.bounds.max.x),
                Random.Range(spawnPositionArea.bounds.min.y, spawnPositionArea.bounds.max.y));


            var meteor = Instantiate(meteorPrefabs[Random.Range(0, meteorPrefabs.Count)], spawnPosition,
                Quaternion.identity);
            
            meteor.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            meteor.GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle * Random.Range(0, 10);
            
            meteor.GetComponent<Collider2D>().isTrigger = false;
          
            activeMeteors.Add(meteor);
            NetworkServer.Spawn(meteor);
        }


        public void ResetMeteor(GameObject meteor) {
            Collider2D spawnPositionArea = meteorSpawnPositions[Random.Range(0, meteorSpawnPositions.Count)];
            Vector2 spawnPosition = new Vector2(
                Random.Range(spawnPositionArea.bounds.min.x, spawnPositionArea.bounds.max.x),
                Random.Range(spawnPositionArea.bounds.min.y, spawnPositionArea.bounds.max.y));
            meteor.transform.position = spawnPosition;
        }
    }

    
}

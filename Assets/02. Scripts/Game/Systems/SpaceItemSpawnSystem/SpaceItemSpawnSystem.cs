using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos {
    public interface ISpaceItemSpawnSystem : ISystem {

    }

    public class SpaceItemSpawnSystem : AbstractNetworkedSystem, ISpaceItemSpawnSystem {
        private IGlobalTradingSystem globalTradingSystem;
        [SerializeField] private Vector2 startingObjSpawnRangeFromStar = new Vector2(50, 95);
        [SerializeField] private int startingObjSpawnCountPerPlayer = 2;

        private int startingObjSpawnNum = 0;

        [SerializeField] private List<GameObject> valveObjects = new List<GameObject>();
        public override void OnStartServer() {
            base.OnStartServer();
            globalTradingSystem = this.GetSystem<IGlobalTradingSystem>();
            startingObjSpawnNum = NetworkManager.singleton.numPlayers * startingObjSpawnCountPerPlayer;
            StartCoroutine(SpawnSpaceItems());
            SpawnValves();
        }

        private void SpawnValves() {
            GameObject[] valveTeam1SpawnPoints = GameObject.FindGameObjectsWithTag("Team1ValveSpawn");
            GameObject[] valveTeam2SpawnPoints = GameObject.FindGameObjectsWithTag("Team2ValveSpawn");
            int team1PlayerNum = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(1).Count;
            int team2PlayerNum = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(2).Count;
            if (!valveTeam1SpawnPoints.Any() || !valveTeam2SpawnPoints.Any() || valveObjects.Count == 0) {
                return;
            }
#if UNITY_EDITOR
            team2PlayerNum++;
            team1PlayerNum++;
#endif
            for (int i = 0; i < team1PlayerNum-1; i++) {
                GameObject valve = GameObject.Instantiate(valveObjects[0], valveTeam1SpawnPoints[i].transform.position,
                    Quaternion.identity);
                NetworkServer.Spawn(valve);
            }

            for (int i = 0; i < team2PlayerNum - 1; i++) {
                GameObject valve = GameObject.Instantiate(valveObjects[1], valveTeam2SpawnPoints[i].transform.position,
                    Quaternion.identity);
                NetworkServer.Spawn(valve);
            }
        }

        private IEnumerator SpawnSpaceItems() {
            yield return new WaitForEndOfFrame();
            List<GameObject> allGoodsWithLowPrice = globalTradingSystem.AllGoodsPrefabsInThisGame.Where(obj => {
                return obj.GetComponent<IGoods>().BasicSellPrice <= 20;
            }).ToList();
            GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
            if (stars.Length > 0) {
                foreach (GameObject star in stars) {
                    for (int i = 0; i < startingObjSpawnNum; i++) {
                        GameObject selectedGood = allGoodsWithLowPrice[Random.Range(0, allGoodsWithLowPrice.Count)];
                        Vector2 starPos = star.transform.position;

                        Vector2 spawnPos = starPos + Random.insideUnitCircle * startingObjSpawnRangeFromStar.y;
                        while (Vector2.Distance(spawnPos, starPos) < startingObjSpawnRangeFromStar.x) {
                            spawnPos = starPos + Random.insideUnitCircle * startingObjSpawnRangeFromStar.y;
                        }

                        GameObject spawnedGood = Instantiate(selectedGood, spawnPos,
                            Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))));
                        NetworkServer.Spawn(spawnedGood);
                    }
                }
            }
        }
    }
}

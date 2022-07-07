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
        public override void OnStartServer() {
            base.OnStartServer();
            globalTradingSystem = this.GetSystem<IGlobalTradingSystem>();
            startingObjSpawnNum = NetworkManager.singleton.numPlayers * startingObjSpawnCountPerPlayer;
            StartCoroutine(SpawnSpaceItems());
            
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

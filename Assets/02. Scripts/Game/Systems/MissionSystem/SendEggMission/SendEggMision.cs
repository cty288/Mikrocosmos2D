using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{


    
    public class SendEggMision : AbstractGameMission, IMission {
       

        [SerializeField] private GameObject sunFlowerPrefab;

        [SerializeField] private GameObject childPrefab;

        private int winningTeam = -1;
        public override string MissionNameLocalizedKey() {
            return "GAME_MISSION_ESCORT_EGG";
        }

        public override string MissionDescriptionLocalizedKey() {
            return "GAME_MISSION_ESCORT_EGG_DESCRIPTION";
        }

        [field: SerializeField]
        public override float MaximumTime { get;  set; } = 120;

        [SerializeField] private GameObject mapPointer;

        private GameObject sunFlower;
        public override void OnStartMission(float overallProgress, int numPlayers) {
            this.RegisterEvent<OnServerTransactionFinished>(OnServerTransactionFinished)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            Transform sunFlowerSpawnPosition = GameObject.FindGameObjectWithTag("Star").transform;
            sunFlower = Instantiate(sunFlowerPrefab, sunFlowerSpawnPosition.position, Quaternion.identity);
            NetworkServer.Spawn(sunFlower);
            Vector4 borders = this.GetSystem<IGameProgressSystem>().GetGameMapSize();

            float x, y;
            do {
                x = Random.Range(borders.x + 50, borders.y - 50);
                y = Random.Range(borders.z + 50, borders.w - 50);
            } while (Physics2D.OverlapCircle(new Vector2(x, y), 1) || Mathf.Abs(x) <= 60 || Mathf.Abs(y) <= 60);
            
            GameObject child = Instantiate(childPrefab, new Vector3(x, y, 0), Quaternion.identity);
            child.GetComponent<IEntity>().SelfMass = 80 * Mathf.SmoothStep(1, 3, overallProgress);
            NetworkServer.Spawn(child);
            RpcSpawnPointerForEgg(child.GetComponent<NetworkIdentity>());
        }

        private void OnServerTransactionFinished(OnServerTransactionFinished e) {
            if (e.Planet == sunFlower) {
                if (!e.IsSell) {
                    winningTeam = e.TeamNumber;
                    StopMission(false);
                }
            }
        }

        [ServerCallback]
        protected override void OnMissionStop(bool runOutOfTime) {
            if (winningTeam != -1 && !runOutOfTime) {
                List<PlayerMatchInfo> matchInfo =
                    this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(winningTeam);

                List<NetworkMainGamePlayer> winners = matchInfo.Select((info => {
                    return info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>();
                })).ToList();
                AnnounceWinners(winners);
            }
           
        }

        [ClientRpc]
        protected void RpcSpawnPointerForEgg(NetworkIdentity egg) {
            PointerManager.Singleton.OnClientAddOrUpdatePointer(new OnClientAddOrUpdatePointer() {
                IsActive = true,
                PointerFollowing = egg.gameObject,
                PointerName = MissionName,
                PointerPrefab = mapPointer
            });
        }

        public override void OnStopClient() {
            base.OnStopClient();
            PointerManager.Singleton.OnClientRemovePointer(new OnClientRemovePointer() {
                PointerName = MissionName
            });
        }
    }
}

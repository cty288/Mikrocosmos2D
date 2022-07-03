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
    public class MineSpaceshipMission : AbstractGameMission {
      

        [SerializeField]
        private GameObject spaceMinecartPrefab;

        private GameObject spaceMinecraftInstance;
        public override string MissionNameLocalizedKey() {
            return "GAME_MISSION_SPACE_MINECART";
        }

        public override string MissionDescriptionLocalizedKey() {
            return "GAME_MISSION_SPACE_MINECART_DESCRIPTION";
        }

        [field: SerializeField ]
        public override float MaximumTime { get; set; } = 120f;
        public override void OnStartMission(float overallProgress) {
            GameObject[] spawnPos = GameObject.FindGameObjectsWithTag("MinecartSpawnPos");

            spaceMinecraftInstance = Instantiate(spaceMinecartPrefab,
                spawnPos[Random.Range(0, spawnPos.Length)].transform.position, Quaternion.identity);
            
            spaceMinecraftInstance.GetComponent<SpaceMinecartModel>().MaxSpeed =
                spaceMinecraftInstance.GetComponent<SpaceMinecartModel>().MaxSpeed /
                Mathf.SmoothStep(1, 2, overallProgress);

            this.RegisterEvent<OnMinespaceshipReachDestination>(OnReachDestination)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            NetworkServer.Spawn(spaceMinecraftInstance);
            // GetComponent<Seeker>()
            
        }

        private void OnReachDestination(OnMinespaceshipReachDestination e) {
            if (e.WinningTeam != -1)
            {
                List<PlayerMatchInfo> matchInfo =
                    this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(e.WinningTeam);
                
                List<NetworkMainGamePlayer> winners = matchInfo.Select((info => {
                    return info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>();
                })).ToList();
                AnnounceWinners(winners);

                //AssignPermanentBuffToPlayers(buffSystem);
            }

            StopMission(false);
        }

        protected override void OnMissionStop(bool runOutOfTime) {
            if (runOutOfTime) {
                ClientMessagerForDestroyedObjects.Singleton.ServerSpawnParticleOnClient(
                    spaceMinecraftInstance.transform.position, 0);
                NetworkServer.Destroy(spaceMinecraftInstance);
              
            }
        }
    }
}

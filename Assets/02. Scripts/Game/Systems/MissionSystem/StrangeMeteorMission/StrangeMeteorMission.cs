using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class StrangeMeteorMission : AbstractGameMission {

      
        [SerializeField] 
        private List<GameObject> strangeMeteorPrefabs;
        public override string MissionNameLocalizedKey() {
            return "GAME_MISSION_STRANGE_METEOR";
        }

        public override string MissionDescriptionLocalizedKey() {
            return "GAME_MISSION_STRANGE_METEOR_DESCRIPTION";
        }

        [field: SerializeField] public override float MaximumTime { get; set; } = 120;

        private List<StrangeMeteorViewController> activeMeteors = new List<StrangeMeteorViewController>();
        
        public override void OnStartMission(float overallProgress) {
            Vector4 borders = this.GetSystem<IGameProgressSystem>().GetGameMapSize();

           
            
            foreach (GameObject meteorPrefab in strangeMeteorPrefabs) {
                float x, y;
                do
                {
                    x = Random.Range(borders.x + 50, borders.y - 50);
                    y = Random.Range(borders.z + 50, borders.w - 50);
                    
                } while (Physics2D.OverlapCircle(new Vector2(x, y), 1) || Mathf.Abs(x) <= 60 || Mathf.Abs(y) <= 60);

                GameObject meteor = Instantiate(meteorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                meteor.GetComponent<StrangeMeteorModel>().PerPlayerProgressPerSecond1 =
                    meteor.GetComponent<StrangeMeteorModel>().PerPlayerProgressPerSecond1 / Mathf.SmoothStep(1, 3, overallProgress);
                NetworkServer.Spawn(meteor);
                activeMeteors.Add(meteor.GetComponent<StrangeMeteorViewController>());
            }
        }

        protected override void OnMissionStop(bool runOutOfTime) {
            float totalFillForTeam1 = 0, totalFillForTeam2 = 0;
            foreach (StrangeMeteorViewController meteor in activeMeteors) {
                totalFillForTeam1 += meteor.ActualFill;
                totalFillForTeam2 += (1 - meteor.ActualFill);
                ClientMessagerForDestroyedObjects.Singleton.ServerSpawnParticleOnClient(meteor.transform.position, 0);
                NetworkServer.Destroy(meteor.gameObject);
            }

            int winningTeam = -1;
            if (totalFillForTeam1 > totalFillForTeam2) {
                winningTeam = 1;
            }

            if (totalFillForTeam1 < totalFillForTeam2) {
                winningTeam = 2;
            }

            if (winningTeam > 0) {
                List<PlayerMatchInfo> matchInfo =
                    this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(winningTeam);

                List<NetworkMainGamePlayer> winners = matchInfo.Select((info => {
                    return info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>();
                })).ToList();
                AnnounceWinners(winners);
            }
        }
    }
}

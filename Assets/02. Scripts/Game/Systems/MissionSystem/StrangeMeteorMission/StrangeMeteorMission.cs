using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class StrangeMeteorMission : AbstractGameMission {

        public override string MissionName { get; } = "StrangeMeteorMission";
        [SerializeField] 
        private List<GameObject> strangeMeteorPrefabs;
        public override string MissionNameLocalized() {
            return Localization.Get("GAME_MISSION_STRANGE_METEOR");
        }

        public override string MissionDescriptionLocalized() {
            return Localization.Get("GAME_MISSION_STRANGE_METEOR_DESCRIPTION");
        }

        [field: SerializeField] public override float MaximumTime { get; set; } = 120;

        private List<StrangeMeteorViewController> activeMeteors = new List<StrangeMeteorViewController>();
        public override void OnMissionStart() {
            Vector4 borders = this.GetSystem<IGameProgressSystem>().GetGameMapSize();

           
            
            foreach (GameObject meteorPrefab in strangeMeteorPrefabs) {
                float x, y;
                do
                {
                    x = Random.Range(borders.x, borders.y);
                    y = Random.Range(borders.z, borders.w);
                } while (Physics2D.OverlapCircle(new Vector2(x, y), 1) || Mathf.Abs(x) <= 60 || Mathf.Abs(y) <= 60);

                GameObject meteor = Instantiate(meteorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                NetworkServer.Spawn(meteor);
                activeMeteors.Add(meteor.GetComponent<StrangeMeteorViewController>());
            }
        }

        protected override void OnMissionStop() {
            
        }
    }
}

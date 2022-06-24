using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class MineSpaceshipMission : AbstractGameMission {
        public override string MissionName { get; } = "SpaceMinecartMission";

        [SerializeField]
        private GameObject spaceMinecartPrefab;
        public override string MissionNameLocalized() {
            return Localization.Get("GAME_MISSION_SPACE_MINECART");
        }

        public override string MissionDescriptionLocalized() {
            return Localization.Get("GAME_MISSION_SPACE_MINECART_DESCRIPTION");
        }

        public override float MaximumTime { get; set; } = 120f;
        public override void OnMissionStart(float overallProgress) {
            
        }

        protected override void OnMissionStop() {
            
        }
    }
}

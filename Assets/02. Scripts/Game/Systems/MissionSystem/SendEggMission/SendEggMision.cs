using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{


    
    public class SendEggMision : AbstractGameMission, IMission {
        public override string MissionName { get; } = "SendEggMission";
        public override string MissionNameLocalized() {
            return Localization.Get("GAME_MISSION_ESCORT_EGG");
        }

        public override string MissionDescriptionLocalized() {
            return Localization.Get("GAME_MISSION_ESCORT_EGG_DESCRIPTION");
        }

        [field: SerializeField]
        public override float MaximumTime { get;  set; } = 120;

        public override void OnMissionStart() {
            
        }

        protected override void OnMissionStop() {
            
        }
    }
}

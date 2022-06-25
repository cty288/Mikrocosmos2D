using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class StrangeMeteorModel : AbstractBasicEntityModel {
        [SerializeField]
        private float perPlayerProgressPerSecond = 0.04f;

        public float PerPlayerProgressPerSecond1 {
            get => perPlayerProgressPerSecond;
            set => perPlayerProgressPerSecond = value;
        }

        public float PerPlayerProgressPerSecond => perPlayerProgressPerSecond;

        private float team1Progress = 0.5f;

        public float Team1Progress {
            get => team1Progress;
            set => team1Progress = value;
        }

        
        private int team1MinusTeam2PlayerDifference = 0;

        public int Team1MinusTeam2PlayerDifference {
            get => team1MinusTeam2PlayerDifference;
            set => team1MinusTeam2PlayerDifference = value;
        }

        [field: SerializeField]
        public override float SelfMass { get;  set; }
        [field: SerializeField]        
        public override string Name { get; set; }
        public override void OnClientHooked() {
            
        }

        public override void OnClientFreed() {
            
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class SpaceMinecartModel : NetworkedModel, IHaveMomentum {
        [field: SerializeField]
        public MoveMode MoveMode { get; set; }

        public float MaxSpeed { get; set; } = 13;
        public float Acceleration { get; } 
        [field: SerializeField]
        public float SelfMass { get; set; }
        public float GetTotalMass() {
            return SelfMass;
        }
    }
}

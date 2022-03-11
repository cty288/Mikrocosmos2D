using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class SpaceshipModel : AbstractBasicEntity, ISpaceshipConfigurationModel {
        public override string Name { get; } = "Spaceship";
        public override void OnClientSelfMassChanged(float oldMass, float newMass) {
            
        }

        [field: SyncVar, SerializeField]
        public float MoveForce { get; set; } //18 30

      
    }
}

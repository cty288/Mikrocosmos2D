using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnMassChanged
    {
        public float newMass;
    }
    public class SpaceshipModel : AbstractBasicEntityModel, ISpaceshipConfigurationModel {
        public override string Name { get; } = "Spaceship";
        public override void OnClientSelfMassChanged(float oldMass, float newMass) {
            
        }

        [field: SyncVar(hook = nameof(Hook)), SerializeField]
        public float MoveForce { get; set; } //18 30


        public void Hook(float oldValue, float newValue) {
            if (hasAuthority)
            {
                this.SendEvent<OnMassChanged>(new OnMassChanged(){newMass = newValue});
            }
        }

    }
}

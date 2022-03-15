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
       

        #region Client
        public override void OnHooked()
        {

        }

        public override void OnFreed()
        {

        }

        public override void OnClientSelfMassChanged(float oldMass, float newMass)
        {

        }
        public void OnMassChanged(float oldValue, float newValue)
        {
            if (hasAuthority)
            {
                this.SendEvent<OnMassChanged>(new OnMassChanged() { newMass = newValue });
                
            }
        }
        #endregion




        #region Server
        [field: SyncVar, SerializeField]
        public float MoveForce { get; set; } //18 30

       
        #endregion



      

        
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ISpaceshipConfigurationModel : IModel {
        public float MoveForce { get; set; }
        public float MaxSpeed { get; set; }
    }
    public class SpaceshipConfigurationModel : AbstractModel, ISpaceshipConfigurationModel
    {
        protected override void OnInit() {
            
        }

        public float MoveForce { get; set; } = 8f;
        public float MaxSpeed { get; set; } = 15f;
    }
}

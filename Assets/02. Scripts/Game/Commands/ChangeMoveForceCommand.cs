using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public class ChangeMoveForceCommand : AbstractCommand<ChangeMoveForceCommand>
    {
        private ISpaceshipConfigurationModel model;
        private float force;

        public ChangeMoveForceCommand(){}
        public ChangeMoveForceCommand(ISpaceshipConfigurationModel spaceshipModel, float moveForce)
        {
            this.model = spaceshipModel;
            this.force = moveForce;
        }
        protected override void OnExecute()
        {
            model.MoveForce = force;
        }
    }
}

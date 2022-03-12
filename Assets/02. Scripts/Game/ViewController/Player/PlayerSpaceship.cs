using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class PlayerSpaceship : BasicEntityViewController<SpaceshipModel> {
        [Command]
        private void CmdChangeMoveForce(float force) {
            ChangeMoveForceCommand cmd = new ChangeMoveForceCommand(model, force);
            this.SendCommand(cmd);
        }
    }
}

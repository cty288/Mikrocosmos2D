using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
   
    
    public partial class PlayerSpaceship : BasicEntityViewController {
        private IHookSystem hookSystem;
        
        
        

        [Command]
        private void CmdChangeMoveForce(float force) {
            ChangeMoveForceCommand cmd = new ChangeMoveForceCommand( GetModel(), force);
            this.SendCommand(cmd);
            
        }

        
    }

    }


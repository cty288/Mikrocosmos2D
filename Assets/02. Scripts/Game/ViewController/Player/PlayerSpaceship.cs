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
            ChangeMoveForceCommand cmd = new ChangeMoveForceCommand(Model, force);
            this.SendCommand(cmd);
        }




        [Command]
        private void CmdTryUseHook() {
            //take item & put down item
            if (Model.HookedItem == null)
            {
                if (Model.HookState == HookState.Freed) {
                    if (hookTrigger.Triggered) {
                        List<Collider2D> colliders = hookTrigger.Colliders;
                        foreach (Collider2D collider in colliders) {
                            if (collider.gameObject
                                .TryGetComponent<IHookableViewController>(out IHookableViewController vc)) {
                                this.SendCommand<HookOrUnhookObjectCommand>(HookOrUnhookObjectCommand.Allocate(vc, true, netIdentity, Model));
                                break;
                            }
                        }

                    }
                }
            }else  { //put down item 
                this.SendCommand<HookOrUnhookObjectCommand>(HookOrUnhookObjectCommand.Allocate(Model.HookedItem, false, null, Model));
            }



        }

    }
}

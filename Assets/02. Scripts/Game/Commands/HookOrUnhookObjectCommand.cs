using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class HookOrUnhookObjectCommand : AbstractCommand<HookOrUnhookObjectCommand> {
        private IHookableViewController hookableViewController;
        private bool isHook;
        private NetworkIdentity hookedBy;
        private ISpaceshipConfigurationModel spaceshipConfigurationModel;
        protected override void OnExecute() {
            if (isHook) {
                spaceshipConfigurationModel.HookedItem = hookableViewController;
                hookableViewController.Model.Hook(hookedBy);
            }
            else {
                hookableViewController.Model.UnHook();
                spaceshipConfigurationModel.HookedItem = null;
            }
        }

        public HookOrUnhookObjectCommand() {
        }

        public static HookOrUnhookObjectCommand Allocate(IHookableViewController vc, bool isHook, NetworkIdentity hookedBy = null, ISpaceshipConfigurationModel spaceshipConfigurationModel = null) {
            HookOrUnhookObjectCommand cmd  = SafeObjectPool<HookOrUnhookObjectCommand>.Singleton.Allocate();
            cmd.hookableViewController = vc;
            cmd.isHook = isHook;
            cmd.hookedBy = hookedBy;
            cmd.spaceshipConfigurationModel = spaceshipConfigurationModel;
            return cmd;
        }

    }
}

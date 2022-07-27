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
        private IHookSystem hookSystem;
        protected override void OnExecute() {
            if (isHook) {
                hookSystem.HookedItem = hookableViewController;
                hookableViewController.Model.TryHook(hookedBy);
            }
            else {
                hookableViewController.Model.UnHookByHook(false, false);
                hookSystem.HookedItem = null;
            }
        }

        public HookOrUnhookObjectCommand() {
        }

        public static HookOrUnhookObjectCommand Allocate(IHookableViewController vc, bool isHook,
            IHookSystem hookSystem,NetworkIdentity hookedBy = null, ISpaceshipConfigurationModel spaceshipConfigurationModel = null) {
            HookOrUnhookObjectCommand cmd  = SafeObjectPool<HookOrUnhookObjectCommand>.Singleton.Allocate();
            cmd.hookableViewController = vc;
            cmd.isHook = isHook;
            cmd.hookSystem = hookSystem;
            cmd.hookedBy = hookedBy;
            cmd.spaceshipConfigurationModel = spaceshipConfigurationModel;
            return cmd;
        }

    }
}

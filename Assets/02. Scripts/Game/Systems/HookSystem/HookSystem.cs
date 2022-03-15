using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos {
    public enum HookAction {
        Hook,
        UnHook,
        Shoot
    }

    public interface IHookSystem : ISystem {
        IHookableViewController HookedItem { get; set; }
       [Command]
        void CmdHoldHookButton();

        [Command]
        void CmdReleaseHookButton();

        bool IsHooking { get; }
    }
    public partial class HookSystem : AbstractNetworkedSystem, IHookSystem
    {
        [field: SerializeField]
        public IHookableViewController HookedItem { get; set; }

        [SerializeField] private float shootTimeThreshold = 0.5f;
        /// <summary>
        /// OneCycle time; including charge / decharge
        /// </summary>
        [SerializeField] private float shootChargeOneCycleTime = 4f;
        
        [field: SyncVar]
        public bool IsHooking { get; private set; }

        private float hookHoldTimer = 0;

        protected ISpaceshipConfigurationModel model;

        private Trigger2DCheck hookTrigger;

        private bool holdingButton = false;

        /// <summary>
        /// 0-0.5: charge up; 0.5-0: charge down
        /// </summary>
        [SyncVar(hook = nameof(OnHookChargePercentChanged))] [SerializeField] 
        private float hookShootChargePercent;

        private void Awake() {
            model = GetBindedModel<ISpaceshipConfigurationModel>();
            hookTrigger = GetComponentInChildren<Trigger2DCheck>();
        }

        

        [Command]
        public void CmdHoldHookButton() {
            holdingButton = true;
           
        }

        private void Update() {
            if (isServer) {
                IsHooking = HookedItem != null;
            }

            if (holdingButton) {
                if (HookedItem != null) {
                    hookHoldTimer += Time.deltaTime;
                    if (hookHoldTimer >= shootTimeThreshold) {
                        float thisCycleTime = (hookHoldTimer - shootTimeThreshold) % shootChargeOneCycleTime;
                        hookShootChargePercent = thisCycleTime / shootChargeOneCycleTime;
                    }
                }
            }
        }

        [Command]
        public void CmdReleaseHookButton() {
            holdingButton = false;
            HookAction targetAction = CheckHookAction();
            switch (targetAction) {
                case HookAction.Hook:
                    TryHook();
                    break;
                case HookAction.UnHook:
                    TryUnHook();
                    break;
                case HookAction.Shoot:
                    break;
            }

            hookShootChargePercent = 0;
            hookHoldTimer = 0;
            //start check hook type
        }

        private void TryUnHook() {
            if (HookedItem != null) {
                HookedItem.Model.UnHook();
                HookedItem = null;
            }
        }

        private void TryHook() {
            if (model.HookState == HookState.Freed)
            {
                if (hookTrigger.Triggered)
                {
                    List<Collider2D> colliders = hookTrigger.Colliders;
                    foreach (Collider2D collider in colliders)
                    {
                        if (collider.gameObject
                            .TryGetComponent<IHookableViewController>(out IHookableViewController vc))
                        {
                            HookedItem = vc;
                            vc.Model.Hook(netIdentity);
                            break;
                        }
                    }

                }
            }
        }

        private HookAction CheckHookAction() {
            if (hookHoldTimer <= shootTimeThreshold) {
                if (HookedItem == null) {
                    return HookAction.Hook;
                }
                else {
                    return HookAction.UnHook;
                }
            }
            else {
                return HookAction.Shoot;
            }
        }


       
    }
}

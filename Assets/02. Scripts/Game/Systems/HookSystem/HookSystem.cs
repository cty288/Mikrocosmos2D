using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Mikrocosmos {
    public enum HookAction {
        Hook,
        UnHook,
        Shoot
    }

    public struct OnItemShot {
        public ICanBeShotViewController TargetShotItem;
        public Vector2 Force;
    }

    public interface IHookSystem : ISystem {
        IHookableViewController HookedItem { get; set; }

        NetworkIdentity HookedNetworkIdentity { get; set; }
       [Command]
        void CmdHoldHookButton();

        [Command]
        void CmdReleaseHookButton();

        bool IsHooking { get; }
    }
    public partial class HookSystem : AbstractNetworkedSystem, IHookSystem
    {
       
        public IHookableViewController HookedItem { get; set; }

        [field:SyncVar, SerializeField]
        public NetworkIdentity HookedNetworkIdentity { get; set; }

        [SerializeField] private float shootTimeThreshold = 0.5f;
        /// <summary>
        /// OneCycle time; including charge / decharge
        /// </summary>
        [SerializeField] private float shootChargeOneCycleTime = 4f;

        [SerializeField] private float maxShootForce = 20f;
        
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
                if (HookedItem != null && (HookedItem is ICanBeShotViewController)) {

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
                    TryShoot();
                    break;
            }

            hookShootChargePercent = 0;
            hookHoldTimer = 0;
            //start check hook type
        }

        private void TryShoot() {
            if (HookedItem != null) {
                HookedItem.Model.UnHook();

               

                float realPercent = (hookShootChargePercent * 2);
                if (realPercent >= 1) {
                    realPercent = -realPercent + 2;
                }

                Vector2 force = transform.up * maxShootForce * realPercent;
                Debug.Log($"Force: {force}, {transform.up}, {maxShootForce}, {realPercent}");
                this.SendEvent<OnItemShot>(new OnItemShot() {
                    Force = force,
                    TargetShotItem = HookedItem as ICanBeShotViewController
                });

                HookedItem = null;
                HookedNetworkIdentity = null;
            }
        }

        private void TryUnHook() {
            if (HookedItem != null && (HookedItem is ICanBeShotViewController)) {
                HookedItem.Model.UnHook();
                HookedItem = null;
                HookedNetworkIdentity = null;
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
                            HookedNetworkIdentity = collider.gameObject.GetComponent<NetworkIdentity>();
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

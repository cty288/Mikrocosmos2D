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

        private Animator animator;

        /// <summary>
        /// 0-0.5: charge up; 0.5-0: charge down
        /// </summary>
        [SyncVar(hook = nameof(OnHookChargePercentChanged))] [SerializeField] 
        private float hookShootChargePercent;

        private void Awake() {
            model = GetBindedModel<ISpaceshipConfigurationModel>();
            hookTrigger = GetComponentInChildren<Trigger2DCheck>();
            animator = GetComponent<Animator>();
        }

        

        [Command]
        public void CmdHoldHookButton() {
            holdingButton = true;
        }

        private void Update() {
            if (isServer) {
                IsHooking = HookedItem != null;

                if (holdingButton) {
                    if (HookedItem != null && (HookedItem is ICanBeShotViewController)) {
                        hookHoldTimer += Time.deltaTime;
                        if (hookHoldTimer >= shootTimeThreshold)
                        {
                            float thisCycleTime = (hookHoldTimer - shootTimeThreshold) % shootChargeOneCycleTime;
                            hookShootChargePercent = thisCycleTime / shootChargeOneCycleTime;
                        }
                    }
                }

                if (checkingHook) {
                    CheckHook();
                }

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot")) {
                    animator.SetBool("Hooking", false);
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

        
        public void ServerShootTrigger()
        {
            if (isServer) {
                Vector2 force = transform.up * maxShootForce * realShootPercent;
                Debug.Log($"Force: {force}, {transform.up}, {maxShootForce}, {realShootPercent}");

                this.SendEvent<OnItemShot>(new OnItemShot()
                {
                    Force = force,
                    TargetShotItem = HookedItem as ICanBeShotViewController
                });

                HookedItem.Model.UnHook(true);

              
                HookedItem = null;

                HookedNetworkIdentity = null;
            }
            
        }

        private float realShootPercent;
        private void TryShoot() {
            if (HookedItem != null && HookedItem is ICanBeShotViewController) {
                float realPercent = (hookShootChargePercent * 2);
                if (realPercent >= 1) {
                    realPercent = -realPercent + 2;
                }

                realShootPercent = realPercent;
                GetComponent<NetworkAnimator>().SetTrigger("Shoot");
            }
        }

        private void TryUnHook() {
            if (HookedItem != null) {

                HookedItem.Model.UnHook(false);
            }
           
            HookedItem = null;
            HookedNetworkIdentity = null;
            animator.SetBool("Hooking", false);
        }

        private bool checkingHook = false;

        [ServerCallback]
        public void ServerStartHookTrigger() {
            checkingHook = true;
        }

        [ServerCallback]
        public void ServerStopHookTrigger() {
            checkingHook = false;
        }


        [ServerCallback]
        private void CheckHook() {
            if (hookTrigger.Triggered && model.HookState == HookState.Freed) {
                List<Collider2D> colliders = hookTrigger.Colliders;
                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject
                        .TryGetComponent<IHookableViewController>(out IHookableViewController vc))
                    {
                        vc.Model.UnHook(true);
                        if (vc.Model.Hook(netIdentity)) {
                            HookedItem = vc;
                            HookedNetworkIdentity = collider.gameObject.GetComponent<NetworkIdentity>();
                            
                            animator.SetBool("Hooking", true);
                            checkingHook = false;
                            break;
                        }
                    }
                }

            }
        }


        [ServerCallback]
        private void TryHook() {
            if (model.HookState == HookState.Freed && animator.GetCurrentAnimatorStateInfo(0).IsName("UnHooking")) {
                GetComponent<NetworkAnimator>().SetTrigger("StartHook");
            }
        }

        private HookAction CheckHookAction() {

            if (HookedItem == null) {
                return HookAction.Hook;
            }
            if (hookHoldTimer <= shootTimeThreshold) {
                if (HookedItem != null) {
                    return HookAction.UnHook;
                }
                return HookAction.Hook;
            }
            else {
                return HookAction.Shoot;
            }
        }


       
    }
}

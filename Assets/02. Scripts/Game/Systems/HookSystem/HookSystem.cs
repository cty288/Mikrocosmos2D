using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
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

    public struct OnHookItemSwitched {
        public NetworkIdentity OldIdentity;
        public NetworkIdentity NewIdentity;
        public NetworkIdentity OwnerIdentity;
    }
    public struct OnItemShot {
        public ICanBeShotViewController TargetShotItem;
        public Vector2 Force;
    }

    public struct OnItemRobbed {
        public NetworkIdentity Victim;
        public IHookable HookedItem;
    }

    public interface IHookSystem : ISystem {
        IHookableViewController HookedItem { get; set; }

        NetworkIdentity HookedNetworkIdentity { get; set; }
       [Command]
        void CmdHoldHookButton();

        [Command]
        void CmdReleaseHookButton();

        void UpdateHookCollisions(bool collisionOn);

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

        private static ResLoader resLoader;

        private IPlayerInventorySystem inventorySystem;

        /// <summary>
        /// 0-0.5: charge up; 0.5-0: charge down
        /// </summary>
        [SyncVar(hook = nameof(OnHookChargePercentChanged))] [SerializeField] 
        private float hookShootChargePercent;

        private void Awake() {
            model = GetBindedModel<ISpaceshipConfigurationModel>();
            hookTrigger = GetComponentInChildren<Trigger2DCheck>();
            animator = GetComponent<Animator>();
            ResLoader.Create(loader => {
                if (resLoader == null) {
                    resLoader = loader;
                }
                
            });
            inventorySystem = GetComponent<IPlayerInventorySystem>();
            this.RegisterEvent<OnItemRobbed>(OnRobbed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnSwitchItemSlot>(OnServerSwitchItemSlot).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnHookItemSwitched>(OnHookItemSwitched).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnBackpackItemRemoved>(OnCurrentBackPackItemRemoved)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        [ServerCallback]
        private void OnCurrentBackPackItemRemoved(OnBackpackItemRemoved e) {
            if (e.Identity == netIdentity)
            {

                NetworkIdentity oldIdentity = HookedNetworkIdentity;


                if (e.CurrentCount>0)
                {
                    Debug.Log(e.PrefabName);
                    GameObject nextItemPrefab = resLoader.LoadSync<GameObject>("assets/goods", e.PrefabName);

                    Debug.Log(nextItemPrefab);
                    GameObject spawned = Instantiate(nextItemPrefab);
                    NetworkServer.Spawn(spawned);
                    spawned.transform.position = GetComponentInChildren<Trigger2DCheck>().transform.position;

                    HookedItem = spawned.GetComponent<IHookableViewController>();
                    HookedNetworkIdentity = spawned.GetComponent<NetworkIdentity>();
                    HookedItem.Model.Hook(netIdentity);
                    animator.SetBool("Hooking", true);

                }
                else
                {
                    animator.SetBool("Hooking", false);
                   HookedItem = null;
                    HookedNetworkIdentity = null;
                }


                this.SendEvent<OnHookItemSwitched>(new OnHookItemSwitched()
                {
                    NewIdentity = HookedNetworkIdentity,
                    OldIdentity = oldIdentity,
                    OwnerIdentity = netIdentity
                });

              
                UpdateHookCollisions(false);
            }
        }

        [ServerCallback]
        private void OnHookItemSwitched(OnHookItemSwitched e) {
            
            if (allHookingIdentities != null && (allHookingIdentities.Contains(e.OwnerIdentity) || (e.OldIdentity  &&  allHookingIdentities.Contains(e.OldIdentity)))) {
                UpdateHookCollisions(false);
            }
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
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hook")) {
                switch (targetAction)
                {
                    case HookAction.Hook:
                        TryHook();
                        break;
                    case HookAction.UnHook:
                        UnHook();
                        break;
                    case HookAction.Shoot:
                        TryShoot();
                        break;
                }

                hookShootChargePercent = 0;
                hookHoldTimer = 0;
            }
            
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

        private void OnDestroy() {
            if (resLoader != null) {
                resLoader.ReleaseAllAssets();
            }
          
            resLoader = null;
        }

        [ServerCallback]
        private void OnServerSwitchItemSlot(OnSwitchItemSlot e) {
            if (e.Identity == netIdentity) {

                NetworkIdentity oldIdentity = HookedNetworkIdentity;
                

                //still have item
                if (!String.IsNullOrEmpty(e.PrefabName)) {
                    GameObject nextItemPrefab = resLoader.LoadSync<GameObject>("assets/goods",e.PrefabName);
                    GameObject spawned = Instantiate(nextItemPrefab);
                    NetworkServer.Spawn(spawned);
                    spawned.transform.position = GetComponentInChildren<Trigger2DCheck>().transform.position;

                    HookedItem = spawned.GetComponent<IHookableViewController>();
                    HookedNetworkIdentity = spawned.GetComponent<NetworkIdentity>();
                    HookedItem.Model.Hook(netIdentity);
                    animator.SetBool("Hooking", true);

                }
                else {
                    animator.SetBool("Hooking", false);
                    HookedItem = null;
                    HookedNetworkIdentity = null;
                }

                
                this.SendEvent<OnHookItemSwitched>(new OnHookItemSwitched() {
                    NewIdentity = HookedNetworkIdentity,
                    OldIdentity = oldIdentity,
                    OwnerIdentity = netIdentity
                });

                if (oldIdentity) {
                    NetworkServer.Destroy(oldIdentity.gameObject);
                }
                
                UpdateHookCollisions(false);
            }
        }

        private List<NetworkIdentity> allHookingIdentities = new List<NetworkIdentity>();

        [ServerCallback]
        public void UpdateHookCollisions(bool collisionOn)
        {
            allHookingIdentities = GetAllHookingIdentities();
            if (allHookingIdentities != null) {
                foreach (NetworkIdentity hookingIdentity in allHookingIdentities)
                {
                    Physics2D.IgnoreCollision(hookingIdentity.GetComponent<Collider2D>(), GetComponent<Collider2D>(),
                        !collisionOn);
                }
            }
           
        }
        [ServerCallback]
        private void OnRobbed(OnItemRobbed e) {
            if (e.Victim == netIdentity && e.HookedItem == HookedItem.Model) {
                UnHook();
            }
        }

        [ServerCallback]
        private void UnHook() {
            UpdateHookCollisions(true);

            if (HookedItem != null) {
                HookedItem.Model.UnHook(false);
                if (HookedItem.Model.CanBeAddedToInventory)
                {
                    inventorySystem.ServerRemoveFromCurrentBackpack();
                }
                else {
                    HookedItem = null;
                    HookedNetworkIdentity = null;
                    animator.SetBool("Hooking", false);
                }
            }
            

            
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
                        .TryGetComponent<IHookableViewController>(out IHookableViewController vc)) {

                        IHookable model = vc.Model;
                        if (model.HookedByIdentity && model.HookedByIdentity!=netIdentity) {
                            this.SendEvent<OnItemRobbed>(new OnItemRobbed() {
                                HookedItem = vc.Model,
                                Victim = model.HookedByIdentity
                            });
                            //vc.Model.UnHook(true);
                        }
                        

                        if (collider.TryGetComponent<IHookSystem>(out IHookSystem owner)) {
                            owner.UpdateHookCollisions(false);
                        }


                        if (model.Hook(netIdentity)) {
                            
                            HookedItem = vc;
                            HookedNetworkIdentity = collider.gameObject.GetComponent<NetworkIdentity>();
                            animator.SetBool("Hooking", true);
                            checkingHook = false;
                            if (HookedItem.Model.CanBeAddedToInventory)
                            {
                                inventorySystem.ServerAddToBackpack(model.Name, 1);
                            }
                           
                            UpdateHookCollisions(false);

                            
                            break;
                        }
                    }
                }

            }
        }

        

        [ServerCallback]
        private List<NetworkIdentity> GetAllHookingIdentities() {
            if (HookedNetworkIdentity) {
                List<NetworkIdentity> result = new List<NetworkIdentity>();

                NetworkIdentity currentItem = HookedNetworkIdentity;
                if (currentItem != null) {
                    result.Add(currentItem);
                    while (currentItem && currentItem.TryGetComponent<IHookSystem>(out IHookSystem owner))
                    {

                        currentItem = owner.HookedNetworkIdentity;
                        if (currentItem)
                        {
                            result.Add(currentItem);
                        }
                    }
                }
                

                return result;
            }

            return null;
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

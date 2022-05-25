using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;
using Random = UnityEngine.Random;

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

    public struct OnItemUsed {
        public ICanBeUsed Item;
        public NetworkIdentity HookedBy;
    }

    public struct OnItemBroken{
        public ICanBeUsed Item;
        public NetworkIdentity HookedBy;
    }

    public interface IHookSystem : ISystem {
        IHookableViewController HookedItem { get; set; }

        NetworkIdentity HookedNetworkIdentity { get; set; }
       [Command]
        void CmdHoldHookButton();

        [Command]
        void CmdReleaseHookButton();

        void UpdateHookCollisions(bool collisionOn);

        void OnServerPlayerHoldUseButton();

        void OnServerPlayerReleaseUseButton();

        void ServerUseItem(Func<bool> condition);

        void UnHook();

        bool IsHooking { get; }
    }
    public partial class HookSystem : AbstractNetworkedSystem, IHookSystem
    {
       
        public IHookableViewController HookedItem { get; set; }

        [field:SyncVar, SerializeField]
        public NetworkIdentity HookedNetworkIdentity { get; set; }

        [SerializeField] private float shootTimeThreshold = 0.2f;
        /// <summary>
        /// OneCycle time; including charge / decharge
        /// </summary>
        [SerializeField] private float shootChargeOneCycleTime = 4f;

        [SerializeField] private float maxShootForce = 80f;

        [SerializeField] private Transform droppedItemSpawnPos;

        

        [field: SyncVar]
        public bool IsHooking { get; private set; }

        private float hookHoldTimer = 0;

        protected ISpaceshipConfigurationModel model;

        private Trigger2DCheck hookTrigger;

        private bool holdingHookButton = false;

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
            this.RegisterEvent<OnItemBroken>(OnItemBroken).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnItemDropped>(OnItemDropped).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        [ServerCallback]
        private void OnItemDropped(OnItemDropped e) {
            if (e.Identity == netIdentity) {

                NetworkIdentity oldHookedIdentity = HookedNetworkIdentity;
                
                if (e.DroppedObject) {
                    if (e.DroppedCurrentSlot) { //dropped current selected slot
                        HookedItem.Model.UnHook(false);
                        //like removed from backpack
                        Debug.Log("Next hooking obj after drop: " + e.NextHookingObject);
                        if (e.NextHookingObject) {
                            GameObject nextItem = e.NextHookingObject;
                            nextItem.SetActive(true);
                            NetworkServer.Spawn(nextItem);
                            nextItem.transform.position = GetComponentInChildren<Trigger2DCheck>().transform.position;

                            HookedItem = nextItem.GetComponent<IHookableViewController>();
                            HookedNetworkIdentity = nextItem.GetComponent<NetworkIdentity>();
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
                            OldIdentity = oldHookedIdentity,
                            OwnerIdentity = netIdentity
                        });
                    }
                    else { //dropped from backpack: just spawn it somewhere
                        GameObject droppedObj = e.DroppedObject;
                        droppedObj.SetActive(true);
                        Vector2 spawnPos = new Vector2(droppedItemSpawnPos.position.x + Random.Range(-1f, 1f),
                            droppedItemSpawnPos.position.y + Random.Range(-1f, 1f));

                        droppedObj.transform.position = spawnPos;
                        droppedObj.GetComponent<IHookable>().UnHook(false);
                        Vector2 randomForce = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

                        droppedObj.GetComponent<Rigidbody2D>().AddForce(randomForce * Random.Range(0f, 5f),
                            ForceMode2D.Impulse);

                        NetworkServer.Spawn(droppedObj);
                    }
                }
                
                UpdateHookCollisions(false);
            }
        }

        [ServerCallback]
        private void OnItemBroken(OnItemBroken e) {
            if (HookedItem!=null && e.Item == HookedItem.Model) {
                GameObject go = HookedNetworkIdentity.gameObject;
                UnHook();
                NetworkServer.Destroy(go);
            }
        }

        [ServerCallback]
        private void OnCurrentBackPackItemRemoved(OnBackpackItemRemoved e) {
            if (e.Identity == netIdentity)
            {

                NetworkIdentity oldIdentity = HookedNetworkIdentity;


                if (e.CurrentCount>0) {

                    GameObject nextItem = e.NextObject;
                    nextItem.SetActive(true);
                    NetworkServer.Spawn(nextItem);



                    nextItem.transform.position = GetComponentInChildren<Trigger2DCheck>().transform.position;

                    HookedItem = nextItem.GetComponent<IHookableViewController>();
                    HookedNetworkIdentity = nextItem.GetComponent<NetworkIdentity>();
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
            holdingHookButton = true;
        }

        private void Update() {
            if (isServer) {
                IsHooking = HookedItem != null;
                useTimer += Time.deltaTime;
                if (holdingHookButton) {
                    if (HookedItem != null && (HookedItem is ICanBeShotViewController)) {
                        hookHoldTimer += Time.deltaTime;
                        if (hookHoldTimer >= shootTimeThreshold)
                        {
                            float thisCycleTime = (hookHoldTimer - shootTimeThreshold) % shootChargeOneCycleTime;
                            hookShootChargePercent = thisCycleTime / shootChargeOneCycleTime;
                        }
                    }
                    else {
                        hookHoldTimer = 0;
                        hookShootChargePercent = 0;
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
            holdingHookButton = false;
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

            this.GetSystem<ITimeSystem>().AddDelayTask(0.1f, () => {
                GetComponent<NetworkAnimator>().ResetTrigger("StartHook");
            });
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
          
            //resLoader = null;
        }

        [ServerCallback]
        private void OnServerSwitchItemSlot(OnSwitchItemSlot e) {
            if (e.Identity == netIdentity) {

                NetworkIdentity oldIdentity = HookedNetworkIdentity;
                if (e.SameSlot) {
                    this.SendEvent<OnHookItemSwitched>(new OnHookItemSwitched() {
                        NewIdentity = e.SwitchedGameObject.GetComponent<NetworkIdentity>(),
                        OldIdentity = null,
                        OwnerIdentity = netIdentity
                    });
                    return;
                }

                //still have item
                if (!String.IsNullOrEmpty(e.PrefabName)) {
                    GameObject nextItem = e.SwitchedGameObject;
                    if (HookedNetworkIdentity==null || e.SwitchedGameObject != HookedNetworkIdentity.gameObject) {
                        nextItem.SetActive(true);
                        NetworkServer.Spawn(nextItem);
                    }
                    
                    nextItem.transform.position = GetComponentInChildren<Trigger2DCheck>().transform.position;

                    HookedItem = nextItem.GetComponent<IHookableViewController>();
                    HookedNetworkIdentity = nextItem.GetComponent<NetworkIdentity>();
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

                if (oldIdentity && oldIdentity.gameObject !=e.SwitchedGameObject) {
                    NetworkServer.UnSpawn(oldIdentity.gameObject);
                    oldIdentity.gameObject.SetActive(false);
                    //NetworkServer.Destroy(oldIdentity.gameObject);
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

        private float useTimer = 0;
        private bool itemUsedForThisPress = false;

        [ServerCallback]
        public void OnServerPlayerHoldUseButton() {

            ServerUseItem((() => {
                if (HookedItem != null) {
                    if (HookedItem.Model is ICanBeUsed model) {
                        if (model.CanBeUsed && model.Durability != 0) {
                            if (model.UseMode == ItemUseMode.UseWhenKeyDown && itemUsedForThisPress) {
                                return false;
                            }
                            //now check time
                            if (useTimer >= model.Frequency) {
                                itemUsedForThisPress = true;
                                useTimer = 0;
                                return true;
                            }
                        }
                    }
                }

                return false;
            }));
        }

        [ServerCallback]
        public void OnServerPlayerReleaseUseButton() {
            itemUsedForThisPress = false;
        }

        [ServerCallback]
        public void ServerUseItem(Func<bool> condition) {
            if (condition()) {
                if (HookedItem.Model is ICanBeUsed model) {
                    NetworkIdentity hookedBy = netIdentity;
                    model.OnItemUsed();
                    this.SendEvent<OnItemUsed>(new OnItemUsed()
                    {
                        Item = model,
                        HookedBy = hookedBy
                    });
                    /*
                    //check durability, if =0, then destroy item
                    if (model.Durability == 0) {
                        GameObject go = HookedNetworkIdentity.gameObject;
                        UnHook();
                        NetworkServer.Destroy(go);
                    }*/
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
        public void UnHook() {
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
                            if (HookedItem.Model.CanBeAddedToInventory) {
                                inventorySystem.ServerAddToBackpack(model.Name, HookedNetworkIdentity.gameObject);
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

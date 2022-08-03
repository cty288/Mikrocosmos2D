using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Mikrocosmos
{
    public struct OnServerObjectHookStateChanged {
        public NetworkIdentity Identity;
        public HookState HookState;
        public NetworkIdentity HookedByIdentity;
    }
    public abstract class AbstractBasicEntityModel : NetworkedModel, IEntity, ICanSendEvent,
    ICanGetModel, ICanGetSystem {
        [field:  SerializeField, SyncVar]
        public MoveMode MoveMode { get; set; } = MoveMode.ByPhysics;

        [field: SyncVar, SerializeField]
        public float MaxSpeed { get; protected set; }

        [field: SyncVar, SerializeField] 
        public bool CanBeHooked { get; set; } = true;

        [field: SyncVar, SerializeField]
        public bool CanBeAddedToInventory { get; set; }

        [field:SerializeField]
        public float AdditionalMassWhenHookedMultiplier { get; set; } = 2;

        //  [field: SyncVar, SerializeField] public bool CanBeUsed { get; set; } = false;

       // [field: SyncVar, SerializeField] public int Durability { get; set; } = -1;

      //  [field: SerializeField] public int MaxDurability { get; protected set; } = -1;


        [field: SyncVar(hook = nameof(OnHookStateChanged)), SerializeField] 
        public HookState HookState { get; protected set; } = HookState.Freed;


        [field: SyncVar, SerializeField]
        public NetworkIdentity HookedByIdentity { get; protected set; }

        [field: SerializeField]
        public NetworkIdentity LastHookedByIdentity { get; protected set; }


        [field: SerializeField]
        public Transform HookedByTransform { get; protected set; }

        private float originalMaxSpeed;
        
        protected virtual void OnEnable()
        {
            MaxSpeed =7f;
            StartCoroutine(RecoverMaxSpeed(originalMaxSpeed));
        }

        IEnumerator RecoverMaxSpeed(float originalSpeed) {
            yield return new WaitForSeconds(1f);
            MaxSpeed = originalSpeed;
        }
        public override void OnStartServer() {
            base.OnStartServer();
            bindedRigidibody = GetComponent<Rigidbody2D>();
            bindedRigidibody.mass = GetTotalMass();
        }

        /// <summary>
        /// Hook self if not hooked
        /// </summary>
        [ServerCallback]
        public bool TryHook(NetworkIdentity hookedBy) {
            if (CanBeHooked &&  ServerCheckCanHook(hookedBy)) {
                HookState = HookState.Hooked;
                HookedByIdentity = hookedBy;
                LastHookedByIdentity = HookedByIdentity;
                this.SendEvent<OnServerObjectHookStateChanged>(new OnServerObjectHookStateChanged()
                {
                    Identity = netIdentity,
                    HookState = HookState,
                    HookedByIdentity = hookedBy
                });
               // OnServerHooked();
                if (hookedBy && HookState == HookState.Hooked) {
                    HookedByTransform = hookedBy.GetComponentInChildren<Trigger2DCheck>().transform;
                    // LayerMask collisionMask = this.GetModel<ICollisionMaskModel>().Allocate();
                    //bindedRigidibody.bodyType = RigidbodyType2D.Kinematic;
                    bindedRigidibody.mass = GetTotalMass();
                    return true;
                }
                else {
                    HookedByTransform = null;
                }
                
            }

            return false;
        }

       

        [field: SerializeField] public bool canDealMomentumDamage { get; set; } = true;


        [ServerCallback]
        public void UnHook(bool isUnHookedByHookButton) {
            if (HookedByIdentity) {
                HookedByIdentity.GetComponent<IHookSystem>().UnHook(isUnHookedByHookButton);
            }
        }

        public bool Frozen { get; private set; } = false;

        public void SetFrozen(bool freeze) {
            Frozen = freeze;
            bindedRigidibody.constraints = freeze ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.None;
        }

        public void ResetEntity() {
            if (isServer) {
                UnHook(false);
                OnReset();
            }
        }

        public virtual void OnReset(){}

        [ServerCallback]
        protected virtual bool ServerCheckCanHook(NetworkIdentity hookedBy) {
            return true;
        }

        [ServerCallback]
        protected virtual void OnServerBeforeUnHooked(bool isUnhookedByHookButton) {

        }
    
        [ServerCallback]
        public virtual void OnServerHooked() {

        }
        [ServerCallback]
        protected virtual void OnServerUnHooked()
        {

        }

        /// <summary>
        /// Unhook self if hooked
        /// </summary>
        [ServerCallback]
        public void UnHookByHook(bool isShoot, bool isUnHookedByHookButton) {
            if (!bindedRigidibody || !this) {
                return;
            }
            //优化一下
            if (HookedByIdentity) {
                
                Debug.Log("UnHooked");
                OnServerBeforeUnHooked(isUnHookedByHookButton);
                /*
                HookedByIdentity.GetComponent<IHookSystem>().HookedItem = null;
                HookedByIdentity.GetComponent<IHookSystem>().HookedNetworkIdentity = null;
                HookedByIdentity.GetComponent<Animator>().SetBool("Hooking", false);*/
                HookState = HookState.Freed;
                
             
                if (!isShoot) {
                    bindedRigidibody.velocity = HookedByIdentity.GetComponent<Rigidbody2D>().velocity;
                    bindedRigidibody.angularVelocity = 0;
                }
                
                if (this is ICanBeUsed model) {
                    if (model.IsUsing) {
                        model.OnItemStopUsed();
                    }
                }
                //prevent hit player when unhooked
                //if (!GetComponent<Collider2D>().isTrigger) {
                //Invoke(nameof(RecoverCollider), 0.5f);
                //}
                //GetComponent<Collider2D>().isTrigger = true;
                NetworkIdentity hookeIdentity = HookedByIdentity;
                Physics2D.IgnoreCollision(hookeIdentity.GetComponent<Collider2D>(), GetComponent<Collider2D>(),
                    true);
                
             
                this.GetSystem<ITimeSystem>().AddDelayTask(0.2f, () => {
                    if (this && hookeIdentity != HookedByIdentity) {
                        Physics2D.IgnoreCollision(hookeIdentity.GetComponent<Collider2D>(), GetComponent<Collider2D>(),
                            false);
                    }
                });
                
                // this.GetModel<ICollisionMaskModel>().Release();
            }
            bindedRigidibody.mass = Mathf.Max(5,  GetTotalMass());
            this.SendEvent<OnServerObjectHookStateChanged>(new OnServerObjectHookStateChanged() {
                Identity = netIdentity,
                HookState = HookState,
                HookedByIdentity = this.HookedByIdentity
            });
            OnServerUnHooked();
            
            HookedByIdentity = null;
            HookedByTransform = null;

        }

       

        private void RecoverCollider() {
            GetComponent<Collider2D>().isTrigger = false;
        }
     

        [field: SyncVar, SerializeField]
        public float Acceleration { get; protected set; }


        [field: SyncVar, SerializeField]
        public abstract float SelfMass { get;  set; }

        protected Rigidbody2D bindedRigidibody; 

        protected virtual void Awake() {
            originalMaxSpeed = MaxSpeed;
            bindedRigidibody = GetComponent<Rigidbody2D>();
          
        }

        //[ServerCallback]
        public virtual float GetTotalMass() {
            if (HookState == HookState.Hooked) { 
                //hooked by somebody -> hooked.GetTotalMass() -> hooked.TotalMass()...
                //if hooked by somebody, that hooker must be another spaceship
                if (HookedByIdentity) {
                    return (HookedByIdentity.GetComponent<IHaveMomentumViewController>()).Model.GetTotalMass();
                }
              
            }

            return SelfMass;
        }

        public virtual void AddSpeedAndAcceleration(float percentage) {


            MaxSpeed *= (1 + percentage);
            Acceleration *= (1 + percentage);
        }

        


        public abstract string Name { get; set; }

      
       // private LayerMask clientOriginalLayer;
        protected virtual void OnHookStateChanged(HookState oldState, HookState newState) {
            if (newState == HookState.Hooked) {
                //gameObject.layer = LayerMask.NameToLayer("ClientHookedItem");
                OnClientHooked();
                if (HookedByIdentity&& HookedByIdentity.hasAuthority) {
                    OnOwnerHooked();
                }
            }

            if (newState == HookState.Freed)
            {
                if (this) {
                    OnClientFreed();
                }
              
            }
        }

      
        protected virtual void OnOwnerHooked() {

        }
        public abstract void OnClientHooked();
        
        
        public abstract void OnClientFreed();

    }
}

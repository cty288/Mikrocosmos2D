using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnServerObjectHookStateChanged {
        public NetworkIdentity Identity;
        public HookState HookState;
        public NetworkIdentity HookedByIdentity;
    }
    public abstract class AbstractBasicEntityModel : NetworkedModel, IEntity, ICanSendEvent,
    ICanGetModel {
        [field:  SerializeField, SyncVar]
        public MoveMode MoveMode { get; set; } = MoveMode.ByPhysics;

        [field: SyncVar, SerializeField]
        public float MaxSpeed { get; protected set; }

        [field: SyncVar, SerializeField] public bool CanBeHooked { get; set; } = true;

        [field: SyncVar, SerializeField]
        public bool CanBeAddedToInventory { get; set; }

      //  [field: SyncVar, SerializeField] public bool CanBeUsed { get; set; } = false;

       // [field: SyncVar, SerializeField] public int Durability { get; set; } = -1;

      //  [field: SerializeField] public int MaxDurability { get; protected set; } = -1;


        [field: SyncVar(hook = nameof(OnHookStateChanged)), SerializeField] 
        public HookState HookState { get; protected set; } = HookState.Freed;


        [field: SyncVar, SerializeField]
        public NetworkIdentity HookedByIdentity { get; protected set; }

     

        [field: SerializeField]
        public Transform HookedByTransform { get; protected set; }


        /// <summary>
        /// Hook self if not hooked
        /// </summary>
        [ServerCallback]
        public bool Hook(NetworkIdentity hookedBy) {
            if (CanBeHooked &&  ServerCheckCanHook(hookedBy)) {
                HookState = HookState.Hooked;
                HookedByIdentity = hookedBy;
                this.SendEvent<OnServerObjectHookStateChanged>(new OnServerObjectHookStateChanged()
                {
                    Identity = netIdentity,
                    HookState = HookState,
                    HookedByIdentity = hookedBy
                });
                OnServerHooked();
                if (hookedBy) {
                    HookedByTransform = hookedBy.GetComponentInChildren<Trigger2DCheck>().transform;
                    // LayerMask collisionMask = this.GetModel<ICollisionMaskModel>().Allocate();
                    //bindedRigidibody.bodyType = RigidbodyType2D.Kinematic;
                   
                }
                else {
                    HookedByTransform = null;
                }
               

                return true;
            }

            return false;
        }

        [ServerCallback]
        public void UnHook() {
            if (HookedByIdentity) {
                HookedByIdentity.GetComponent<IHookSystem>().UnHook();
            }
        }

        public void ResetEntity() {
            if (isServer) {
                UnHook();
                OnReset();
            }
        }

        public virtual void OnReset(){}

        [ServerCallback]
        protected virtual bool ServerCheckCanHook(NetworkIdentity hookedBy) {
            return true;
        }

        [ServerCallback]
        protected virtual void OnServerBeforeUnHooked() {

        }

        [ServerCallback]
        protected virtual void OnServerHooked() {

        }
        [ServerCallback]
        protected virtual void OnServerUnHooked()
        {

        }

        /// <summary>
        /// Unhook self if hooked
        /// </summary>
        [ServerCallback]
        public void UnHook(bool isShoot) {
            //优化一下
            if (HookedByIdentity) {
                
                Debug.Log("UnHooked");
                OnServerBeforeUnHooked();
                /*
                HookedByIdentity.GetComponent<IHookSystem>().HookedItem = null;
                HookedByIdentity.GetComponent<IHookSystem>().HookedNetworkIdentity = null;
                HookedByIdentity.GetComponent<Animator>().SetBool("Hooking", false);*/
                HookState = HookState.Freed;
                
                gameObject.layer = clientOriginalLayer;
                if (!isShoot) {
                    bindedRigidibody.velocity = HookedByIdentity.GetComponent<Rigidbody2D>().velocity;
                    bindedRigidibody.angularVelocity = 0;
                }

                //prevent hit player when unhooked
                if (!GetComponent<Collider2D>().isTrigger) {
                    Invoke(nameof(RecoverCollider), 0.1f);
                }
                GetComponent<Collider2D>().isTrigger = true;
                
                Physics2D.IgnoreCollision(HookedByIdentity.GetComponent<Collider2D>(), GetComponent<Collider2D>(),
                    false);

               // this.GetModel<ICollisionMaskModel>().Release();
            }

            this.SendEvent<OnServerObjectHookStateChanged>(new OnServerObjectHookStateChanged()
            {
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
        public abstract float SelfMass { get; protected set; }

        protected Rigidbody2D bindedRigidibody; 

        protected virtual void Awake() {
            bindedRigidibody = GetComponent<Rigidbody2D>();
            clientOriginalLayer = gameObject.layer;
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

    
        protected  virtual  void Update() {
            if (this) {
                bindedRigidibody.mass = GetTotalMass();
            }
          
        }
            
        

        public abstract string Name { get; set; }

      
        private LayerMask clientOriginalLayer;
        private void OnHookStateChanged(HookState oldState, HookState newState) {
            if (newState == HookState.Hooked) {
               

                //gameObject.layer = LayerMask.NameToLayer("ClientHookedItem");
                OnClientHooked();
            }

            if (newState == HookState.Freed)
            {
                if (this) {
                    gameObject.layer = clientOriginalLayer;
                    OnClientFreed();
                }
              
            }
        }

        

      
        public abstract void OnClientHooked();
        
        
        public abstract void OnClientFreed();

    }
}

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
    public abstract class AbstractBasicEntityModel : NetworkedModel, IEntity, ICanSendEvent {
     
        [field: SyncVar, SerializeField]
        public float MaxSpeed { get; protected set; }

       
       


        [field: SyncVar(hook = nameof(OnHookStateChanged)), SerializeField] 
        public HookState HookState { get; protected set; } = HookState.Freed;


        [field: SyncVar(hook = nameof(OnClientHookedByIdentityChanged)), SerializeField]
        public NetworkIdentity HookedByIdentity { get; protected set; }

     

        [field: SerializeField]
        public Transform ClientHookedByTransform { get; protected set; }


        /// <summary>
        /// Hook self if not hooked
        /// </summary>
        [ServerCallback]
        public bool Hook(NetworkIdentity hookedBy) {
            if (ServerCheckCanHook(hookedBy)) {
                HookState = HookState.Hooked;
                HookedByIdentity = hookedBy;
                this.SendEvent<OnServerObjectHookStateChanged>(new OnServerObjectHookStateChanged()
                {
                    Identity = netIdentity,
                    HookState = HookState,
                    HookedByIdentity = hookedBy
                });
                OnServerHooked();
                return true;
            }

            return false;
        }

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
                HookedByIdentity.GetComponent<IHookSystem>().HookedItem = null;
                HookedByIdentity.GetComponent<IHookSystem>().HookedNetworkIdentity = null;
                HookedByIdentity.GetComponent<Animator>().SetBool("Hooking", false);
                HookState = HookState.Freed;

                if (netIdentity.connectionToClient != null)
                {
                    TargetOnUnhooked(HookedByIdentity.GetComponent<Rigidbody2D>().velocity);
                }
                else
                {

                    bindedRigidibody.velocity += HookedByIdentity.GetComponent<Rigidbody2D>().velocity;
                }
            }

            this.SendEvent<OnServerObjectHookStateChanged>(new OnServerObjectHookStateChanged()
            {
                Identity = netIdentity,
                HookState = HookState,
                HookedByIdentity = this.HookedByIdentity
            });
            OnServerUnHooked();
            HookedByIdentity = null;
            
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
                return (HookedByIdentity.GetComponent<IHaveMomentumViewController>()).Model.GetTotalMass();
            }

            return SelfMass;
        }

        [ServerCallback]
        public virtual float GetMomentum() {
            return 0.5f * GetTotalMass() * bindedRigidibody.velocity.sqrMagnitude;
        }

        protected  virtual  void Update() {
            bindedRigidibody.mass = GetTotalMass();
            if (isClient) {
                if (HookState == HookState.Hooked) {
                    if (HookedByIdentity ==
                        NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship) {
                        gameObject.layer = LayerMask.NameToLayer("ClientHookedItem");
                    }
                    else {
                        gameObject.layer = clientOriginalLayer;
                    }
                }
            }
        }

        public abstract string Name { get; }

        [TargetRpc]
        private void TargetOnUnhooked(Vector2 velocity) {
            bindedRigidibody.velocity += velocity;
        }

        private LayerMask clientOriginalLayer;
        private void OnHookStateChanged(HookState oldState, HookState newState) {
            if (newState == HookState.Hooked) {
                if (HookedByIdentity ==
                    NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship) {
                    Debug.Log(NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.name);
                }

                gameObject.layer = LayerMask.NameToLayer("ClientHookedItem");
                OnClientHooked();
            }

            if (newState == HookState.Freed)
            {
                gameObject.layer = clientOriginalLayer;
                OnClientFreed();
            }
        }

        

        [ClientCallback]
        private void OnClientHookedByIdentityChanged(NetworkIdentity oldIdentity, NetworkIdentity newIdentity) {
            if (newIdentity) {
                ClientHookedByTransform = newIdentity.GetComponentInChildren<Trigger2DCheck>().transform;
            }
            else {
                ClientHookedByTransform = null;
            }
        }
        public abstract void OnClientHooked();
        
        
        public abstract void OnClientFreed();

    }
}

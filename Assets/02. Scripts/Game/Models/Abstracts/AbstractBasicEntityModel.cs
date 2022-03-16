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
        public void Hook(NetworkIdentity hookedBy) {
            HookState = HookState.Hooked;
            HookedByIdentity = hookedBy;
        }

      


        /// <summary>
        /// Unhook self if hooked
        /// </summary>
        [ServerCallback]
        public void UnHook() {
            HookState = HookState.Freed;
            HookedByIdentity = null;
        }

     

        [field: SyncVar, SerializeField]
        public float Acceleration { get; protected set; }

        public abstract float SelfMass { get; }

        protected Rigidbody2D bindedRigidibody; 

        protected virtual void Awake() {
            bindedRigidibody = GetComponent<Rigidbody2D>();
        }

        [ServerCallback]
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

        private void Update() {
            bindedRigidibody.mass = GetTotalMass();
        }

        public abstract string Name { get; }

        

        private void OnHookStateChanged(HookState oldState, HookState newState) {
            if (newState == HookState.Hooked)
            {
                OnHooked();
            }

            if (newState == HookState.Freed)
            {
                OnFreed();
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
        public abstract void OnHooked();
        
        
        public abstract void OnFreed();

    }
}

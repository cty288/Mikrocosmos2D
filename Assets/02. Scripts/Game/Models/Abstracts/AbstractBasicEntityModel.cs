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
    public abstract class AbstractBasicEntityModel : AbstractNetworkedController<Mikrocosmos>, IEntity, ICanSendEvent {
        [field: SyncVar(hook = nameof(OnClientSelfMassChanged)), SerializeField]
        public float SelfMass { get; protected set; }


        [field: SyncVar, SerializeField]
        public float MaxSpeed { get; protected set; }

       
       


        [field: SyncVar(hook = nameof(OnHookStateChanged)), SerializeField] 
        public HookState HookState { get; protected set; } = HookState.Freed;


        [field: SyncVar(hook = nameof(OnClientHookedByIdentityChanged)), SerializeField]
        public NetworkIdentity HookedByIdentity { get; protected set; }

        private NetworkConnectionToClient HookedByOriginalOwner;

        [field: SerializeField]
        public Transform ClientHookedByTransform { get; protected set; }


        /// <summary>
        /// Hook self if not hooked
        /// </summary>
        [ServerCallback]
        public void Hook(NetworkIdentity hookedBy) {
            HookedByOriginalOwner = connectionToClient;
            netIdentity.RemoveClientAuthority();
            netIdentity.AssignClientAuthority(hookedBy.connectionToClient);

            Debug.Log($"Authority assigned to {netIdentity.connectionToClient.identity.gameObject.name}");
            HookState = HookState.Hooked;
            HookedByIdentity = hookedBy;
        }

        public override void OnStopServer() {
            if (connectionToClient != HookedByOriginalOwner && HookedByOriginalOwner!=null) {
                UnHook();
            }
        }


        /// <summary>
        /// Unhook self if hooked
        /// </summary>
        [ServerCallback]
        public void UnHook() {
            Debug.Log(netIdentity.gameObject.name);
            netIdentity.RemoveClientAuthority();
            if (HookedByOriginalOwner != null) {
                netIdentity.AssignClientAuthority(HookedByOriginalOwner);
               // transform
            }
           
            HookState = HookState.Freed;
            HookedByIdentity = null;
            HookedByOriginalOwner = null;
        }

     

        [field: SyncVar, SerializeField]
        public float Acceleration { get; protected set; }

        protected Rigidbody2D bindedRigidibody;

        private void Awake() {
            bindedRigidibody = GetComponent<Rigidbody2D>();
        }

        [ServerCallback]
        public virtual float GetTotalMass() {
            return SelfMass;
        }

        [ServerCallback]
        public float GetMomentum() {
            return 0.5f * SelfMass * bindedRigidibody.velocity.sqrMagnitude;
        }


        public abstract string Name { get; }

        public abstract void OnClientSelfMassChanged(float oldMass, float newMass);


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

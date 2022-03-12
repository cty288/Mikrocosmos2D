using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public abstract class AbstractBasicEntityModel : AbstractNetworkedController<Mikrocosmos>, IEntity, ICanSendEvent {
        [field: SyncVar(hook = nameof(OnClientSelfMassChanged)), SerializeField]
        public float SelfMass { get; protected set; }


        [field: SyncVar, SerializeField]
        public float MaxSpeed { get; protected set; }


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

    }
}

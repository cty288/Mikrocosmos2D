using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class AbstractBasePlanetModel : NetworkedModel, IHaveGravity, ICanProducePackage{
        protected Rigidbody2D bindedRigidbody;
        [field: SerializeField]
        public LayerMask AffectedLayerMasks { get; set; }
        private void Awake() {
            bindedRigidbody = GetComponent<Rigidbody2D>();
        }

        float IHaveMomentum.MaxSpeed { get; }
        float IHaveMomentum.Acceleration { get; }

        [field: SerializeField, SyncVar]
        public float SelfMass { get; protected set; }
        public virtual float GetTotalMass() {
            return SelfMass;
        }

        public virtual float GetMomentum() {
            return 0.5f * GetTotalMass() * bindedRigidbody.velocity.sqrMagnitude;
        }

        [field: SerializeField, SyncVar]
        public float GravityFieldRange { get; protected set; }
        [field: SerializeField, SyncVar]
        public float G { get; protected set; }
    }

    
}

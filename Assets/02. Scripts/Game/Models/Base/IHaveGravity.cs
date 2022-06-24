using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IHaveGravity : IModel, IHaveMomentum {
       float GravityFieldRange { get; }
       float G { get; }

       LayerMask AffectedLayerMasks { get; set; }
    }



    public abstract class AbstractHaveGravityModel : NetworkedModel, IHaveGravity {
        protected Rigidbody2D bindedRigidbody;

        [field: SerializeField] 
        public LayerMask AffectedLayerMasks { get; set; } //player obst planet

        private void Awake() {
            bindedRigidbody = GetComponent<Rigidbody2D>();
        }

        [field: SerializeField] public MoveMode MoveMode { get; set; } = MoveMode.ByTransform;

        float IHaveMomentum.MaxSpeed { get; }
        float IHaveMomentum.Acceleration { get; }

        [field: SerializeField, SyncVar]
        public float SelfMass { get;  set; }

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

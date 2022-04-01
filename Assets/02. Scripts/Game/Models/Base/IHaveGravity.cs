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
    }

    public abstract class AbstractHaveGravityModel : NetworkedModel, IHaveGravity {
        protected Rigidbody2D bindedRigidbody;
        [SerializeField] protected LayerMask affectedLayerMasks;

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

        private void FixedUpdate() {
            if (isServer) {
                KeepUniversalG();
            }
        }

        [ServerCallback]
        void KeepUniversalG()
        {
            Vector2 Center = this.transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(Center, GravityFieldRange, affectedLayerMasks);

            foreach (Collider2D obj in colliders)
            {

                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                
                if (rb && obj.gameObject != gameObject)
                {
                    if (rb.TryGetComponent<IAffectedByGravity>(out IAffectedByGravity target)) {
                        float explosionForce = -1 * UniversalG(this, target, transform.position, rb.transform.position) * Time.deltaTime;
                        target.ServerAddGravityForce(explosionForce, Center, GravityFieldRange);
                    }
                  
                }

            }

        }

        private float UniversalG(IHaveMomentum source, IHaveMomentum target, Vector2 sourcePos, Vector2 targetPos) {

            float sorceMass = source.GetTotalMass();
            float destMass = target.GetTotalMass();
            return (sorceMass * destMass / Distance(sourcePos, targetPos)) * G;

        }

        protected float Distance(Vector2 pos1, Vector2 pos2)
        {
            Vector2 diff = (pos1 - pos2);
            float dist = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (dist < 1)
                return 1;
            else return (dist);
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

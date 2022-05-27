using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IAffectedByGravity : IModel, IHaveMomentum {
        void ServerAddGravityForce(float force, Vector2 position, float range);
        Vector2 StartDirection { get; }

        float InitialForceMultiplier { get; }
    }

    public abstract class AbstractAffectedByGravityModel : NetworkedModel, IAffectedByGravity {
        protected Rigidbody2D bindedRigidbody;

        private void Awake()
        {
            bindedRigidbody = GetComponent<Rigidbody2D>();
        }

        float IHaveMomentum.MaxSpeed { get; }
        float IHaveMomentum.Acceleration { get; }

        [field: SerializeField, SyncVar]
        public float SelfMass { get; protected set; }
        public virtual float GetTotalMass() {
            return SelfMass;
        }

        public float GetMomentum() {
            return 0.5f * GetTotalMass() * bindedRigidbody.velocity.sqrMagnitude;
        }

        [ServerCallback]
        public void ServerAddGravityForce(float force, Vector2 position, float range) {
            bindedRigidbody.AddExplosionForce(force, position, range);
            if (connectionToClient != null) {
                TargetOnClientGravityForceAdded(force, position, range); //for client authority
            }
        }

        [field: SerializeField]
        public Vector2 StartDirection { get; protected set; }
        [field: SerializeField]
        public float InitialForceMultiplier { get; protected set; }

    
        protected float initialForce;

        public override void OnStartServer() {
            Vector2 Center = this.transform.position;
            initialForce = ProperForce();
            this.gameObject.GetComponent<Rigidbody2D>().AddForce(initialForce * ProperDirect(Center), ForceMode2D.Impulse);
        }

       [ServerCallback]
        private float ProperForce()
        {
            var pos = transform.position;
            var rb = GetComponent<Rigidbody2D>();
            var Rb = GameObject.Find("Star").GetComponent<IHaveGravity>();
            return InitialForceMultiplier * GetTotalMass() * Mathf.Sqrt(Rb.GetTotalMass() / Distance(pos, Vector3.zero));
        }

        private Vector2 ProperDirect(Vector2 pos) {
            float x = Random.value, y = Random.value / 10;
            Vector2 result;
            if (StartDirection != Vector2.zero)
            {
                result = StartDirection.normalized;
            }
            else
            {
                Vector2 starPos = GameObject.Find("Star").transform.position;
                result = Vector2.Perpendicular(((starPos - pos).normalized));
            }
            return result;
        }


        [TargetRpc]
        public virtual void TargetOnClientGravityForceAdded(float force, Vector2 position, float range) { }
        float Distance(Vector2 pos1, Vector2 pos2)
        {
            Vector2 diff = (pos1 - pos2);
            float dist = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (dist < 1)
                return 1;
            else return (dist);
        }
    }

   
}

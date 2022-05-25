using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IMeteorModel: IModel, IAffectedByGravity {

    }
    public class MeteorModel : AbstractDamagableEntityModel, IMeteorModel {
        [field: SyncVar, SerializeField]
        public override float SelfMass { get; protected set; } = 5f;
        public override string Name { get; set; } = "Meteor";
        public override void OnClientHooked() {
            
        }

        public override void OnClientFreed() {
           
        }

        [ServerCallback]
        public void ServerAddGravityForce(float force, Vector2 position, float range) {
            GetComponent<Rigidbody2D>().AddExplosionForce(force, position, range);
        }

        [field: SerializeField] public Vector2 StartDirection { get; protected set; }

        [field: SerializeField]
        public float InitialForceMultiplier { get; protected set; }

        [field: SerializeField] public bool AffectedByGravity { get; set; } = true;

        protected float initialForce;
        public override void OnStartServer()
        {
            Vector2 Center = this.transform.position;
            initialForce = ProperForce();
            this.gameObject.GetComponent<Rigidbody2D>().AddForce(initialForce * ProperDirect(Center), ForceMode2D.Impulse);
        }

        public override int GetDamageFromExcessiveMomentum(float excessiveMomentum) {
            return Mathf.RoundToInt(excessiveMomentum);
        }

        public override void OnServerTakeDamage(int oldHealth, int newHealth) {
            
        }

        public override void OnReceiveExcessiveMomentum(float excessiveMomentum) {
            Debug.Log($"Meteor Excessive Momentum: {excessiveMomentum}");
        }

        [ServerCallback]
        private float ProperForce()
        {
            var pos = transform.position;
            var rb = GetComponent<Rigidbody2D>();
            var Rb = GameObject.Find("Star").GetComponent<IHaveGravity>();
            return InitialForceMultiplier * GetTotalMass() * Mathf.Sqrt(Rb.GetTotalMass() / Distance(pos, Vector3.zero));
        }

        private Vector2 ProperDirect(Vector2 pos)
        {
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

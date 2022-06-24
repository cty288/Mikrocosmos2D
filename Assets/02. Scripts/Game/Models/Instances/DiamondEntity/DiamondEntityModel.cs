using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class DiamondEntityModel : AbstractBasicEntityModel, IAffectedByGravity {
        public override float SelfMass { get;  set; } = 1f;
        public override string Name { get; set; }

        private DiamondEntityViewController vc;

        protected override void Awake() {
            base.Awake();
            vc = GetComponent<DiamondEntityViewController>();
        }

        public override void OnClientHooked() {
            
        }

        public override void OnClientFreed() {
            
        }

        public void ServerAddGravityForce(float force, Vector2 position, float range) {
            if (!vc.Attracted) {
                GetComponent<Rigidbody2D>().AddExplosionForce(force, position, range);
            }
            
        }

        public Vector2 StartDirection { get; }
        public float InitialForceMultiplier { get; }

        [field: SerializeField]
        public bool AffectedByGravity { get; set; }
    }
}

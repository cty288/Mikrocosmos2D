using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class EarthBagModel : AbstractDamagableEntityModel {
        [field: SerializeField]
        public override float SelfMass { get; set; }
        [field: SerializeField]
        public override string Name { get; set; }

        [SerializeField] private float damagerPerMomentum = 1;
        public override void OnClientHooked() {
            
        }

        public override void OnClientFreed() {
          
        }

        public override int GetDamageFromExcessiveMomentum(float excessiveMomentum) {
            return Mathf.RoundToInt(damagerPerMomentum * excessiveMomentum);
        }

        public override void OnServerTakeDamage(int oldHealth, NetworkIdentity damageDealer, int newHealth) {
           
        }

        public override void OnReceiveExcessiveMomentum(float excessiveMomentum) {
          
        }
    }
}

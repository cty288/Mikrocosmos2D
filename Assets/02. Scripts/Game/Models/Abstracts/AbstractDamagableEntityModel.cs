using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnEntityTakeDamage {
        public IDamagable Entity;
        public int NewHealth;
    }
    public abstract class AbstractDamagableEntityModel : AbstractBasicEntityModel, IDamagable{

        [field: SerializeField]
        public float MaxMomentumReceive { get; protected set; }
        [field: SerializeField]
        public float MomentumThredhold { get; protected set; }

        [field: SerializeField]
        public int MaxHealth { get; protected set; }
        
        [field:  SyncVar]
        public int CurrentHealth { get; set; }

        public override void OnStartServer() {
            base.OnStartServer();
            CurrentHealth = MaxHealth;
        }

        public virtual float TakeRawMomentum(GameObject hit, float offset) {
            if (hit && hit.TryGetComponent<IHaveMomentum>(out IHaveMomentum hitter)) {
                Vector2 hitterMomentum = Vector2.zero;
                if (hitter.MoveMode == MoveMode.ByPhysics) {
                    hitterMomentum = hit.GetComponent<Rigidbody2D>().velocity * hitter.GetTotalMass();
                }

                

                float selfMomentum = (-bindedRigidibody.velocity * GetTotalMass() + hitterMomentum).magnitude;
                Debug.Log($"Self Raw Momentum Received: {selfMomentum}");
                selfMomentum += offset;
                selfMomentum -= MomentumThredhold;
                selfMomentum = Mathf.Clamp(selfMomentum, 0, MaxMomentumReceive);
             
                return selfMomentum;
            }

            return 0;

        }


        public abstract int GetDamageFromExcessiveMomentum(float excessiveMomentum);
        public void TakeRawDamage(int damage) {
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(0, CurrentHealth);
            OnHealthChange(CurrentHealth);
            this.SendEvent<OnEntityTakeDamage>(new OnEntityTakeDamage() {
                Entity = this,
                NewHealth = CurrentHealth
            });
        }

        public abstract void OnHealthChange(int newHealth);
        public abstract void OnReceiveExcessiveMomentum(float excessiveMomentum);
    }
}

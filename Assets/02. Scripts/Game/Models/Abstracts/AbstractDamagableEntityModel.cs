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
        
        [field:  SyncVar, SerializeField]
        public int CurrentHealth { get; set; }

        [SerializeField] private float momentumCoolDown = 0.1f;
        private float momentumCoolDownTimer = 0f;
        public override void OnStartServer() {
            base.OnStartServer();
            CurrentHealth = MaxHealth;
        }

        protected override void Update() {
            base.Update();
            momentumCoolDownTimer += Time.deltaTime;
        }

        public virtual float TakeRawMomentum(float rawMomentum, float offset) {
            if (momentumCoolDownTimer >= momentumCoolDown) {
                momentumCoolDownTimer = 0;
                Debug.Log($"Received Raw Momentum: {rawMomentum}");
                rawMomentum += offset;
                rawMomentum -= MomentumThredhold;
                rawMomentum = Mathf.Clamp(rawMomentum, 0, MaxMomentumReceive);
                return rawMomentum;
            }

            momentumCoolDownTimer = 0;
            return 0;
        }


        public abstract int GetDamageFromExcessiveMomentum(float excessiveMomentum);
        public void TakeRawDamage(int damage) {
            int oldHealth = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(0, CurrentHealth);
            OnServerTakeDamage(oldHealth, CurrentHealth);
            this.SendEvent<OnEntityTakeDamage>(new OnEntityTakeDamage() {
                Entity = this,
                NewHealth = CurrentHealth
            });
        }

        public abstract void OnServerTakeDamage(int oldHealth, int newHealth);
        public abstract void OnReceiveExcessiveMomentum(float excessiveMomentum);
    }
}

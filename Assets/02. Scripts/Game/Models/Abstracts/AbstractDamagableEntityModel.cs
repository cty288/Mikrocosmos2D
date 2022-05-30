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
        public int OldHealth;
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

        [SerializeField] private int healthRecoverThreshold = 50;
        [SerializeField] private int healthRecoverPerSecond = 10;
        [SerializeField] private int healthRecoverWaitTimeAfterDamage = 5;

        [SerializeField]
        private float HealthRecoverTimer = 0f;
        private bool healthRecoverStart = false;

        public override void OnStartServer() {
            base.OnStartServer();
            CurrentHealth = MaxHealth;
            Invoke(nameof(StartRecoverHealthCoroutine), 1f);
        }

        private void StartRecoverHealthCoroutine() {
            if (gameObject.activeInHierarchy) {
                StopAllCoroutines();
                StartCoroutine(RecoverHealth());
            }
        
        }

        private IEnumerator RecoverHealth() {
            while (true) {
                yield return new WaitForSeconds(1f);
                HealthRecoverTimer++;
                if (HealthRecoverTimer > healthRecoverWaitTimeAfterDamage) {
                    if (CurrentHealth <= healthRecoverThreshold) {
                        healthRecoverStart = true;
                    }

                    if (healthRecoverStart) {
                        AddHealth(healthRecoverPerSecond);
                    }

                    if (CurrentHealth >= MaxHealth) {
                        healthRecoverStart = false;
                    }
                }
            }
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
            if(damage<=0){
                return;
            }
            if (TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                if (buffSystem.HasBuff<InvincibleBuff>()) {
                    return;
                }
            }
            
            int oldHealth = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(0, CurrentHealth);
            OnServerTakeDamage(oldHealth, CurrentHealth);
            this.SendEvent<OnEntityTakeDamage>(new OnEntityTakeDamage() {
                Entity = this,
                NewHealth = CurrentHealth,
                OldHealth = oldHealth
            });
            //Debug.Log("Health Recover Timer 0");
            HealthRecoverTimer = 0f;
            healthRecoverStart = false;
        }

        public void AddHealth(int health) {
            CurrentHealth += health;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }

        public abstract void OnServerTakeDamage(int oldHealth, int newHealth);
        public abstract void OnReceiveExcessiveMomentum(float excessiveMomentum);
    }
}

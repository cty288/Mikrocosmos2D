using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Mikrocosmos
{
    public struct OnEntityTakeDamage {
        public IDamagable Entity;
        public int NewHealth;
        public int OldHealth;
        public NetworkIdentity DamageSource;
    }
    public abstract class AbstractDamagableEntityModel : AbstractBasicEntityModel, IDamagable{

        [field: SerializeField]
        public float MaxMomentumReceive { get; protected set; }
        [field: SerializeField]
        public float MomentumThredhold { get; protected set; }

        [field: SerializeField, SyncVar(hook = nameof(OnClientMaxHealthChange))]
        public int MaxHealth { get; protected set; }
        
        [field:  SyncVar(hook = nameof(OnClientHealthChange)), SerializeField]
        public int CurrentHealth { get; set; }

        [SerializeField] private float momentumCoolDown = 0.1f;


        private DateTime momentumCoolDownTime;

        [SerializeField] private int healthRecoverThreshold = 50;
        [SerializeField] private int healthRecoverPerSecond = 10;
        [SerializeField] private int healthRecoverWaitTimeAfterDamage = 5;
        [SerializeField] private int maxDamageReceive = 1000;

        [SerializeField]
        private float HealthRecoverTimer = 0f;
        private bool healthRecoverStart = false;

        public override void OnStartServer() {
            base.OnStartServer();
            CurrentHealth = MaxHealth;
            Invoke(nameof(StartRecoverHealthCoroutine), 1f);
            momentumCoolDownTime = DateTime.Now;
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

       
        public virtual float TakeRawMomentum(float rawMomentum, float offset) {
            if ((DateTime.Now - momentumCoolDownTime).TotalSeconds >= momentumCoolDown) {
                momentumCoolDownTime = DateTime.Now;
                Debug.Log($"Received Raw Momentum: {rawMomentum}");
                float tempRawMomentum = rawMomentum;
               
                rawMomentum -= MomentumThredhold;
                rawMomentum = Mathf.Clamp(rawMomentum, 0, MaxMomentumReceive);
                rawMomentum += offset;
                Debug.Log($"Momentum Received by {gameObject.name}: {rawMomentum}. Raw Momentum: {tempRawMomentum}");
                return rawMomentum;
            }

          
            return 0;
        }


        public abstract int GetDamageFromExcessiveMomentum(float excessiveMomentum);
        public int TakeRawDamage(int damage, NetworkIdentity damageDealer, int additionalDamage =0) {
            if(damage<0){
                return 0;
            }
            if (TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                if (buffSystem.HasBuff<InvincibleBuff>()) {
                    return 0;
                }
            }
            
            int oldHealth = CurrentHealth;
            damage = Mathf.Clamp(damage, 0, maxDamageReceive);
            CurrentHealth -= (damage + additionalDamage);
            CurrentHealth = Mathf.Max(0, CurrentHealth);
            OnServerTakeDamage(oldHealth, damageDealer, CurrentHealth);
            this.SendEvent<OnEntityTakeDamage>(new OnEntityTakeDamage() {
                Entity = this,
                NewHealth = CurrentHealth,
                OldHealth = oldHealth,
                DamageSource = damageDealer
            });
            //Debug.Log("Health Recover Timer 0");
            HealthRecoverTimer = 0f;
            healthRecoverStart = false;
            return damage;
        }

        public void AddHealth(int health) {
            if (CurrentHealth < MaxHealth) {
                CurrentHealth += health;
                CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
            }
           
        }

        public abstract void OnServerTakeDamage(int oldHealth, NetworkIdentity damageDealer, int newHealth);
        public abstract void OnReceiveExcessiveMomentum(float excessiveMomentum);

        public virtual void OnClientHealthChange(int oldHealth, int newHealth) {

        }

        public virtual void OnClientMaxHealthChange(int oldMaxHealth, int newMaxHealth)
        {

        }
    }
}

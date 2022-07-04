using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class StoneShieldBulletViewController : BasicBulletViewController {

        public float LastTime = 3f;
        public float FlySpeed = 10f;
        private int damage;
        
        public int Damage {
            get => damage;
            set => damage = value;
        }

        private float flyTimer;
        private bool hitTriggered = false;
        
        protected override void OnCollisionEnter2D(Collision2D collision) {
            Debug.Log("StoneShieldBulletViewController Collision Enter 2d");
        }

        private void OnEnable() {
            Debug.Log("StoneShieldBullet Enabled");
            flyTimer = 0;
        }

        public override void OnReset() {
            base.OnReset();
            flyTimer = 0f;
            hitTriggered = false;
            damage = 0;
        }


        protected override void Update() {
            base.Update();
            if (isServer && gameObject.activeInHierarchy) {
                flyTimer += Time.deltaTime;
                if (flyTimer >= LastTime && !hitTriggered) {
                    hitTriggered = true;
                    animator.SetTrigger("Hit");
                }
            }
        }

        private void OnDisable() {
            Debug.Log("On StoneShield Disabled");
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (isServer) {
                transform.position += -transform.right * (FlySpeed * Time.fixedDeltaTime);
            }
          
        }

        private void OnTriggerEnter2D(Collider2D collider) {
            if (isServer && !hitTriggered)
            {
                if (collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum entity)) {
                    if (entity is ICanAbsorbDamage canAbsorbDamage && shooterWeapon != collider)
                    {
                        if (canAbsorbDamage.AbsorbDamage) {
                            canAbsorbDamage.OnAbsorbDamage(damage);
                            hitTriggered = true;
                            animator.SetTrigger("Hit");
                        }
                    }
                    else {
                        if (entity is IDamagable damagable) {
                            //Debug.Log("Bullet Speed: " + rigidbody.velocity.magnitude);
                            damagable.TakeRawDamage(Damage, shooterPlayer);
                        }
                    }
                }
            }
        }
    }
}

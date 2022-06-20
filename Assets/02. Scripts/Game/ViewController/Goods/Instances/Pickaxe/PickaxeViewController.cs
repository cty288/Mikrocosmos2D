using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class PickaxeViewController : BasicGoodsViewController {
        private PickaxeModel pickaxeModel;

        protected override void Awake() {
            base.Awake();
            pickaxeModel = GetComponent<PickaxeModel>();
        }

        protected override void OnCollisionEnter2D(Collision2D collision) {
            if (isServer) {
                if (collision.collider.CompareTag("Meteor")) {
                    Model.MoveMode = MoveMode.ByPhysics;
                }
                else {
                    Model.MoveMode = MoveMode.ByTransform;
                }
            }
           
            base.OnCollisionEnter2D(collision);
        }

        private void OnCollisionExit2D(Collision2D other) {
            Model.MoveMode = MoveMode.ByTransform;
        }

        protected override IEnumerator PhysicsForceCalculation(IDamagable targetModel, Rigidbody2D targetRigidbody) {
            float waitTime = 0.02f;
            Vector2 offset = Vector2.zero;
            if (targetModel is ISpaceshipConfigurationModel)
            {
                targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = false;
                targetRigidbody.GetComponent<PlayerSpaceship>().RecoverCanControl(waitTime);
            }

            Vector2 speed1 = targetRigidbody.velocity;
            yield return new WaitForSeconds(waitTime);
            if (targetRigidbody)
            {
                Vector2 speed2 = targetRigidbody.velocity;

                Vector2 acceleration = (speed2 - speed1) / waitTime;
                Debug.Log($"Speed1: {speed1}, Speed 2: {speed2}, Acceleration: {acceleration}. " +
                          $"Fixed Dealta Time : {Time.fixedDeltaTime}");
                if (targetModel != null)
                {
                    Vector2 force = acceleration * Mathf.Sqrt(targetModel.GetTotalMass());
                    if (targetModel is ISpaceshipConfigurationModel model)
                    {
                        force *= speed2.magnitude / model.MaxSpeed;
                    }

                    force = new Vector2(Mathf.Sign(force.x) * Mathf.Log(Mathf.Abs(force.x)),
                        Mathf.Sign(force.y) * Mathf.Log(Mathf.Abs(force.y), 2));
                    force *= 2;
                    float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                    if (targetRigidbody.gameObject.CompareTag("Meteor")) {
                        excessiveMomentum += (excessiveMomentum / targetModel.MaxMomentumReceive) *
                                             pickaxeModel.DamageToMeteors;
                    }
                    targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                    targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum));
                }

            }
        }
    }
}

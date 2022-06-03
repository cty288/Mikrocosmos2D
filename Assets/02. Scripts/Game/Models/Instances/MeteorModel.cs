using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public interface IMeteorModel: IModel, IAffectedByGravity {
        List<GameObject> Rewards { get; }

        public void StartAddTorqueForce(Vector2 addForce, float time);
    }
    public class MeteorModel : AbstractDamagableEntityModel, IMeteorModel {

        private float torqueForceTimer = 0;
        private Vector2 torqueForce;


        [field: SerializeField]
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

        private void FixedUpdate() {
            if (isServer) {
                torqueForceTimer -= Time.fixedDeltaTime;
                if (torqueForceTimer > 0) {
                    bindedRigidibody.AddForce(torqueForce, ForceMode2D.Impulse);
                }
            }
        }

        [field: SerializeField] public Vector2 StartDirection { get; protected set; }

        [field: SerializeField]
        public float InitialForceMultiplier { get; protected set; }

        [field: SerializeField] public bool AffectedByGravity { get; set; } = true;

        protected float initialForce;
        public override void OnStartServer()
        {
            base.OnStartServer();
            Vector2 Center = this.transform.position;
            initialForce = ProperForce();
            this.gameObject.GetComponent<Rigidbody2D>().AddForce(initialForce * ProperDirect(Center), ForceMode2D.Impulse);
        }


        

        public override int GetDamageFromExcessiveMomentum(float excessiveMomentum) {
            return Mathf.RoundToInt(excessiveMomentum);
        }

        public override void OnServerTakeDamage(int oldHealth, int newHealth) {
            if (newHealth <= 0) {
                this.SendEvent<OnMeteorDestroyed>(new OnMeteorDestroyed() {
                    Meteor = gameObject
                });
            }
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

        [field:SerializeField]
        public List<GameObject> Rewards { get; protected set; }

        public void StartAddTorqueForce(Vector2 addForce, float time) {
            torqueForce = addForce;
            torqueForceTimer = time;
            
        }
    }
}

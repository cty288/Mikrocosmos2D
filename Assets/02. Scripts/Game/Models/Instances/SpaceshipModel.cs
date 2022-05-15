using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public struct OnMassChanged
    {
        public float newMass;
    }

    public struct OnEscapeCounterChanged {
        public int newValue;
    }
    public class SpaceshipModel : AbstractBasicEntityModel, ISpaceshipConfigurationModel, IAffectedByGravity {
        public override string Name { get; } = "Spaceship";
        private IHookSystem hookSystem;
        private Rigidbody2D rigidbody;
        #region Server

        [Command]
        private void CmdUnHook() {
            UnHook();
        }
      
        #endregion

        #region Client

        protected override void Awake() {
            base.Awake();
            hookSystem = GetComponent<IHookSystem>();
            rigidbody = GetComponent<Rigidbody2D>();
        }

        public override void OnClientHooked()
        {

        }

        public override void OnClientFreed()
        {

        }
        public int EscapeNeedCount { get; } = 10;
        public float EscapeLossTime { get; } = 0.125f;
        public float MaxMaxSpeed { get; } = 40;

        public int EscapeCounter { get; private set; }

        private float escapeLossTimer = 0f;

       

        protected override void Update()
        {
            base.Update();
            if (isServer) {
                Acceleration = Mathf.Max(5, InitialAcceleration - GetTotalMass() * AccelerationDecreasePerMass);
            }
            if (hasAuthority)
            {
                escapeLossTimer += Time.deltaTime;
                if (escapeLossTimer >= EscapeLossTime)
                {
                    escapeLossTimer = 0;
                    if (EscapeCounter > 0)
                    {
                        EscapeCounter--;
                        this.SendEvent<OnEscapeCounterChanged>(new OnEscapeCounterChanged() { newValue = EscapeCounter });
                    }

                }
            }

        }

        public void IncreaseEscapeCounter()
        {
            EscapeCounter++;
            escapeLossTimer = 0;
            if (EscapeCounter >= EscapeNeedCount)
            {
                EscapeCounter = 0;
                CmdUnHook();
                
            }
            this.SendEvent<OnEscapeCounterChanged>(new OnEscapeCounterChanged() { newValue = EscapeCounter });
        }

        [field: SerializeField]
        public float InitialAcceleration { get; private set; } = 20;

        [field: SyncVar, SerializeField]
        public override float SelfMass { get; protected set; } = 1;
        [field: SerializeField]
        public float AccelerationDecreasePerMass { get; private set; } = 2;

        public float BackpackMass { get; } = 0;
    
        public float GetConnectedObjectSoleMass() {
            if (hookSystem.HookedNetworkIdentity == null) {
                return 0;
            }
            IHaveMomentum hookingModel = hookSystem.HookedNetworkIdentity.GetComponent<IHaveMomentumViewController>().Model;

            if (hookingModel is ISpaceshipConfigurationModel) {
                ISpaceshipConfigurationModel spaceshipModel = (hookingModel as ISpaceshipConfigurationModel);
                return spaceshipModel.SelfMass + spaceshipModel.BackpackMass +
                       spaceshipModel.GetConnectedObjectSoleMass();
            }
            else {
                return hookingModel.SelfMass;
            }
        }

        

        public override float GetTotalMass() {
            /*
             * hooked by somebody -> hookedBy.GetTotalMass() -> hookedBy.TotalMass()...
             * hooking somebody && !hooked by somebody -> (getRigidbodyMass + backpack) of all hooked player; add together
             * !hooking somebody && !hooked by somebody -> (getRigidbodyMass+backpack_)
             */

            //HookState
            if (HookState == HookState.Hooked) {  //hooked by somebody -> hooked.GetTotalMass() -> hooked.TotalMass()...
                //if hooked by somebody, that hooker must be another spaceship
                return (HookedByIdentity.GetComponent<IHaveMomentumViewController>()).Model.GetTotalMass();
            }
            else {
                if (hookSystem.IsHooking && hookSystem.HookedNetworkIdentity)
                {//return SelfMass + backpack + ...
                    IHaveMomentum hookingModel =  hookSystem.HookedNetworkIdentity.GetComponent<IHaveMomentumViewController>().Model;
                    
                   if (hookingModel is ISpaceshipConfigurationModel) {
                       return GetConnectedObjectSoleMass() + SelfMass + BackpackMass;
                   }else {
                       return SelfMass + BackpackMass + hookingModel.SelfMass;
                   }
                }
            }
            return SelfMass + BackpackMass;
        }


        [TargetRpc]
        public  void TargetOnClientGravityForceAdded(float force, Vector2 position, float range) {
            
              rigidbody .AddExplosionForce(force, position, range);
        }

        #endregion

        #region Server
        protected float initialForce;
        public override void OnStartServer()
        {
            Vector2 Center = this.transform.position;
            initialForce = ProperForce();
            this.gameObject.GetComponent<Rigidbody2D>().AddForce(initialForce * ProperDirect(Center), ForceMode2D.Impulse);
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
        [ServerCallback]
        public void ServerAddGravityForce(float force, Vector2 position, float range)
        {
            TargetOnClientGravityForceAdded(force, position, range);
        }

        [field: SerializeField]
        public Vector2 StartDirection { get; }

        [field: SerializeField] public float InitialForceMultiplier { get; } = 0;

        #endregion


    }
}

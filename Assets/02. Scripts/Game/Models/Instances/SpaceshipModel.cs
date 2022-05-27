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

    public struct OnSpaceshipRequestDropItems {
        public NetworkIdentity SpaceshipIdentity;
        public int NumberItemRequest;

    }
    public class SpaceshipModel : AbstractDamagableEntityModel, ISpaceshipConfigurationModel, IAffectedByGravity {
        public override string Name { get; set; } = "Spaceship";
        private IHookSystem hookSystem;
        private Rigidbody2D rigidbody;


        [SerializeField] 
        private float damagePerMomentum;

        [SerializeField] protected int dropOneItemMomentumThreshold = 3;
        #region Server

        
      
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
        public float EscapeLossTime { get; } = 0.15f;
        public float MaxMaxSpeed { get; } = 100;

        [field: SyncVar(hook = nameof(ClientOnEscapeCounterChanged))]
        public int EscapeCounter { get; private set; }

        private float escapeLossTimer = 0f;

       

        protected override void Update()
        {
            base.Update();
            if (isServer) {
                Acceleration = Mathf.Max(15, InitialAcceleration - GetTotalMass() * AccelerationDecreasePerMass);

                escapeLossTimer += Time.deltaTime;
                if (escapeLossTimer >= EscapeLossTime)
                {
                    escapeLossTimer = 0;
                    if (EscapeCounter > 0)
                    {
                        EscapeCounter--;
                    }

                }
            }
        }

        [Command]
        public void CmdIncreaseEscapeCounter()
        {
            EscapeCounter++;
            escapeLossTimer = 0;
            if (EscapeCounter >= EscapeNeedCount)
            {
                EscapeCounter = 0;
                UnHook();
            }

        }

        [field: SyncVar,SerializeField]
        public float InitialAcceleration { get; private set; } = 20;

        [field: SyncVar, SerializeField]
        public override float SelfMass { get; protected set; } = 1;
        [field: SyncVar, SerializeField]
        public float AccelerationDecreasePerMass { get; private set; } = 2;

        public float BackpackMass {
            get {
                float mass = 0;
                
                if (TryGetComponent<IPlayerInventorySystem>(out IPlayerInventorySystem inventory)) {
                    List<BackpackSlot> slots = inventory.BackpackItems;
                    foreach (BackpackSlot slot in slots) {
                        foreach (GameObject stackedObject in slot.StackedObjects) {
                            mass += stackedObject.GetComponent<IHookable>().SelfMass;
                        }
                    }
                }

                return mass;
            }
        }

        public float GetConnectedObjectSoleMass() {
            if (hookSystem.HookedNetworkIdentity == null) {
                return 0;
            }
            IHookable hookingModel = hookSystem.HookedNetworkIdentity.GetComponent<IHookable>();

            if (hookingModel == null) {
                return 0;
            }

            if (hookingModel is ISpaceshipConfigurationModel) {
                ISpaceshipConfigurationModel spaceshipModel = (hookingModel as ISpaceshipConfigurationModel);
                return spaceshipModel.SelfMass + spaceshipModel.BackpackMass +
                       spaceshipModel.GetConnectedObjectSoleMass();
            }
            else {
                if (hookingModel.CanBeAddedToInventory) {
                    return 0;
                }
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
                    IHookable hookingModel =  hookSystem.HookedNetworkIdentity.GetComponent<IHookable>();
                    
                   if (hookingModel is ISpaceshipConfigurationModel) {
                       return GetConnectedObjectSoleMass() + SelfMass + BackpackMass;
                   }else {
                       float hookingModelSelfMass = 0;
                       if (hookingModel.CanBeAddedToInventory) {
                           hookingModelSelfMass = hookingModel.SelfMass * 4;
                       }
                        return SelfMass + BackpackMass + hookingModelSelfMass;
                   }
                }
            }
            return SelfMass + BackpackMass;
        }


        [ClientCallback]
        private void ClientOnEscapeCounterChanged(int oldNum, int newNum) {
            this.SendEvent<OnEscapeCounterChanged>(new OnEscapeCounterChanged() { newValue = newNum });
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
            rigidbody.AddExplosionForce(force, position, range);
        }

        [field: SerializeField]
        public Vector2 StartDirection { get; }

        [field: SerializeField] public float InitialForceMultiplier { get; } = 0;
        [field: SerializeField] public bool AffectedByGravity { get; set; } = true;

        #endregion

        public override int GetDamageFromExcessiveMomentum(float excessiveMomentum) {
            return Mathf.RoundToInt(damagePerMomentum * excessiveMomentum);
        }

        public override void OnServerTakeDamage(int oldHealth, int newHealth) {
           // int healthReceived = newHealth - oldHealth;
        }

        [ServerCallback]
        public override void OnReceiveExcessiveMomentum(float excessiveMomentum) {
            Debug.Log($"Excessive Momentum: {excessiveMomentum}");
            int numberItemDrop = (Mathf.FloorToInt(excessiveMomentum / dropOneItemMomentumThreshold));
            this.SendEvent<OnSpaceshipRequestDropItems>(new OnSpaceshipRequestDropItems() {
                NumberItemRequest = numberItemDrop,
                SpaceshipIdentity = netIdentity
            });
        }
    }
}

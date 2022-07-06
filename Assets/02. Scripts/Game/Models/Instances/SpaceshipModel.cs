using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
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
        public NetworkIdentity identity;
        public bool hasAuthority;
    }

    public struct OnSpaceshipRequestDropItems {
        public NetworkIdentity SpaceshipIdentity;
        public int NumberItemRequest;

    }

    public struct OnClientSpaceshipHealthChange
    {
        public int NewHealth;
        public int MaxHealth;
        public NetworkIdentity Identity;
    }

    public struct OnPlayerDie {
        public NetworkIdentity SpaceshipIdentity;
    }

    public struct OnClientSpaceshipHooked {
        public NetworkIdentity identity;
        public bool hasAuthority;
    }

    public struct OnClientSpaceshipUnHooked
    {
        public NetworkIdentity identity;
        public bool hasAuthority;
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

        public override void OnClientHooked() {
       
            this.SendEvent<OnClientSpaceshipHooked>(new OnClientSpaceshipHooked() {
                hasAuthority = hasAuthority,
                identity = netIdentity
            });
        
           
        }

        public override void OnClientFreed()
        {
            this.SendEvent<OnClientSpaceshipUnHooked>(new OnClientSpaceshipUnHooked() {
                hasAuthority = hasAuthority,
                identity = netIdentity
            });
            
        }
        public int EscapeNeedCount { get; } = 10;
        public float EscapeLossTime { get; } = 0.15f;
        public float MaxMaxSpeed { get; } = 100;

        private float initialMaxSpeed;
        private float startAcceleration;

        [field: SyncVar(hook = nameof(ClientOnEscapeCounterChanged))]
        public int EscapeCounter { get; private set; }

        private float escapeLossTimer = 0f;

        [SerializeField] private float minimumAcceleration = 20;

        protected virtual void Update()
        {
            if (isServer) {
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

        [field: SerializeField] public float DieDizzyTime { get; protected set; } = 5f;

        [field: SerializeField] public float RespawnInvincibleTime { get; protected set; } = 5f;
        public void ServerUpdateMass() {
            rigidbody.mass = GetTotalMass();
            //Acceleration = Mathf.Max(minimumAcceleration, InitialAcceleration - GetTotalMass() * AccelerationDecreasePerMass);
        }

        [field: SyncVar,SerializeField]
        public float InitialAcceleration { get; private set; } = 20;

        [field: SyncVar, SerializeField]
        public override float SelfMass { get;  set; } = 1;
        
        

        [field: SyncVar, SerializeField]
        public float AccelerationDecreasePerMass { get; private set; } = 2;

        public void AddSpeedAndAcceleration(float percentage) {
            MaxSpeed =Mathf.Clamp(MaxSpeed + initialMaxSpeed * percentage, 5, MaxMaxSpeed);
            InitialAcceleration = Mathf.Clamp(InitialAcceleration + startAcceleration * percentage, minimumAcceleration,
                200);
            GetTotalMass();
        }

        public void AddMaximumHealth(float percentage) {
            MaxHealth = Mathf.RoundToInt(MaxHealth + 100 * percentage);
            CurrentHealth = Mathf.RoundToInt(CurrentHealth * (1 + percentage));
        }

        public float BackpackMass {
            get {
                float mass = 0;
                
                if (TryGetComponent<IPlayerInventorySystem>(out IPlayerInventorySystem inventory)) {
                    List<BackpackSlot> slots = inventory.BackpackItems;
                    foreach (BackpackSlot slot in slots) {
                        foreach (GameObject stackedObject in slot.StackedObjects) {
                            if (stackedObject) {
                                mass += stackedObject.GetComponent<IHookable>().SelfMass;
                            }
                         
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
            float totalMass = 0;
            //HookState
            if (HookState == HookState.Hooked) {  //hooked by somebody -> hooked.GetTotalMass() -> hooked.TotalMass()...
                //if hooked by somebody, that hooker must be another spaceship

                totalMass = (HookedByIdentity.GetComponent<IHaveMomentumViewController>()).Model.GetTotalMass();
            }
            else {
                if (hookSystem.IsHooking && hookSystem.HookedNetworkIdentity)
                {//return SelfMass + backpack + ...
                    IHookable hookingModel =  hookSystem.HookedNetworkIdentity.GetComponent<IHookable>();
                    
                   if (hookingModel is ISpaceshipConfigurationModel) {
                       totalMass= GetConnectedObjectSoleMass() + SelfMass + BackpackMass;
                   }else {
                       float hookingModelSelfMass  = hookingModel.SelfMass * hookingModel.AdditionalMassWhenHookedMultiplier;
                      
                        totalMass = SelfMass + BackpackMass + hookingModelSelfMass;
                   }
                }
                else {
                    totalMass = SelfMass + BackpackMass;
                }
            }
            RefreshAcceleration(totalMass);
            return totalMass;
        }

        protected override void OnEnable() {
            
        }

        private void RefreshAcceleration(float totalMass) {
            Acceleration = Mathf.Max(minimumAcceleration, InitialAcceleration - totalMass * AccelerationDecreasePerMass);
            if (Math.Abs(Acceleration - minimumAcceleration) < 15) {
                this.SendEvent<OnServerSpaceshipOverweight>(new OnServerSpaceshipOverweight() {
                    Spaceship = gameObject,
                    Tolerance = 15,
                    SpaceshipModel = this,
                    MinimumAcceleration = minimumAcceleration
                });
            }
        }

        [ClientCallback]
        private void ClientOnEscapeCounterChanged(int oldNum, int newNum) {
            this.SendEvent<OnEscapeCounterChanged>(new OnEscapeCounterChanged() {
                newValue = newNum,
                hasAuthority = hasAuthority,
                identity = netIdentity
            });
        }
        #endregion

        #region Server
        protected float initialForce;
        public override void OnStartServer()
        {
            base.OnStartServer();
            Vector2 Center = this.transform.position;
            ServerUpdateMass();
            initialForce = ProperForce();
            this.gameObject.GetComponent<Rigidbody2D>().AddForce(initialForce * ProperDirect(Center), ForceMode2D.Impulse);
            initialMaxSpeed = MaxSpeed;
            startAcceleration = InitialAcceleration;

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

        public override void OnServerTakeDamage(int oldHealth, NetworkIdentity damageDealer, int newHealth) {
           // int healthReceived = newHealth - oldHealth;
           if (newHealth <= 0) {
               this.SendEvent<OnSpaceshipRequestDropItems>(new OnSpaceshipRequestDropItems() {
                   NumberItemRequest = 99999,
                   SpaceshipIdentity = netIdentity
               });

               this.SendEvent<OnPlayerDie>(new OnPlayerDie() {
                   SpaceshipIdentity = netIdentity
               });
           }
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

        public override void OnClientHealthChange(int oldHealth, int newHealth) {
            base.OnClientHealthChange(oldHealth, newHealth);
            if (newHealth != oldHealth) {
                this.SendEvent<OnClientSpaceshipHealthChange>(new OnClientSpaceshipHealthChange()
                {
                    NewHealth = newHealth,
                    MaxHealth = MaxHealth,
                    Identity = netIdentity
                });
            }
           
        }


        public override void OnClientMaxHealthChange(int oldMaxHealth, int newMaxHealth) {
            base.OnClientMaxHealthChange(oldMaxHealth, newMaxHealth);
            this.SendEvent<OnClientSpaceshipHealthChange>(new OnClientSpaceshipHealthChange()
            {
                NewHealth = CurrentHealth,
                MaxHealth = newMaxHealth,
                Identity = netIdentity
            });
        }
    }

    public struct OnServerSpaceshipOverweight {
        public GameObject Spaceship;
        public float Tolerance;
        public ISpaceshipConfigurationModel SpaceshipModel;
        public float MinimumAcceleration;
    }
}

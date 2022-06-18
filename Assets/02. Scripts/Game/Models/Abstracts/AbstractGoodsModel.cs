using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientGoodsTransactionStatusChanged {
        public IGoods Goods;
        public bool IsFinished;
    }

    public struct ClientOnBuyItemInitialized {
        public GameObject item;
    }

    public struct OnServerTrySellItem {
        public IGoods RequestingGoods;
        public IGoods DemandedByPlanet;
        public NetworkIdentity HookedByIdentity;
        public GameObject RequestingGoodsGameObject;
    }

    public struct OnServerTryBuyItem
    {
        public IGoods RequestingGoods;
        public NetworkIdentity HookedByIdentity;
        public GameObject RequestingGoodsGameObject;
    }


    public abstract class AbstractGoodsModel : AbstractBasicEntityModel, IGoods, IAffectedByGravity,
    ICanSendQuery{

        [field: SyncVar, SerializeField]
        public override float SelfMass { get; protected set; }

        [field: SyncVar, SerializeField]
        public override string Name { get; set; } = "Goods";

        [field: SerializeField] public int BasicSellPrice { get; set; }
        [field: SerializeField] public int BasicBuyPrice { get; set; }
        [field: SerializeField] public GoodsRarity GoodRarity { get; set; }

        [field: SyncVar(hook = nameof(OnTransactionStatusChanged))]
        public bool TransactionFinished { get; set; } = true;

        [field: SerializeField] public bool DestroyedBySun { get; set; } = true;


        [field: SyncVar] public int RealPrice { get; set; }

        [field: SyncVar(hook = nameof(ClientOnSellStatusChanged)), SerializeField] 
        public bool IsSell { get; set; } = true;

        protected Trigger2DCheck triggerCheck;

        protected override void Awake() {
            base.Awake();
            triggerCheck = GetComponent<Trigger2DCheck>();
        }


        [ServerCallback]
        public void ServerAddGravityForce(float force, Vector2 position, float range) {
            if (TransactionFinished && !HookedByIdentity) {
                //Debug.Log("Affected");
                GetComponent<Rigidbody2D>().AddExplosionForce(force, position, range);
            }

        }

        [ServerCallback]
        protected override bool ServerCheckCanHook(NetworkIdentity hookedBy) {
            if (TransactionFinished) {
                return true;
            }
            else {
                if (!IsSell) {
                    return false;
                }

                //check money
                int money = this.SendQuery<int>(new ServerGetPlayerMoneyQuery(hookedBy));
                if (money - RealPrice < 0) {
                    this.SendEvent<OnServerPlayerMoneyNotEnough>(new OnServerPlayerMoneyNotEnough() {
                     PlayerIdentity = hookedBy
                    });
                    return false;
                }
                else {
                    this.SendEvent<OnServerTryBuyItem>(new OnServerTryBuyItem() {
                        HookedByIdentity = hookedBy,
                        RequestingGoods = this,
                        RequestingGoodsGameObject = gameObject
                    });
                    OnServerItemBought(hookedBy);
                    return true;
                }
            }
        }

       

        //deal with trading
        [ServerCallback]
        protected override void OnServerBeforeUnHooked() {
            base.OnServerBeforeUnHooked();
            if (triggerCheck) {
                if (triggerCheck.Triggered) {
                    
                    foreach (Collider2D collider in triggerCheck.Colliders) {
                        //is a goods
                        if ( collider.TryGetComponent<PlanetBuyItemDetectTrigger>(out PlanetBuyItemDetectTrigger detectTrigger)) {
                            IGoods good = detectTrigger.GetGoods();
                            if (good == null) return;
                            
                            //is the same type? (same type of goods?)
                            if (good.Name== Name) {
                                //is the good actually demanding by a planet?
                                if (!good.TransactionFinished && !good.IsSell) {
                                    //satisfy the condition, now sell it
                                    //however, need to check money first
                                    OnServerItemSold(HookedByIdentity);
                                    this.SendEvent<OnServerTrySellItem>(new OnServerTrySellItem() {
                                        DemandedByPlanet = good,
                                        RequestingGoods = this,
                                        HookedByIdentity = HookedByIdentity,
                                        RequestingGoodsGameObject = gameObject
                                    });
                                    
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        [ServerCallback]
        protected virtual void OnServerItemBought(NetworkIdentity buyer) {

        }

        [ServerCallback]
        protected virtual void OnServerItemSold(NetworkIdentity seller) {

        }

        public Vector2 StartDirection { get; }


        [field:SerializeField]
        public float InitialForceMultiplier { get; protected set; }
        [field: SerializeField] public bool AffectedByGravity { get; set; } = true;

        [ClientCallback]
        private void OnTransactionStatusChanged(bool oldStatus, bool newStatus) {
            
                this.SendEvent<OnClientGoodsTransactionStatusChanged>(new OnClientGoodsTransactionStatusChanged() {
                    Goods = this,
                    IsFinished = newStatus
                });
            

        }

        [ClientCallback]
        private void ClientOnSellStatusChanged(bool oldStatus, bool newStatus) {
            if (!newStatus) {
                this.SendEvent<ClientOnBuyItemInitialized>(new ClientOnBuyItemInitialized() {
                    item = gameObject
                });
            }
        }
    }
}

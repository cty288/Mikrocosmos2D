using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientGoodsTransactionFinished {
       
        public IGoods Goods;
    }
    public abstract class AbstractGoodsModel : AbstractBasicEntityModel, IGoods, IAffectedByGravity {
        [field:  SerializeField]
        public int BasicSellPrice { get; set; }
        [field: SerializeField]
        public int BasicBuyPrice { get; set; }
        [field: SerializeField]
        public GoodsRarity GoodRarity { get; set; }

        [field: SyncVar(hook = nameof(OnTransactionStatusChanged))] 
        public bool TransactionFinished { get; set; } = true;

        [field: SyncVar]
        public int RealPrice { get; set; }

        [field: SyncVar]
        public bool IsSell { get; set; } = true;

        [ServerCallback] 
        public void ServerAddGravityForce(float force, Vector2 position, float range)
        {
            if (TransactionFinished) {
                //Debug.Log("Affected");
                GetComponent<Rigidbody2D>().AddExplosionForce(force, position, range);
            }
           
        }

        protected override void OnServerHooked() {
            base.OnServerHooked();
            if (IsSell) {
                TransactionFinished = true;
            }
        }

        public Vector2 StartDirection { get; }
        public float InitialForceMultiplier { get; }

        [ClientCallback]
        private void OnTransactionStatusChanged(bool oldStatus, bool newStatus) {
            if (newStatus) {
                this.SendEvent<OnClientGoodsTransactionFinished>(new OnClientGoodsTransactionFinished() {
                    Goods = this
                });
            }
            
        }
    }
}

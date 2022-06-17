using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ISunFlowerModel : IModel, ICanBuyPackage {

    }
    public class SunFlowerModel : NetworkedModel, ISunFlowerModel {
        protected Rigidbody2D bindedRigidbody;

        private void Awake() {
            bindedRigidbody = GetComponent<Rigidbody2D>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            buyItemList.AddRange(initialBuyItemList);
        }


        public List<GoodsConfigure> GetBuyItemsWithRarity(GoodsRarity rarity) {
            List<GoodsConfigure> ret = new List<GoodsConfigure>();
            foreach (GoodsConfigure configure in buyItemList) {
                if (configure.Good.GoodRarity == rarity)
                {
                    ret.Add(configure);
                }
            }

            return ret;
        }

        public SyncList<GoodsConfigure> BuyItemList {
            get {
                return buyItemList;
            }
        }


        [SerializeField] private List<GoodsConfigure> initialBuyItemList = new List<GoodsConfigure>();
        
        protected readonly SyncList<GoodsConfigure> buyItemList = new SyncList<GoodsConfigure>();
        
       
    }
}


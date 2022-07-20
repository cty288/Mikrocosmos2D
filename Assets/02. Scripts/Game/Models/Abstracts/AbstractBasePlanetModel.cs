using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IPlanetModel : IHaveGravity, ICanBuyPackage, ICanSellPackage {
         PlanetTypeEnum PlanetType { get; }
        
      
    }



    public abstract partial class AbstractBasePlanetModel : NetworkedModel, IPlanetModel, ICanGetModel
    {
        protected Rigidbody2D bindedRigidbody;
        [field: SerializeField]
        public LayerMask AffectedLayerMasks { get; set; }


        private void Awake() {
            bindedRigidbody = GetComponent<Rigidbody2D>();
            
        }

        public override void OnStartServer() {
            base.OnStartServer();
            buyItemList.AddRange(initialBuyItemList);
            sellItemList.AddRange(initialSellItemList);
        }

        [field: SerializeField] public MoveMode MoveMode { get; set; } = MoveMode.ByTransform;
        float IHaveMomentum.MaxSpeed { get; }
        float IHaveMomentum.Acceleration { get; }

        [field: SerializeField, SyncVar]
        public float SelfMass { get;  set; }
        public virtual float GetTotalMass() {
            return SelfMass;
        }

        public virtual void AddSpeedAndAcceleration(float percentage) {
            
        }

        public virtual float GetMomentum() {
            return 0.5f * GetTotalMass() * bindedRigidbody.velocity.sqrMagnitude;
        }

        [field: SerializeField, SyncVar]
        public float GravityFieldRange { get; protected set; }
        [field: SerializeField, SyncVar]
        public float G { get; protected set; }


        public SyncList<GoodsConfigure> BuyItemList {
            get {
                return buyItemList;
            }
        }


        public SyncList<GoodsConfigure> SellItemList {
            get {
                return sellItemList;
            }
        }

        [SerializeField] private List<GoodsConfigure> initialBuyItemList = new List<GoodsConfigure>();
        [SerializeField] private List<GoodsConfigure> initialSellItemList = new List<GoodsConfigure>();

        protected readonly  SyncList<GoodsConfigure> buyItemList = new SyncList<GoodsConfigure>();
        protected readonly  SyncList<GoodsConfigure> sellItemList = new SyncList<GoodsConfigure>();


        [field: SerializeField]
        public PlanetTypeEnum PlanetType { get;  set; }

        public List<GoodsConfigure> GetBuyItemsWithRarity(GoodsRarity rarity) {
            List<GoodsConfigure> ret = new List<GoodsConfigure>();
            foreach (GoodsConfigure configure in buyItemList) {
                if (configure.Good.GoodRarity == rarity) {
                    ret.Add(configure);
                }
            }
            
            return ret;
        }


        
        public List<GoodsConfigure> GetSellItemsWithRarity(GoodsRarity rarity) {
            
            List<GoodsConfigure> ret = new List<GoodsConfigure>();
            foreach (GoodsConfigure configure in sellItemList)
            {
                if (configure.Good.GoodRarity == rarity)
                {
                    ret.Add(configure);
                }
            }

            return ret;
        }

        [field: SerializeField]
        public string Name { get; set; }
    }

    
}

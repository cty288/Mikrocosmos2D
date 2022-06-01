using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Mikrocosmos
{
    public abstract class AbstractGoodsViewController : AbstractCanCreateShadeEntity, IGoodsViewController, ICanBeMaskedViewController {

        private Collider2D collider;
        private ShadowCaster2D shadeCaster;

        [SerializeField] private Material buyMaterial;
        [SerializeField] private Material sellMaterial;
        [SerializeField] private Material visionMaterial;

        [SerializeField] protected Material defaultSpriteMaterial;
        protected Material visionEntityMaterial;

        private IGlobalTradingSystem globalTradingSystem;

        [SerializeField]
        protected SpriteRenderer[] visionAffectedSprites;
        public override void OnStartClient()
        {
            base.OnStartClient();
            this.GetComponent<SpriteRenderer>().enabled = false;
            this.GetSystem<ITimeSystem>().AddDelayTask(0.1f, () => {
                if (this) {
                    this.GetComponent<SpriteRenderer>().enabled = true;
                    ClientUpdateCanBeMasked();
                }
               
            });

            this.RegisterEvent<ClientOnBuyItemInitialized>(OnBuyItemInitialized)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientGoodsTransactionFinished>(OnClientBuyGoods)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
         
            ClientUpdateCanBeMasked();
        
        }

        private void OnClientBuyGoods(OnClientGoodsTransactionFinished e) {
            if (e.Goods == GoodsModel) {
                //visionEntityMaterial = visionMaterial;
                ClientUpdateCanBeMasked();
                GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
        }

        public override void OnStartServer() {
            base.OnStartServer();
            globalTradingSystem = this.GetSystem<IGlobalTradingSystem>();
            this.RegisterEvent<OnServerGoodsCraftSuccess>(OnCraftItemSuccess)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        [ServerCallback]
        private void OnCraftItemSuccess(OnServerGoodsCraftSuccess e) {
            if (e.Item1 == GoodsModel || e.Item2 == GoodsModel) {
                Model.UnHook();
                NetworkServer.Destroy(gameObject);
            }
        }


        protected override void Awake() {
            base.Awake();
            GoodsModel = GetComponent<IGoods>();
            collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
            shadeCaster = GetComponent<ShadowCaster2D>();
            visionEntityMaterial = visionMaterial;
        }


        private void Start() {

            if (shadeCaster) {
                shadeCaster.enabled = false;
                this.GetSystem<ITimeSystem>().AddDelayTask(0.1f, () => {
                    if (this)
                    {
                        if (Model.HookState != HookState.Hooked)
                        {
                            shadeCaster.enabled = IsMaskable;
                        }
                    }

                });
            }
           
        }

        protected override void OnCollisionEnter2D(Collision2D collision) {
            base.OnCollisionEnter2D(collision);
            if (isServer) {
               
                if (collision.collider.TryGetComponent<IGoods>(out IGoods goods)) {
                    
                    if (Model.HookedByIdentity && goods.HookedByIdentity) {

                        if (gameObject.GetHashCode() > collision.collider.gameObject.GetHashCode()) {

                            float playerRelativeVelocity = (Model.HookedByIdentity.GetComponent<Rigidbody2D>().velocity -
                                                           goods.HookedByIdentity.GetComponent<Rigidbody2D>().velocity).magnitude;
                           
                            
                            if (playerRelativeVelocity >=
                                globalTradingSystem.MinimumCompositeSpeedForCraftingCompounds) {
                                Debug.Log($"Relative Velocity: {playerRelativeVelocity}");
                                globalTradingSystem.ServerRequestCraftGoods(GoodsModel, goods, collision.GetContact(0).point);
                            }
                        }
                    }


                }
            }
            
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (isServer) {
                if (!GoodsModel.TransactionFinished && FollowingPoint) {
                    transform.position = FollowingPoint.position;
                }
            }
         
        }

        protected override void Update() {
            base.Update();
            if (GoodsModel.TransactionFinished) {
                collider.isTrigger = false;
                if (shadeCaster) {
                    shadeCaster.castsShadows = true;
                }
                
            }
            else {
                if (shadeCaster) {
                    shadeCaster.castsShadows = false;
                }
                
                collider.isTrigger = true;
                rigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        
        

        //deal with sell
        


        #region Vision
        [field: SyncVar(hook = nameof(OnCanBeMaskedChanged)), SerializeField]
        public bool CanBeMasked { get; protected set; } = true;

        public void ServerTurnOn()
        {
            CanBeMasked = true;
        }

        public void ServerTurnOff()
        {
            CanBeMasked = false;
        }



        private void OnCanBeMaskedChanged(bool oldValue, bool newValue)
        {
            ClientUpdateCanBeMasked();
        }


      

        private void OnBuyItemInitialized(ClientOnBuyItemInitialized e) {
            if (e.item == gameObject) {
                this.GetComponent<SpriteRenderer>().enabled = true;
                ClientUpdateCanBeMasked();
            }
            
        }


        [ClientCallback]
        protected virtual void ClientUpdateCanBeMasked() {
            Material mat;
            if (!CanBeMasked) {
                mat = Material.Instantiate(defaultSpriteMaterial);
            }
            else
            {
                
                if (GoodsModel.IsSell) {
                    if (GoodsModel.TransactionFinished) {
                        visionEntityMaterial = visionMaterial;
                    }
                    else {
                        visionEntityMaterial = sellMaterial;
                    }
                    mat = Material.Instantiate(visionEntityMaterial);
                }
                else {
                    mat = Material.Instantiate(buyMaterial);
                    
                }
               
            }

            foreach (SpriteRenderer sprite in visionAffectedSprites)
            {
                sprite.material = mat;
            }
        }


        #endregion



        public Transform FollowingPoint { get; set; }
        public IGoods GoodsModel { get; private set; }
    }
}

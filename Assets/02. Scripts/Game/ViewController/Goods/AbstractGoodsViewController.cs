using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using MikroFramework.TimeSystem;
using Mirror;
using Polyglot;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public static class DescriptionLayoutFinder {

        public static GameObject cashedLayout = null;
        public static GameObject GetLayout() {
            if (cashedLayout) {
                return cashedLayout;
            }
            cashedLayout = GameObject.FindGameObjectWithTag("DescriptionLayout");
            return cashedLayout;
        }
    }
    public abstract class AbstractGoodsViewController : AbstractCanCreateShadeEntity, IGoodsViewController, ICanBeMaskedViewController {

        private ResLoader resLoader;
        private ShadowCaster2D shadeCaster;

        [SerializeField] private Material buyMaterial;
        [SerializeField] private Material sellMaterial;
        [SerializeField] private Material visionMaterial;

        [SerializeField] protected Material defaultSpriteMaterial;
        protected Material visionEntityMaterial;
        

        private IGlobalTradingSystem globalTradingSystem;

        [SerializeField]
        protected SpriteRenderer[] visionAffectedSprites;

        [SerializeField]
        protected SpriteRenderer[] visionAffectedSpritesOnMap;


        public override void OnStartClient()
        {
            base.OnStartClient();
            this.GetComponent<SpriteRenderer>().enabled = false;
            ResLoader.Create((loader => resLoader = loader));
            this.GetSystem<ITimeSystem>().AddDelayTask(0.1f, () => {
                if (this) {
                    this.GetComponent<SpriteRenderer>().enabled = true;
                    ClientUpdateCanBeMasked();
                }
               
            });

            this.RegisterEvent<ClientOnBuyItemInitialized>(OnBuyItemInitialized)
                .UnRegisterWhenGameObjectDestroyed(gameObject, true);

            this.RegisterEvent<OnClientGoodsTransactionStatusChanged>(OnClientBuyGoods)
                .UnRegisterWhenGameObjectDestroyed(gameObject, true);
         
            ClientUpdateCanBeMasked();
        
        }

        private void OnDestroy() {
            if (resLoader != null) {
                resLoader.ReleaseAllAssets();
            }
        }

        private void OnClientBuyGoods(OnClientGoodsTransactionStatusChanged e) {
            if (e.Goods == GoodsModel && e.IsFinished) {
                //visionEntityMaterial = visionMaterial;
                ClientUpdateCanBeMasked();
                GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
        }

        public override void OnStartServer() {
            base.OnStartServer();
            globalTradingSystem = this.GetSystem<IGlobalTradingSystem>();
            this.RegisterEvent<OnServerGoodsCraftSuccess>(OnCraftItemSuccess)
                .UnRegisterWhenGameObjectDestroyed(gameObject, true);
        }

        [ServerCallback]
        private void OnCraftItemSuccess(OnServerGoodsCraftSuccess e) {
            if (e.Item1 == GoodsModel || e.Item2 == GoodsModel) {
                Model.UnHook(false);
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

        private GameObject absorbSpaceship;
        
        private bool waitingToCheckAbsorbing = false;

        [ServerCallback]
        public bool TryAbsorb(IPlayerInventorySystem invneInventorySystem, GameObject absorbedTarget) {
            if (isServer) {
                if (this.GetSystem<IGameProgressSystem>().GameState != GameState.InGame) {
                    return false;
                }

                if (Model.Frozen) {
                    return false;
                }
                
                if (GoodsModel.AbsorbedToBackpack && !waitingToCheckAbsorbing && Model.HookState == HookState.Freed &&
                    GoodsModel.TransactionFinished && !GoodsModel.IsAbsorbing) {
                   
                  
                    if (invneInventorySystem.ServerCheckCanAddToBackpack(GoodsModel, out var slot, out int index)) {
                        waitingToCheckAbsorbing = true;
                        
                        this.GetSystem<ITimeSystem>().AddDelayTask(0.5f, () => {
                            waitingToCheckAbsorbing = false;
                            if (this && !GoodsModel.IsAbsorbing) {
                                absorbSpaceship = absorbedTarget;
                                if (Mathf.Abs(Vector2.Distance(transform.position,
                                        absorbSpaceship.transform.position)) <= 15) {
                                    GoodsModel.IsAbsorbing = true;
                                }
                            }
                        });
                        return true;
                    }
                }
            }

            return false;
        }

        [ClientCallback]
        protected override DescriptionItem GetDescription() {
            string prefabAssetName = "";
            if (GoodsModel.GoodRarity == GoodsRarity.RawResource) {
                prefabAssetName = "DescriptionPanel_Raw";
            }
            else {
                prefabAssetName = "DescriptionPanel_General";
            }

            DescriptionItem item = DescriptionFactory.Singleton.GetGoodsDescriptionItem(prefabAssetName,
                GoodsModel.GoodRarity,
                GoodsModel.Name, GetDescriptionText(), GetHintAssetName());

            if (item != null) {
                OnDescriptionGenerated(item);
            }

            return item;
        }

        [ClientCallback]
        protected virtual DescriptionItem OnDescriptionGenerated(DescriptionItem descriptionItem) {
            return descriptionItem;
        }

        [ClientCallback]
        protected virtual string GetDescriptionText() {
            return Localization.Get($"DESCRIPTION_{Model.Name}");
        }

        [ClientCallback]
        protected virtual string GetHintAssetName() {
            return "LeftClick";
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

                if (GoodsModel.IsAbsorbing && absorbSpaceship && GoodsModel.AbsorbedToBackpack) {
                    if (Model.HookState != HookState.Freed || Model.Frozen) {
                        GoodsModel.IsAbsorbing = false;
                        absorbSpaceship = null;
                        return;
                    }
                    if (Vector2.Distance(transform.position, absorbSpaceship.transform.position) > 15) {
                        GoodsModel.IsAbsorbing = false;
                        absorbSpaceship = null;
                        return;
                    }                    

                    rigidbody.MovePosition(Vector2.Lerp(transform.position, absorbSpaceship.transform.position,
                        5f * Time.fixedDeltaTime));

                    if (Vector2.Distance(transform.position, absorbSpaceship.transform.position) <= 5) {
                        GoodsModel.IsAbsorbing = false;
                        if (absorbSpaceship.TryGetComponent<IPlayerInventorySystem>(out var playerInventorySystem))
                        {
                            playerInventorySystem.ServerAddToBackpack(GoodsModel.Name, gameObject, (o)=>{});
                        }
                        absorbSpaceship = null;
                    } 
                    
                }
            }

            
         
        }

        protected virtual void OnEnable() {
            
        }
        
        protected override void Update() {
            base.Update();
            if (GoodsModel.TransactionFinished) {
                collider.isTrigger = GoodsModel.IsAbsorbing;
                if (shadeCaster) {
                    shadeCaster.castsShadows = true;
                }
                //rigidbody.simulated = tr
                rigidbody.bodyType = RigidbodyType2D.Dynamic;
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

        [field: SerializeField]
        public bool AlsoMaskedOnMap { get; protected set; } = false;

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

            if (AlsoMaskedOnMap)
            {
                foreach (SpriteRenderer sprite in visionAffectedSpritesOnMap)
                {
                    sprite.material = mat;
                }
            }
        }


        #endregion



        public Transform FollowingPoint { get; set; }
        public IGoods GoodsModel { get; private set; }
    
    }
}

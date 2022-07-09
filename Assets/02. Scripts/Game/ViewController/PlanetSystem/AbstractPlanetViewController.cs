using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public interface IPlanetViewController : ICanBuyPackageViewController, ICanSellPackageViewController,
        IHaveGravityViewController {
         Dictionary<string, GameObject> ClientBuyBubbles { get; }
         Dictionary<string, GameObject> ClientSellBubbles { get; }
    }
    public abstract class AbstractPlanetViewController : AbstractNetworkedController<Mikrocosmos>, IPlanetViewController
    {


        public ICanBuyPackage BuyPackageModel { get; protected set; }
        public ICanSellPackage SellPackageModel { get; protected set; }
        public IHaveGravity GravityModel { get; protected set; }

       // private Transform sellItemSpawnPosition;
        //private Text sellPriceText;

        //private Transform buytemSpawnPosition;
      //  private Text buyPriceText;

        private Slider tradingSlider;
        private Text team1TradeProgressText;
        private Text team2TradeProgressText;

        private Transform buyBubbleLayout;
        private Transform sellBubbleLayout;

        [SerializeField] private GameObject buyBubblePrefab;
        [SerializeField] private GameObject sellBubblePrefab;
        
        private int team1ProgressTextInt = 50;

        [SerializeField, Range(0, 360)] private float initialProgress;


        [SerializeField] private float selfRotationSpeed;

        private Transform spriteTransform;

        private bool isSun;

        private bool canOvalRotate = true;
        private void Awake() {
            spriteTransform = transform.Find("Sprite");
            BuyPackageModel = GetComponent<ICanBuyPackage>();
            GravityModel = GetComponent<IHaveGravity>();
            SellPackageModel = GetComponent<ICanSellPackage>();

            rigidbody = GetComponent<Rigidbody2D>();
            
            progress = initialProgress;

            buyBubbleLayout = transform.Find("Canvas/BuyBubbleLayout");
            sellBubbleLayout = transform.Find("Canvas/SellBubbleLayout");

            isSun = gameObject.name == "Star";

            if (!isSun) {
                tradingSlider = transform.Find("Canvas/TradeSlider").GetComponent<Slider>();
                team1TradeProgressText = tradingSlider.transform.Find("Team1ProgressText").GetComponent<Text>();
                team2TradeProgressText = tradingSlider.transform.Find("Team2ProgressText").GetComponent<Text>();
            }
            



            this.RegisterEvent<OnClientPlanetAffinityWithTeam1Changed>(OnClientPlanetAffinityWithTeam1Changed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnMissionStart>(OnMissionStart).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnMissionStop>(OnMissionStop).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnMissionStop(OnMissionStop obj) {
            canOvalRotate = true;
        }

        private void OnMissionStart(OnMissionStart obj) {
            canOvalRotate = false;
        }


        private void FixedUpdate()
        {

            if (isServer)
            {
                if (canOvalRotate && target) {
                    OvalRotate();
                }
                
                KeepUniversalG();
                SelfRotate();
            }

            if (isClient && !isSun) {
                team1TradeProgressText.text = $"{team1ProgressTextInt}%";
                team2TradeProgressText.text = $"{100 - team1ProgressTextInt}%";
            }
        }

        private void SelfRotate()
        {
            //rotate Z
            if (spriteTransform) {
                spriteTransform.Rotate(0, 0, selfRotationSpeed * Time.fixedDeltaTime);
            }
         
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnServerPlanetGenerateSellItem>(OnServerPlanetGenerateSellItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerPlanetGenerateBuyingItem>(OnServerPlanetGenerateBuyingItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerPlanetDestroySellItem>(OnServerPlanetDestroySellItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnServerPlanetDestroyBuyItem>(OnServerPlanetDestroyBuyItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            if (target) {
                distance = Vector3.Distance(target.transform.position, transform.position);
            } 
        }

        private void OnServerPlanetDestroyBuyItem(OnServerPlanetDestroyBuyItem e) {
            if (e.ParentPlanet == gameObject) {
                string name = e.Item.GetComponent<IGoods>().Name;
                RpcDestroyBuyBubble(name);
            }
        }

        private void OnServerPlanetDestroySellItem(OnServerPlanetDestroySellItem e) {
            if (e.ParentPlanet == gameObject) {
                string name = e.Item.GetComponent<IGoods>().Name;
                RpcDestroySellBubble(name);
            }
        }


        [ServerCallback]
        private void OnServerPlanetGenerateBuyingItem(OnServerPlanetGenerateBuyingItem e)
        {
            if (e.ParentPlanet == gameObject) {
                IGoods generatedGoods = e.GeneratedItem.GetComponent<IGoods>();
                string generatedName = generatedGoods.Name;
                string previousName = "";
                if (e.PreviousItem) {
                    previousName = e.PreviousItem.GetComponent<IGoods>().Name;
                }

                GameObject buyBubble = GenerateBuyBubble(e.Price, generatedName, previousName, e.MaxTime,
                    generatedGoods.GoodRarity == GoodsRarity.RawResource);
               
                if (buyBubble) {
                    buyBubble.GetComponent<PlanetBuyBubble>().ServerGoodsBuying = generatedGoods;

                    e.GeneratedItem.GetComponent<IGoodsViewController>().FollowingPoint =
                        buyBubble.transform.Find("BuyItemSpawnPos");
                    RpcGenerateBuyBubble(e.Price, generatedName, previousName, e.MaxTime, generatedGoods.GoodRarity == GoodsRarity.RawResource);
                }

                if (!e.CountTowardsGlobalIItemList) {
                    RpcGenerateBuyBubble(e.Price, generatedName, previousName, e.MaxTime,
                        generatedGoods.GoodRarity == GoodsRarity.RawResource);
                }


            }

        }

        [ServerCallback]
        private void OnServerPlanetGenerateSellItem(OnServerPlanetGenerateSellItem e)
        {
            if (e.ParentPlanet == gameObject) {
                IGoods generatedGoods = e.GeneratedItem.GetComponent<IGoods>();
                string generatedName = generatedGoods.Name;
                string previousName = "";
                if (e.PreviousItem) {
                    previousName = e.PreviousItem.GetComponent<IGoods>().Name;
                }
                
                GameObject sellBubble = GenerateSellBubble(e.Price, generatedName, previousName,
                    generatedGoods.GoodRarity == GoodsRarity.RawResource);
                if (sellBubble) {
                    sellBubble.GetComponent<PlanetSellBubble>().ServerGoodsSelling = generatedGoods;
                    sellBubble.GetComponent<PlanetSellBubble>().ServerGoodsObjectSelling = e.GeneratedItem;
                    e.GeneratedItem.GetComponent<IGoodsViewController>().FollowingPoint = sellBubble.transform.Find("SellItemSpawnPos");
                    RpcGenerateSellBubble(e.Price, generatedName, previousName, generatedGoods.GoodRarity == GoodsRarity.RawResource);
                }

                if (!e.CountTowardsGlobalIItemList) {
                    RpcGenerateSellBubble(e.Price, generatedName, previousName, generatedGoods.GoodRarity == GoodsRarity.RawResource);
                }
            }
        }

        private GameObject GenerateSellBubble(int price,string bubbleToGenerate,  string bubbleToDestroy, bool isRaw) {
            GameObject oldBubble = null;
            if (!String.IsNullOrEmpty(bubbleToDestroy)) {
                if (ClientSellBubbles.ContainsKey(bubbleToDestroy)) {
                    ClientSellBubbles.Remove(bubbleToDestroy, out oldBubble);
                    if (oldBubble && String.IsNullOrEmpty(bubbleToGenerate)) {
                        Destroy(oldBubble);
                    }
                }
            }
            if (bubbleToGenerate == null || ClientSellBubbles.ContainsKey(bubbleToGenerate)) {
                return null;
            }

            GameObject sellBubble = null;
            if (oldBubble) {
                sellBubble = oldBubble;
            }
            else {
                sellBubble = Instantiate(sellBubblePrefab, sellBubbleLayout);
                sellBubble.transform.SetAsLastSibling();
            }
            
            
            sellBubble.GetComponent<PlanetSellBubble>().SetPrice(price, isRaw);
           
            ClientSellBubbles.Add(bubbleToGenerate, sellBubble);
            return sellBubble;
        }

        private GameObject GenerateBuyBubble(int price, string bubbleToGenerate, string bubbleToDestroy, float maxTime, bool isRaw)
        {
            GameObject oldBubble = null;            
            Debug.Log($"Bubble To Destroy: {bubbleToDestroy}, Bubble To Generate: {bubbleToGenerate}");
            if (!String.IsNullOrEmpty(bubbleToDestroy))
            {
                if (ClientBuyBubbles.ContainsKey(bubbleToDestroy))
                {
                    ClientBuyBubbles.Remove(bubbleToDestroy, out oldBubble);
                    if (oldBubble && String.IsNullOrEmpty(bubbleToGenerate)) {
                        Destroy(oldBubble);
                    }
                }

            }

            if (ClientBuyBubbles.ContainsKey(bubbleToGenerate)) {
              return null;
            }

            GameObject buyBubble = null;
            if (oldBubble)
            {
                buyBubble = oldBubble;
            }
            else
            {
                buyBubble = Instantiate(buyBubblePrefab, buyBubbleLayout);
                buyBubble.transform.SetAsLastSibling();
            }
            
            PlanetBuyBubble bubbleScript = buyBubble.GetComponent<PlanetBuyBubble>();
           bubbleScript.UpdateInfo(price, maxTime, isRaw);

           
            ClientBuyBubbles.Add(bubbleToGenerate, buyBubble);
            return buyBubble;
        }

        [ClientRpc]
        private void RpcDestroySellBubble(string bubbleToDestroy) {
            if (!String.IsNullOrEmpty(bubbleToDestroy)) {
                if (ClientSellBubbles.ContainsKey(bubbleToDestroy))
                {
                    ClientSellBubbles.Remove(bubbleToDestroy, out GameObject oldBubble);
                    if (oldBubble) {
                        Destroy(oldBubble);
                    }
                }
            }
        }

        [ClientRpc]
        private void RpcDestroyBuyBubble(string bubbleToDestroy)
        {
            if (!String.IsNullOrEmpty(bubbleToDestroy)) {
                if (ClientBuyBubbles.ContainsKey(bubbleToDestroy)) {
                    
                    ClientBuyBubbles.Remove(bubbleToDestroy, out GameObject oldBubble);
                    if (oldBubble) {
                        Destroy(oldBubble);
                    }
                }

            }
        }

        [ClientRpc]
        private void RpcGenerateSellBubble(int price, string bubbleToGenerate, string bubbleToDestroy, bool isRaw) {
            if (isClientOnly) {
                GenerateSellBubble(price, bubbleToGenerate, bubbleToDestroy, isRaw);
            }
        }

        [ClientRpc]
        private void RpcGenerateBuyBubble(int price, string bubbleToGenerate, string bubbleToDestroy, float maxTime, bool isRaw)
        {
            if (isClientOnly) {
                GenerateBuyBubble(price, bubbleToGenerate, bubbleToDestroy, maxTime, isRaw);
            }
        }
        private void OnClientPlanetAffinityWithTeam1Changed(OnClientPlanetAffinityWithTeam1Changed e)
        {
            if (e.PlanetIdentity == netIdentity) {
                if (e.NewAffinity <= 0.2) {
                    team1TradeProgressText.enabled = false;
                    team2TradeProgressText.enabled = true;
                }else if (e.NewAffinity >= 0.8) {
                    team1TradeProgressText.enabled = true;
                    team2TradeProgressText.enabled = false;
                }
                else {
                    team1TradeProgressText.enabled = true;
                    team2TradeProgressText.enabled = true;
                }


                DOTween.To(() => tradingSlider.value, x => tradingSlider.value = x,
                    e.NewAffinity, 0.5f);

                DOTween.To(() => team1ProgressTextInt, x => team1ProgressTextInt = x,
                    Mathf.Clamp(Mathf.RoundToInt(e.NewAffinity * 100), 0, 100), 0.5f);
            }
        }

        #region Rotation
        public GameObject target;

        public float speed = 100;
        [SerializeField]
        float progress = 0;
      
        float distance = 0;
        public float x = 5;
        public float z = 7;

        private Rigidbody2D rigidbody;

        [SerializeField] private float momentumOffset = 100;
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider)
            {
                if (collision.collider.TryGetComponent<IDamagable>(out IDamagable model))
                {
                    if (model is IHookable hookable)
                    {
                        if (hookable.HookState == HookState.Hooked)
                        {
                            StartCoroutine(NonPhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>(),
                                collision.GetContact(0).point));
                            return;
                        }
                    }
                    //normal
                    StartCoroutine(PhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>(), collision.GetContact(0).point));
                    //StartCoroutine(NonPhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>()));
                }
            }
        }
        IEnumerator NonPhysicsForceCalculation(IDamagable targetModel, Rigidbody2D targetRigidbody, Vector2 contactPoint)
        {
            float waitTime = 0.02f;
            Vector2 offset = Vector2.zero;
            if (targetModel is ISpaceshipConfigurationModel)
            {
                targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = false;
                targetRigidbody.GetComponent<PlayerSpaceship>().RecoverCanControl(waitTime);
            }
            Vector2 speed1 = targetRigidbody.velocity;
            yield return new WaitForSeconds(waitTime);
            if (targetRigidbody)
            {
                Vector2 speed2 = targetRigidbody.velocity;

                Vector2 acceleration = (speed2 - speed1) / waitTime;
                
                if (targetModel != null)
                {
                    Vector2 force = acceleration * Mathf.Sqrt(targetModel.GetTotalMass());
                    if (targetModel is ISpaceshipConfigurationModel model)
                    {
                        force *= speed2.magnitude / model.MaxSpeed;
                    }
                    force = new Vector2(Mathf.Sign(force.x) * Mathf.Log(Mathf.Abs(force.x)), Mathf.Sign(force.y) * Mathf.Log(Mathf.Abs(force.y), 2));
                    force *= 2;
                    OnHitByObject(force.magnitude, contactPoint);
                    float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                    targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                    targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum), netIdentity);


                }

            }
           
        }


        IEnumerator PhysicsForceCalculation(IDamagable targetModel, Rigidbody2D targetRigidbody, Vector2 contactPoint)
        {
            float waitTime = 0.02f;
            Vector2 offset = Vector2.zero;
            if (targetModel is ISpaceshipConfigurationModel)
            {
                targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = false;
                targetRigidbody.GetComponent<PlayerSpaceship>().RecoverCanControl(waitTime);
            }
            Vector2 speed1 = targetRigidbody.velocity;
            yield return new WaitForSeconds(waitTime);
            if (targetRigidbody) {
                Vector2 speed2 = targetRigidbody.velocity;

                Vector2 acceleration = (speed2 - speed1) / waitTime;
             // .Log($"Speed1: {speed1}, Speed 2: {speed2}, Acceleration: {acceleration}. " +
                //          $"Fixed Dealta Time : {Time.fixedDeltaTime}");
                if (targetModel != null)
                {
                    Vector2 force = acceleration * Mathf.Sqrt(targetModel.GetTotalMass());
                    if (targetModel is ISpaceshipConfigurationModel model)
                    {
                        force *= speed2.magnitude / model.MaxSpeed;
                    }
                    force = new Vector2(Mathf.Sign(force.x) * Mathf.Log(Mathf.Abs(force.x)), Mathf.Sign(force.y) * Mathf.Log(Mathf.Abs(force.y), 2));
                    force *= 2;
                    OnHitByObject(force.magnitude, contactPoint);
                    float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                    targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                    targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum), netIdentity);


                }
            }
            
        }


        protected virtual void OnHitByObject(float force, Vector2 contactPoint) {

        }
        void OvalRotate()
        {

            progress += (Time.fixedDeltaTime * speed);
            progress %= 360;
            Vector3 p = new Vector3(x * Mathf.Cos(progress * Mathf.Deg2Rad), z * Mathf.Sin(progress * Mathf.Deg2Rad) * distance, 0);
            if (GravityModel.MoveMode == MoveMode.ByPhysics)
            {
                rigidbody.MovePosition(target.transform.position + p);
            }
            else
            {
                transform.position = target.transform.position + p;
            }

        }
        //start refactor


        [ServerCallback]
        void KeepUniversalG()
        {
            Vector2 Center = this.transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(Center, GravityModel.GravityFieldRange, GravityModel.AffectedLayerMasks);

            foreach (Collider2D obj in colliders)
            {

                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

                if (rb && obj.gameObject != gameObject)
                {
                    if (rb.TryGetComponent<IAffectedByGravity>(out IAffectedByGravity target))
                    {
                        if (target.AffectedByGravity) {
                            float explosionForce = -1 * UniversalG(GravityModel.GetTotalMass(), rb, transform.position, rb.transform.position) * Time.fixedDeltaTime;
                            target.ServerAddGravityForce(explosionForce, Center, GravityModel.GravityFieldRange);
                        }

                    }

                }

            }

        }

        private float UniversalG(float mass, Rigidbody2D target, Vector2 sourcePos, Vector2 targetPos) {
            float destMass = target.mass;
            return (mass * destMass /Distance(sourcePos, targetPos)) * GravityModel.G;

        }
        
        protected float Distance(Vector2 pos1, Vector2 pos2)
        {
            //Vector2 diff = (pos1 - pos2);
            float dist = Vector2.Distance(pos1, pos2);
            if (dist < 1)
                return 1;
            else return (dist);
        }
        //end 



        #endregion


        public Dictionary<string, GameObject> ClientBuyBubbles { get; protected set; } = new Dictionary<string, GameObject>();
         public Dictionary<string, GameObject> ClientSellBubbles { get; protected set; } = new Dictionary<string, GameObject>();
    }
}

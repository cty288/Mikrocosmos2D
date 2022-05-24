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

    }
    public abstract class AbstractPlanetViewController: AbstractNetworkedController<Mikrocosmos>, IPlanetViewController {
      

        public ICanBuyPackage BuyPackageModel { get; protected set; }
        public ICanSellPackage SellPackageModel { get; protected set; }
        public IHaveGravity GravityModel { get; protected set; }

        private Transform sellItemSpawnPosition;
        private Text sellPriceText;

        private Transform buytemSpawnPosition;
        private Text buyPriceText;

        private Slider tradingSlider;
        private Text team1TradeProgressText;
        private Text team2TradeProgressText;

        private int team1ProgressTextInt = 50;
       
        private void Awake() {
         
            BuyPackageModel = GetComponent<ICanBuyPackage>();
            GravityModel = GetComponent<IHaveGravity>();
            SellPackageModel = GetComponent<ICanSellPackage>();
            
            rigidbody = GetComponent<Rigidbody2D>();
            distance = Vector3.Distance(target.transform.position, transform.position);
            progress = Random.Range(0, 360000);

            sellItemSpawnPosition = transform.Find("SellBubbleBG/SellItemSpawnPos");
            buytemSpawnPosition = transform.Find("BuyBubbleBG/BuyItemSpawnPos");
            if (sellItemSpawnPosition) {
                sellPriceText = transform.Find("Canvas/SellPrice").GetComponent<Text>();
                buyPriceText = transform.Find("Canvas/BuyPrice").GetComponent<Text>();
                buyPriceText.gameObject.SetActive(false);

                tradingSlider = transform.Find("Canvas/TradeSlider").GetComponent<Slider>();
                team1TradeProgressText = tradingSlider.transform.Find("Team1ProgressText").GetComponent<Text>();
                team2TradeProgressText = tradingSlider.transform.Find("Team2ProgressText").GetComponent<Text>();
            }


            this.RegisterEvent<OnClientPlanetAffinityWithTeam1Changed>(OnClientPlanetAffinityWithTeam1Changed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        private void Update() {

            if (isServer) {
                OvalRotate();
                KeepUniversalG();
            }

            if (isClient && sellItemSpawnPosition) {
                team1TradeProgressText.text = $"{team1ProgressTextInt}%";
                team2TradeProgressText.text = $"{100 - team1ProgressTextInt}%";
            }
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnServerPlanetGenerateSellItem>(OnServerPlanetGenerateSellItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerPlanetGenerateBuyingItem>(OnServerPlanetGenerateBuyingItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
           
        }

       

        [ServerCallback]
        private void OnServerPlanetGenerateBuyingItem(OnServerPlanetGenerateBuyingItem e) {
            if (e.ParentPlanet == gameObject) {
                e.GeneratedItem.GetComponent<IGoodsViewController>().FollowingPoint = buytemSpawnPosition;
                RpcChangeBuyPrice(e.Price);
            }
          
        }

        [ServerCallback]
        private void OnServerPlanetGenerateSellItem(OnServerPlanetGenerateSellItem e) {
            if (e.ParentPlanet == gameObject) {
                
                Debug.Log("Sell Item generated");
                e.GeneratedItem.GetComponent<IGoodsViewController>().FollowingPoint = sellItemSpawnPosition;
                RpcChangeSellPrice(e.Price);
            }
        }

        [ClientRpc]
        private void RpcChangeSellPrice(int price) {
            sellPriceText.text = price.ToString();
        }

        [ClientRpc]
        private void RpcChangeBuyPrice(int price) {
            buyPriceText.gameObject.SetActive(true);
            buyPriceText.text = price.ToString();
        }
        private void OnClientPlanetAffinityWithTeam1Changed(OnClientPlanetAffinityWithTeam1Changed e) {
            if (e.PlanetIdentity == netIdentity) {
                DOTween.To(() => tradingSlider.value, x => tradingSlider.value = x,
                    e.NewAffinity, 0.5f);

                DOTween.To(() => team1ProgressTextInt, x => team1ProgressTextInt = x,
                    Mathf.Clamp(Mathf.RoundToInt(e.NewAffinity * 100), 0, 100), 0.5f);
            }
        }

        #region Rotation
        public GameObject target;

        public float speed = 100;
        float progress = 0;
        float distance = 0;
        public float x = 5;
        public float z = 7;

        private Rigidbody2D rigidbody;




        void OvalRotate()
        {

            progress += Time.deltaTime * speed;
            Vector3 p = new Vector3(x * Mathf.Cos(progress * Mathf.Deg2Rad), z * Mathf.Sin(progress * Mathf.Deg2Rad) * distance, 0);
            if (GravityModel.MoveMode == MoveMode.ByPhysics) {
                rigidbody.MovePosition(target.transform.position + p);
            }
            else {
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
                        float explosionForce = -1 * UniversalG(GravityModel, target, transform.position, rb.transform.position) * Time.deltaTime;
                        if (target.AffectedByGravity) {
                            target.ServerAddGravityForce(explosionForce, Center, GravityModel.GravityFieldRange);
                        }
                   
                    }

                }

            }

        }

        private float UniversalG(IHaveMomentum source, IHaveMomentum target, Vector2 sourcePos, Vector2 targetPos)
        {

            float sorceMass = source.GetTotalMass();
            float destMass = target.GetTotalMass();
            return (sorceMass * destMass / Distance(sourcePos, targetPos)) * GravityModel.G;

        }

        protected float Distance(Vector2 pos1, Vector2 pos2)
        {
            Vector2 diff = (pos1 - pos2);
            float dist = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (dist < 1)
                return 1;
            else return (dist);
        }
        //end 



        #endregion



    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class SunFlowerViewController : AbstractNetworkedController<Mikrocosmos>, ICanBuyPackageViewController {
        public ICanBuyPackage BuyPackageModel { get; }

        private Transform buyBubbleLayout;
        [SerializeField] private GameObject buyBubblePrefab;
        
        private Transform spriteTransform;

        private Rigidbody2D rigidbody;
        private void Awake()
        {
            spriteTransform = transform.Find("Sprite");
            rigidbody = GetComponent<Rigidbody2D>();
            buyBubbleLayout = transform.Find("Canvas/BuyBubbleLayout");
        }


        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            this.RegisterEvent<OnServerPlanetGenerateBuyingItem>(OnServerPlanetGenerateBuyingItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerPlanetDestroyBuyItem>(OnServerPlanetDestroyBuyItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnServerPlanetDestroyBuyItem(OnServerPlanetDestroyBuyItem e) {
            if (e.ParentPlanet == gameObject)
            {
                string name = e.Item.GetComponent<IGoods>().Name;
                RpcDestroyBuyBubble(name);
            }
        }


        [ServerCallback]
        private void OnServerPlanetGenerateBuyingItem(OnServerPlanetGenerateBuyingItem e)
        {
            if (e.ParentPlanet == gameObject)
            {
                string generatedName = e.GeneratedItem.GetComponent<IGoods>().Name;
                string previousName = "";
                if (e.PreviousItem)
                {
                    previousName = e.PreviousItem.GetComponent<IGoods>().Name;
                }

                GameObject buyBubble = GenerateBuyBubble(e.Price, generatedName, previousName, e.MaxTime);

                if (buyBubble)
                {
                    buyBubble.GetComponent<PlanetBuyBubble>().ServerGoodsBuying = e.GeneratedItem.GetComponent<IGoods>();

                    e.GeneratedItem.GetComponent<IGoodsViewController>().FollowingPoint =
                        buyBubble.transform.Find("BuyItemSpawnPos");
                    RpcGenerateBuyBubble(e.Price, generatedName, previousName, e.MaxTime);
                }

                if (!e.CountTowardsGlobalIItemList)
                {
                    RpcGenerateBuyBubble(e.Price, generatedName, previousName, e.MaxTime);
                }


            }

        }

       

        private GameObject GenerateBuyBubble(int price, string bubbleToGenerate, string bubbleToDestroy, float maxTime)
        {
            GameObject oldBubble = null;
            Debug.Log($"Bubble To Destroy: {bubbleToDestroy}, Bubble To Generate: {bubbleToGenerate}");
            if (!String.IsNullOrEmpty(bubbleToDestroy))
            {
                if (ClientBuyBubbles.ContainsKey(bubbleToDestroy))
                {
                    ClientBuyBubbles.Remove(bubbleToDestroy, out oldBubble);
                    if (oldBubble && String.IsNullOrEmpty(bubbleToGenerate))
                    {
                        Destroy(oldBubble);
                    }
                }

            }

            if (ClientBuyBubbles.ContainsKey(bubbleToGenerate))
            {
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
            bubbleScript.UpdateInfo(price, maxTime);


            ClientBuyBubbles.Add(bubbleToGenerate, buyBubble);
            return buyBubble;
        }


        [ClientRpc]
        private void RpcDestroyBuyBubble(string bubbleToDestroy)
        {
            if (!String.IsNullOrEmpty(bubbleToDestroy))
            {
                if (ClientBuyBubbles.ContainsKey(bubbleToDestroy))
                {

                    ClientBuyBubbles.Remove(bubbleToDestroy, out GameObject oldBubble);
                    if (oldBubble)
                    {
                        Destroy(oldBubble);
                    }
                }

            }
        }

        [ClientRpc]
        private void RpcGenerateBuyBubble(int price, string bubbleToGenerate, string bubbleToDestroy, float maxTime)
        {
            if (isClientOnly)
            {
                GenerateBuyBubble(price, bubbleToGenerate, bubbleToDestroy, maxTime);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider)
            {
                if (collision.collider.TryGetComponent<IDamagable>(out IDamagable model))
                {
                    if (model is IHookable hookable)
                    {
                        if (hookable.HookState == HookState.Hooked)
                        {
                            StartCoroutine(NonPhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>()));
                            return;
                        }
                    }
                    //normal
                    StartCoroutine(PhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>()));
                    //StartCoroutine(NonPhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>()));
                }
            }
        }
        IEnumerator NonPhysicsForceCalculation(IDamagable targetModel, Rigidbody2D targetRigidbody)
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
                    float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                    targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                    targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum));


                }

            }

        }


        IEnumerator PhysicsForceCalculation(IDamagable targetModel, Rigidbody2D targetRigidbody)
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
                    float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                    targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                    targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum));


                }
            }

        }
        public Dictionary<string, GameObject> ClientBuyBubbles { get; protected set; } = new Dictionary<string, GameObject>();
    }
}

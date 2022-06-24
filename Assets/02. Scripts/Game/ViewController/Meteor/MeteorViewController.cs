using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{

    public struct OnMeteorDestroyed {
        public GameObject Meteor;
    }
    //obj pool
    public class MeteorViewController : AbstractCanCreateShadeEntity, IDamagableViewController
    {
        private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject damageParticle;
        [SerializeField] private GameObject dieParticle;

       
        protected override void Awake()
        {
            base.Awake();
            DamagableModel = GetComponent<IDamagable>();
            spriteRenderer = GetComponent<SpriteRenderer>();
          
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnEntityTakeDamage>(OnEntityTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEntityTakeDamage(OnEntityTakeDamage e)
        {
            if (e.Entity == Model) {
                OnServerTakeDamage(e.OldHealth, e.NewHealth);
            }
        }


        [ServerCallback]
        public virtual void OnServerTakeDamage(int oldHealth, int newHealth) {
            RpcOnClientTakeDamage(oldHealth, newHealth);
            if (newHealth <= 0 && oldHealth > 0) {
                if (Model.HookedByIdentity) {
                    Model.UnHook();
                }

                GenerateRewards();

                if (GetComponent<PoolableNetworkedGameObject>()) {
                    this.GetSystem<IMeteorSystem>().ResetMeteor(gameObject);
                    NetworkedObjectPoolManager.Singleton.Recycle(gameObject);
                    NetworkServer.UnSpawn(gameObject);
                }else {
                    NetworkServer.Destroy(gameObject);
                }
               
            }
        }

        private void GenerateRewards() {
            List<GameObject> rewards = GetModel<IMeteorModel>().Rewards;
            //get a random reward
            int randomReward = Random.Range(0, rewards.Count);
            //spawn it
            GameObject reward = rewards[randomReward];

            if (reward.name == "DiamondEntity") {
                int totalMoney = Random.Range(5, GetModel<IMeteorModel>().MaxMoneyReward);
                //generate 3-8 diamonds, randomly assign money to them but the sum of their money should be equal to totalMoney
                List<int> moneyList = new List<int>();
                for (int i = 0; i < Random.Range(3, 8); i++) {
                    moneyList.Add(Random.Range(1, Mathf.Min(7, totalMoney)));
                }
                //get the sum of moneyList
                int sum = 0;
                foreach (int money in moneyList)
                {
                    sum += money;
                }
                //if sum is not equal to totalMoney, we need to make sure that the sum of moneyList is equal to totalMoney, but none of moneyList should be smaller than 1 or greater than 15
                if (sum != totalMoney)
                {
                    int diff = totalMoney - sum;
                    int index = 0;

                    while (diff != 0 && index < moneyList.Count)
                    {
                        if (diff > 0)
                        {
                            if (moneyList[index] < 15)
                            {
                                moneyList[index] += 1;
                                diff -= 1;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        else
                        {
                            if (moneyList[index] > 1)
                            {
                                moneyList[index] -= 1;
                                diff += 1;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                //spawn the diamonds
                for (int i = 0; i < moneyList.Count; i++) {
                    GameObject diamond = NetworkedObjectPoolManager.Singleton.Allocate(reward);
                    diamond.transform.position = transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
                    //randomly set diamond's rotation
                    diamond.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                    //set diamond's money
                    diamond.GetComponent<DiamondEntityViewController>().SetMoney(moneyList[i]);
                    //add some velocity to the diamond's rigidbody
                    diamond.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized
                        *15f, ForceMode2D.Impulse);
                    NetworkServer.Spawn(diamond);
                    Debug.Log("Diamond Spawned");
                }
            }else {
                //generate random count of rewards
                int count = Random.Range(1, 4);
                for (int i = 0; i < count; i++) {
                    GameObject rewardInstance = Instantiate(reward, transform.position,
                        Quaternion.Euler(0, 0, Random.Range(0, 360)));
                    
                    rewardInstance.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized
                                                                        * 15f, ForceMode2D.Impulse);
                    if (rewardInstance.TryGetComponent<IGoods>(out var goods)) {
                        goods.AbsorbedToBackpack = true;
                    }

                    
                    NetworkServer.Spawn(rewardInstance);
                }
            }
        }

        public override void OnStopClient() {
            base.OnStopClient();
           // if (DamagableModel.CurrentHealth <= 0) {
                GameObject.Instantiate(dieParticle, transform.position, Quaternion.identity);
          //  }
        }

        [ClientRpc]
        public virtual void RpcOnClientTakeDamage(int oldHealth, int newHealth) {
            if (newHealth < oldHealth) {
                int damage =Mathf.Abs(newHealth - oldHealth);
                spriteRenderer.DOBlendableColor(new Color(0.6f, 0.6f, 0.6f), 0.1f).SetLoops(2, LoopType.Yoyo);
                GameObject.Instantiate(damageParticle, transform.position, Quaternion.identity);
               
            }
              
        }


        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Border")) {
                this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => {
                    if (this) {
                        GetComponent<Collider2D>().isTrigger = false;
                    }
                
                });

            }
        }

        

        public IDamagable DamagableModel { get; protected set; }
    }
}

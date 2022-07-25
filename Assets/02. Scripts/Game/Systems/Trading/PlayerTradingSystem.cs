using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Mikrocosmos
{

    public struct OnClientMoneyChange
    {
        public int OldMoney;
        public int NewMoney;
    }

    public struct OnClientMoneyNotEnough {
        
    }

    public struct OnPlayerReceiveMoney {
        public NetworkIdentity Player;
        public int MoneyReceived;
    }
    public interface IPlayerTradingSystem : ISystem
    {
        public int Money { get; }

        public void SpendMoney(int count);

        public void ReceiveMoney(int count);
    }

    public class PlayerTradingSystem : AbstractNetworkedSystem, IPlayerTradingSystem {


        [SerializeField] private GameObject diamondPrefab;
        [field: SyncVar(hook = nameof(OnClientMoneyChange)), SerializeField]
        public int Money { get; set; } = 50;


        
        public void SpendMoney(int count) {
            if (Money >= count) {
                Money -= count;
            }
            else
            {
                TargetOnPlayerMoneyNotEnough();
            }
        }

        public void ReceiveMoney(int count) {
            Money += count;
            this.SendEvent<OnPlayerReceiveMoney>(new OnPlayerReceiveMoney() {
                Player = netIdentity,
                MoneyReceived = count
            });
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnServerPlayerMoneyNotEnough>(OnServerPlayerMoneyNotEnough)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnPlayerDie(OnPlayerDie e) {
            if (e.SpaceshipIdentity == netIdentity) {
                float dropPercentage = Mathf.Clamp(Money / 1000f, 0.1f, 0.3f);
                dropPercentage += Random.Range(-0.05f, 0.05f);
                int dropMoney = Mathf.RoundToInt(Money * dropPercentage);
                Money -= dropMoney;
                if (dropMoney > 0) {
                    SpawnMoney(dropMoney);
                }
            }
        }

        
        private void SpawnMoney(int dropMoney) {
            int totalMoney = dropMoney;
            //generate 3-8 diamonds, randomly assign money to them but the sum of their money should be equal to totalMoney
            List<int> moneyList = new List<int>();
            int spawnCount = Mathf.Min(dropMoney, Random.Range(3, 8));
            for (int i = 0; i < spawnCount; i++) {
                moneyList.Add(Random.Range(1, Mathf.Min(7, totalMoney) + 1));
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
                        if (moneyList[index] < (totalMoney / 2))
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
            for (int i = 0; i < moneyList.Count; i++)
            {
                GameObject diamond = NetworkedObjectPoolManager.Singleton.Allocate(diamondPrefab);
                diamond.transform.position = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
                //randomly set diamond's rotation
                diamond.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                //set diamond's money
                diamond.GetComponent<DiamondEntityViewController>().SetMoney(moneyList[i]);
                //add some velocity to the diamond's rigidbody
                diamond.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized
                    * 15f, ForceMode2D.Impulse);
                NetworkServer.Spawn(diamond);
                Debug.Log("Diamond Spawned");
            }
        }

        private void OnServerPlayerMoneyNotEnough(OnServerPlayerMoneyNotEnough e) {
            if (e.PlayerIdentity == netIdentity) {
                TargetOnPlayerMoneyNotEnough();
            }
        }


        [ClientCallback]
        private void OnClientMoneyChange(int oldMoney, int newMoney)
        {
            if (hasAuthority)
            {
                this.SendEvent<OnClientMoneyChange>(new OnClientMoneyChange()
                {
                    OldMoney = oldMoney,
                    NewMoney = newMoney
                });
                if (newMoney > oldMoney) {
                    this.GetSystem<IAudioSystem>().PlaySound("AddMoney", SoundType.Sound2D);
                }
            }
        }

        [TargetRpc]
        private void TargetOnPlayerMoneyNotEnough() {
            this.SendEvent<OnClientMoneyNotEnough>();
        }
    }
}

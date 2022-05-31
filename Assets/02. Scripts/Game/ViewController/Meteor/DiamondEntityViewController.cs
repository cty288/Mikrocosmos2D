using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class DiamondEntityViewController : BasicEntityViewController {
        private Trigger2DCheck triggerCheck;
        [SerializeField]
        private Transform targetPlayer;
        private int money; //5-30;
        private bool startDetect = false;

        public bool Attracted {
            get {
                return targetPlayer != null;
            }
        }
        public int Money {
            get => money;
            set => money = value;
        }

        public override void OnStartServer() {
            base.OnStartServer();
            triggerCheck = GetComponent<Trigger2DCheck>();
          
            targetPlayer = null;
            triggerCheck.Clear();
        }


        private void OnEnable() {
            startDetect = false;
            this.GetSystem<ITimeSystem>().AddDelayTask(0.4f, () => {
                startDetect = true;
            });
        }

        public void SetMoney(int money) {
            Money = money;
            //set the scale of this object according to money, with a minimum of 1, and a maximum of 4
            float scale = Mathf.Clamp(money / 5f, 1f, 4f);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        protected override void Update() {
            base.Update();
            if (isServer) {
                if (startDetect && triggerCheck.Triggered && !targetPlayer) {
                    OnTriggered();
                }

               
            }
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (targetPlayer)
            {
                rigidbody.MovePosition(Vector2.Lerp(transform.position, targetPlayer.position, Time.fixedDeltaTime * 5f));  
            }
        }

        private void OnTriggered() {
            List<Collider2D> players = triggerCheck.Colliders;
            //find the player that is closest to this object
            float closestDistance = float.MaxValue;
            int closestIndex = -1;
            for (int i = 0; i < players.Count; i++)
            {
                float distance = Vector2.Distance(players[i].transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }
            if (closestIndex != -1) {
                targetPlayer = players[closestIndex].transform;
                GetComponents<Collider2D>().ToList().ForEach(c => c.isTrigger = true);
            }
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (isServer) {
                if (targetPlayer) {
                    if (col.transform == targetPlayer) {
                        if (targetPlayer.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem playerTradingSystem)) {
                            playerTradingSystem.Money += Money;
                        }

                        NetworkedObjectPoolManager.Singleton.Recycle(gameObject);
                    }
                }
            }
        }
    }
}

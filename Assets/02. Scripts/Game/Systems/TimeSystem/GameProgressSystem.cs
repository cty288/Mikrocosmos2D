using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{

    public struct OnClientFinalCountdownTimerChange {
        public int Time;
    }
    public struct OnClientFinalCountdownTimerStart {
        
    }

    public struct OnClientFinalCountDownTimerEnds {
        public int WinTeam;
        public List<string> WinNames;
    }

    public struct OnTieTimerStart {

    }

    public interface IGameProgressSystem : ISystem {
        public BindableProperty<int> TotalTransactionTime { get; }
    }
    public class GameProgressSystem : AbstractNetworkedSystem, IGameProgressSystem {
        protected DateTime globalTimer;
        [Tooltip("In Minutes")]

        [SerializeField] protected float maximumGameTime = 15;
        [Tooltip("In Seconds")]
        [SerializeField, SyncVar(hook = nameof(ClientOnCountdownChange))] 
        protected int finalCountDown = 60;

        [SerializeField] protected int finalCountdownTransactionThresholdPerPlayer = 15;
        

        private bool finialCountDownStarted = false;
        public override void OnStartServer() {
            base.OnStartServer();
            Mikrocosmos.Interface.RegisterSystem<IGameProgressSystem>(this);

            this.RegisterEvent<OnServerTransactionFinished>(OnTransactionFinished)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            globalTimer = DateTime.Now;
        }
        
        private void Update() {
            if (isServer) {
                Debug.Log($"Seconds since game start: {(DateTime.Now - globalTimer).Seconds}");
                if ((DateTime.Now - globalTimer).Minutes >= maximumGameTime && !finialCountDownStarted) {
                    StartFinalCountDown();
                }

            }
           
        }

        private IEnumerator FinalCountDownTimerStart() {
            while (finalCountDown> 0) {
                finalCountDown--;
                yield return new WaitForSeconds(1);
            }

            int winTeam = 0;
            float team1Affinity = this.GetSystem<IGlobalTradingSystem>().GetTotalAffinityWithTeam(1);
            float team2Affinity = this.GetSystem<IGlobalTradingSystem>().GetTotalAffinityWithTeam(2);

            if (team1Affinity > team2Affinity) {
                winTeam = 1;
            }
            else if (team1Affinity < team2Affinity) {
                winTeam = 2;
            }
            else {
                winTeam = 0;
            }

            if (winTeam != 0) {
                List<PlayerMatchInfo> winPlayers = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(winTeam);
                List<string> winNames = winPlayers.Select(x => x.Name).ToList();
                
                RpcOnFinialCountDownEnds(winTeam, winNames);
            }else {
                finalCountDown = 60;
                StartCoroutine(FinalCountDownTimerStart());
               RpcOnTieTimerStarted();
            }
        }

        [ServerCallback]
        private void OnTransactionFinished(OnServerTransactionFinished e) {
            TotalTransactionTime.Value++;
            int totalPlayerCount = NetworkServer.connections.Count;
            if (TotalTransactionTime.Value >= finalCountdownTransactionThresholdPerPlayer * totalPlayerCount && !finialCountDownStarted) {
                StartFinalCountDown();
            }
        }

        [ServerCallback]
        protected void StartFinalCountDown() {
            finialCountDownStarted = true;
            Debug.Log("StartFinalCountdown");
            RpcOnFinialCountDownStarted();
            StartCoroutine(FinalCountDownTimerStart());
        }

        public BindableProperty<int> TotalTransactionTime { get; protected set; } = new BindableProperty<int>(0);


        [ClientRpc]
        protected void RpcOnFinialCountDownStarted() {
            Debug.Log("Target Start Final Countdown");
            this.SendEvent<OnClientFinalCountdownTimerStart>(new OnClientFinalCountdownTimerStart());
        }

        [ClientRpc]
        protected void RpcOnTieTimerStarted()
        {
            this.SendEvent<OnTieTimerStart>();
        }

        [ClientRpc]
        protected void RpcOnFinialCountDownEnds(int winTeam, List<string> winNames)
        {
            this.SendEvent<OnClientFinalCountDownTimerEnds>(new OnClientFinalCountDownTimerEnds() {
                WinTeam = winTeam,
                WinNames = winNames
            });
        }


        [ClientCallback]
        protected void ClientOnCountdownChange(int oldTime, int newTime) {
            if (newTime > 0) {
                this.SendEvent<OnClientFinalCountdownTimerChange>(new OnClientFinalCountdownTimerChange() {
                    Time = newTime
                });
            }
           
        }

    }
}

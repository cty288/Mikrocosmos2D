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

        /// <summary>
        /// This returns an estimated overall game progress, based on the maximum between overall trading progress and overall time passed.
        /// </summary>
        /// <returns></returns>
        public float GetGameProgress();

        /// <summary>
        /// x: xMin, y: xMax. z: yMin, a: yMax
        /// </summary>
        /// <returns></returns>
        public Vector4 GetGameMapSize();
    }
    public class GameProgressSystem : AbstractNetworkedSystem, IGameProgressSystem {
        protected DateTime globalTimer;
        [SerializeField] 
        private int globalTimerUpdateFrequencyInSeconds = 60;
        
        [Tooltip("In Minutes")]

        [SerializeField] protected float maximumGameTime = 15;
        [Tooltip("In Seconds")]
        [SerializeField, SyncVar(hook = nameof(ClientOnCountdownChange))] 
        protected int finalCountDown = 60;

        [SerializeField] protected int finalCountdownTransactionThresholdPerPlayer = 15;
        

        private bool finialCountDownStarted = false;

        private void Awake() {
            Mikrocosmos.Interface.RegisterSystem<IGameProgressSystem>(this);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            

            this.RegisterEvent<OnServerTransactionFinished>(OnTransactionFinished)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            globalTimer = DateTime.Now;
            StartCoroutine(GlobalTimerUpdate());
        }

        IEnumerator GlobalTimerUpdate() {
            while (true) {
                yield return new WaitForSeconds(globalTimerUpdateFrequencyInSeconds);
                
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
       
        public float GetGameProgress() {
            int totalPlayerCount = NetworkServer.connections.Count;
            return Mathf.Max(
                TotalTransactionTime.Value / ((float) (finalCountdownTransactionThresholdPerPlayer * totalPlayerCount)),
                (float) (DateTime.Now.Subtract(globalTimer).TotalMinutes / maximumGameTime));
        }

        public Vector4 GetGameMapSize() {
            List<GameObject> borders = GameObject.FindGameObjectsWithTag("Border").ToList();
            //get the border which has the smallest x and biggest x
            float minX = borders.OrderBy(x => x.transform.position.x).First().transform.position.x + 5;
            float maxX = borders.OrderByDescending(x => x.transform.position.x).First().transform.position.x - 5;
            //get the border which has the smallest y and biggest y
            float minY = borders.OrderBy(x => x.transform.position.y).First().transform.position.y +5;
            float maxY = borders.OrderByDescending(x => x.transform.position.y).First().transform.position.y-5;
            return new Vector4(minX, maxX, minY, maxY);
        }


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

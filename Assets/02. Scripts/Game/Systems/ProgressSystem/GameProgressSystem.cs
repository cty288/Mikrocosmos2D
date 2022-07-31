using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mikrocosmos
{
    public enum GameState {
        NotStarted,
        InGame,
        End
    }
    /*
    public struct OnClientFinalCountdownTimerChange {
        public int Time;
    }*/
    public struct OnClientFinalCountdownTimerStart {
        public float Time;
    }

    public struct OnClientBeginGameCountdownStart {
        public float Time;
    }
    public struct OnClientGameEnd {
        public GameEndInfo GameEndInfo;
    }
    public struct OnClientFinalCountDownTimerEnds {
        public int WinTeam;
        public List<string> WinNames;
    }

    public struct OnTieTimerStart {
        public float Time;
    }

    public class PlayerWinInfo {
        public PlayerMatchInfo PlayerInfo;
        public int Score;

        public PlayerWinInfo(PlayerMatchInfo playerInfo, int score) {
            PlayerInfo = playerInfo;
            Score = score;
        }

        public PlayerWinInfo() {

        }
    }

    public class CategoryWinner {
        public CategoryWinningType CategoryWinningType;
        public PlayerMatchInfo PlayerInfo;
        public CategoryWinner(CategoryWinningType categoryWinningType, PlayerMatchInfo playerInfo)
        {
            CategoryWinningType = categoryWinningType;
            PlayerInfo = playerInfo;
        }

        public CategoryWinner() {

        }
    }
    public class GameEndInfo {
        public int WinTeam;
        public float Team1Affinity;
        public List<PlayerWinInfo> PlayerWinInfos;
        public List<CategoryWinner> CategoryWinners;

        public GameEndInfo(int winTeam, float team1Affinity, List<PlayerWinInfo> playerWinInfos, List<CategoryWinner> categoryWinners) {
            WinTeam = winTeam;
            Team1Affinity = team1Affinity;
            PlayerWinInfos = playerWinInfos;
            CategoryWinners = categoryWinners;
        }

        public GameEndInfo() {
            PlayerWinInfos = new List<PlayerWinInfo>();
            CategoryWinners = new List<CategoryWinner>();
        }
    }

    public enum CategoryWinningType {
        MostTrade,
        EarnMostMoney,
        MostEffectiveKills,
        DieLeast
    }
    public interface IGameProgressSystem : ISystem {
     

        /// <summary>
        /// This returns an estimated overall game progress, based on the maximum between overall trading progress and overall time passed.
        /// </summary>
        /// <returns></returns>
        public float GameProgress { get; }

        /// <summary>
        /// x: xMin, y: xMax. z: yMin, a: yMax
        /// </summary>
        /// <returns></returns>
        public Vector4 GetGameMapSize();

     
        public GameState GameState { get; }
    }
    public class GameProgressSystem : AbstractGameProgressSystem {
        
        protected DateTime globalTimer;
        [SerializeField] 
        private int globalTimerUpdateFrequencyInSeconds = 60;
        
        [Tooltip("In Minutes")]

        [SerializeField] protected float maximumGameTime = 15;

        public float MaximumGameTime => maximumGameTime;
      

        [FormerlySerializedAs("finalCountdownTransactionThresholdPerPlayer")] 
       [SerializeField] protected int finalCountdownMoneyThresholdPerPlayer = 15;

    

     
        

      

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
        

        [ServerCallback]
        private void OnTransactionFinished(OnServerTransactionFinished e) {
            
            TotalTransactionMoney.Value+=e.Price;
            int totalPlayerCount = roomMatchSystem.GetActivePlayerNumber();
            if (TotalTransactionMoney.Value >= finalCountdownMoneyThresholdPerPlayer * totalPlayerCount && !finialCountDownStarted) {
                StartFinalCountDown();
            }
        }

        

        public BindableProperty<int> TotalTransactionMoney { get; protected set; } = new BindableProperty<int>(0);

        public override float GameProgress {
            get {
                int totalPlayerCount = roomMatchSystem.GetActivePlayerNumber();
                return Mathf.Max(
                    TotalTransactionMoney.Value / ((float)(finalCountdownMoneyThresholdPerPlayer * totalPlayerCount)),
                    (float)(DateTime.Now.Subtract(globalTimer).TotalMinutes / maximumGameTime));
            }
            protected set {}
        }
    }
}

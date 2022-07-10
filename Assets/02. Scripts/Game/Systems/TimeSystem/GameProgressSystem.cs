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
        InGame,
        End
    }
    public struct OnClientFinalCountdownTimerChange {
        public int Time;
    }
    public struct OnClientFinalCountdownTimerStart {
        
    }

    public struct OnClientGameEnd {
        public GameEndInfo GameEndInfo;
    }
    public struct OnClientFinalCountDownTimerEnds {
        public int WinTeam;
        public List<string> WinNames;
    }

    public struct OnTieTimerStart {

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

        public GameState GameState { get; }
    }
    public class GameProgressSystem : AbstractNetworkedSystem, IGameProgressSystem {
        [field: SerializeField, SyncVar] 
        public GameState GameState { get; set; } = GameState.InGame;
       

        protected DateTime globalTimer;
        [SerializeField] 
        private int globalTimerUpdateFrequencyInSeconds = 60;
        
        [Tooltip("In Minutes")]

        [SerializeField] protected float maximumGameTime = 15;
        [Tooltip("In Seconds")]
        [SerializeField, SyncVar(hook = nameof(ClientOnCountdownChange))] 
        protected int finalCountDown = 60;

        [SerializeField] protected int finalCountdownTransactionThresholdPerPlayer = 15;

        private IGlobalScoreSystem globalScoreSystem;

        [SerializeField] private CategoryWinningType[] categoryWinningTypes = new CategoryWinningType[] {
            CategoryWinningType.MostTrade,
            CategoryWinningType.EarnMostMoney,
            CategoryWinningType.MostEffectiveKills,
            CategoryWinningType.DieLeast
        };
        
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
            GameState = GameState.InGame;
            this.GetSystem<ITimeSystem>().AddDelayTask(0.1f, () => {
                globalScoreSystem = this.GetSystem<IGlobalScoreSystem>();
            });
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
                GameState = GameState.End;
                GameEndInfo endInfo = GetGameEndInfo(winTeam);
                //RpcOnFinialCountDownEnds(winTeam, winNames);
                RpcOnGameEnd(endInfo);
            }
            else {
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
        protected void RpcOnGameEnd(GameEndInfo gameEndInfo) {
            this.SendEvent<OnClientGameEnd>(new OnClientGameEnd() {
                GameEndInfo = gameEndInfo
            });
            
            Debug.Log($"All Winners: {gameEndInfo.PlayerWinInfos.Count}");
            foreach (PlayerWinInfo info in gameEndInfo.PlayerWinInfos) {
                Debug.Log($"All Winners: {info.PlayerInfo.Name} -- Score: {info.Score}");
            }
            Debug.Log($"All Winners -- Most Trade: {gameEndInfo.CategoryWinners[0].PlayerInfo.Name}");
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


        [ServerCallback]
        private GameEndInfo GetGameEndInfo(int winTeam)
        {
            float team1Affinity = this.GetSystem<IGlobalTradingSystem>().GetTotalAffinityWithTeam(1);

            List<PlayerMatchInfo> allPlayers = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfo();
            Dictionary<PlayerMatchInfo, IPlayerStatsSystem> playerToStats =
                new Dictionary<PlayerMatchInfo, IPlayerStatsSystem>();


            foreach (PlayerMatchInfo player in allPlayers) {
                playerToStats.Add(player, player.Identity.connectionToClient.identity
                    .GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.GetComponent<IPlayerStatsSystem>());
            }


            List<PlayerWinInfo> playerWinInfos = new List<PlayerWinInfo>();
            foreach (PlayerMatchInfo player in allPlayers) {
                playerWinInfos.Add(GetWinInfoForPlayer(player, playerToStats[player], winTeam));
            }

            playerWinInfos.Sort(((info1, info2) => -info1.Score.CompareTo(info2.Score)));
            List<CategoryWinner> categoryWinners = new List<CategoryWinner>();
            foreach (CategoryWinningType winningType in categoryWinningTypes) {
                categoryWinners.Add(GetWinnerForCategory(winningType, playerToStats, allPlayers));
            }

            return new GameEndInfo(winTeam, team1Affinity, playerWinInfos, categoryWinners);
        }

        private PlayerWinInfo GetWinInfoForPlayer(PlayerMatchInfo player, IPlayerStatsSystem statsSystem, int winTeam) {
            int score = statsSystem.Score;
            if (winTeam == player.Team)
            {
                score = (Mathf.RoundToInt(score * globalScoreSystem.WinningTeamScoreMultiplier));
            }

            return new PlayerWinInfo(player, score);
        }


        
        private CategoryWinner GetWinnerForCategory(CategoryWinningType winningType,
            Dictionary<PlayerMatchInfo, IPlayerStatsSystem> allPlayersAndStats, List<PlayerMatchInfo> allPlayers) {
            switch (winningType) {
                case CategoryWinningType.DieLeast:
                    allPlayers.Sort(((info1, info2) =>
                        allPlayersAndStats[info1].TotalDie.CompareTo(allPlayersAndStats[info2].TotalDie)));
                    return new CategoryWinner(winningType, allPlayers.First());
                    break;
                case CategoryWinningType.EarnMostMoney:
                    allPlayers.Sort(((info1, info2) =>
                        -allPlayersAndStats[info1].TotalMoneyEarned.CompareTo(allPlayersAndStats[info2].TotalMoneyEarned)));
                    return new CategoryWinner(winningType, allPlayers.First());
                    break;
                case CategoryWinningType.MostEffectiveKills:
                    allPlayers.Sort(((info1, info2) =>
                        -allPlayersAndStats[info1].EffectiveKills.CompareTo(allPlayersAndStats[info2].EffectiveKills)));
                    return new CategoryWinner(winningType, allPlayers.First());
                    break;
                case CategoryWinningType.MostTrade:
                    allPlayers.Sort(((info1, info2) =>
                        -allPlayersAndStats[info1].TotalTransactions.CompareTo(allPlayersAndStats[info2].TotalTransactions)));
                    return new CategoryWinner(winningType, allPlayers.First());
                    break;
                default: return null;
            }
        }

    }
}

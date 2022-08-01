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
    public abstract class AbstractGameProgressSystem :  AbstractNetworkedSystem, IGameProgressSystem{
        [field: SerializeField, SyncVar]
        public GameState GameState { get; set; } = GameState.NotStarted;

        [SerializeField] protected float gameStartCountdown = 3;


      
    
        [SerializeField]
        protected int finalCountDown = 60;


        protected IGlobalScoreSystem globalScoreSystem;
        protected IGlobalTradingSystem globalTradingSystem;
        protected int connectedPlayer = 0;
        protected float gameForceStartTimeout = 10;
        protected IRoomMatchSystem roomMatchSystem;

        [SerializeField]
        private CategoryWinningType[] categoryWinningTypes = new CategoryWinningType[] {
            CategoryWinningType.MostTrade,
            CategoryWinningType.EarnMostMoney,
            CategoryWinningType.MostEffectiveKills,
            CategoryWinningType.DieLeast
        };

        protected bool finialCountDownStarted = false;

        protected virtual void Awake() {
            Mikrocosmos.Interface.RegisterSystem<IGameProgressSystem>(this);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            roomMatchSystem = this.GetSystem<IRoomMatchSystem>();

      //      this.RegisterEvent<OnServerTransactionFinished>(OnTransactionFinished)
         //       .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(this.gameObject);
            globalTradingSystem = this.GetSystem<IGlobalTradingSystem>();



            GameState = GameState.NotStarted;
            this.GetSystem<ITimeSystem>().AddDelayTask(0.1f, () => {
                globalScoreSystem = this.GetSystem<IGlobalScoreSystem>();
            });
            this.GetSystem<ITimeSystem>().AddDelayTask(gameForceStartTimeout, () => {
                if (GameState == GameState.NotStarted)
                {
                    ReadyToStartGame();
                }
            });
        }

        [ServerCallback]
        private void OnMainGamePlayerConnected(OnNetworkedMainGamePlayerConnected e)
        {
            connectedPlayer++;
            if (connectedPlayer >= NetworkServer.connections.Count && GameState == GameState.NotStarted)
            {
                this.GetSystem<ITimeSystem>().AddDelayTask(2f, () => {
                    ReadyToStartGame();
                });
            }
        }


        [ServerCallback]
        private void ReadyToStartGame()
        {
            this.GetSystem<ITimeSystem>().AddDelayTask(gameStartCountdown + 0.1f, () => {
                GameState = GameState.InGame;
            });
            RpcOnBeginGameCountdownStart(gameStartCountdown);
        }




     

        private IEnumerator FinalCountDownTimerStart()
        {

            yield return new WaitForSeconds(finalCountDown);
            int winTeam = 0;
            float team1Affinity = globalTradingSystem.GetRelativeAffinityWithTeam(1);
            float team2Affinity = globalTradingSystem.GetRelativeAffinityWithTeam(2);

            if (team1Affinity > team2Affinity)
            {
                winTeam = 1;
            }
            else if (team1Affinity < team2Affinity)
            {
                winTeam = 2;
            }
            else
            {
                winTeam = 0;
            }

            if (winTeam != 0)
            {
                GameState = GameState.End;
                GameEndInfo endInfo = GetGameEndInfo(winTeam);
                //RpcOnFinialCountDownEnds(winTeam, winNames);
                RpcOnGameEnd(endInfo);
            }
            else
            {
                finalCountDown = 60;
                StartCoroutine(FinalCountDownTimerStart());
                RpcOnTieTimerStarted(finalCountDown);
            }
        }

        /*
        [ServerCallback]
        private void OnTransactionFinished(OnServerTransactionFinished e)
        {
            TotalTransactionMoney.Value += e.Price;
            int totalPlayerCount = roomMatchSystem.GetActivePlayerNumber();
            if (TotalTransactionMoney.Value >= finalCountdownMoneyThresholdPerPlayer * totalPlayerCount && !finialCountDownStarted)
            {
                StartFinalCountDown();
            }
        }*/




        [ServerCallback]
        protected void StartFinalCountDown()
        {
            finialCountDownStarted = true;
            Debug.Log("StartFinalCountdown");
            RpcOnFinialCountDownStarted(finalCountDown);
            StartCoroutine(FinalCountDownTimerStart());
        }


        public abstract float GameProgress { get;  protected set; }


        


        public Vector4 GetGameMapSize()
        {
            List<GameObject> borders = GameObject.FindGameObjectsWithTag("Border").ToList();
            //get the border which has the smallest x and biggest x
            float minX = borders.OrderBy(x => x.transform.position.x).First().transform.position.x + 5;
            float maxX = borders.OrderByDescending(x => x.transform.position.x).First().transform.position.x - 5;
            //get the border which has the smallest y and biggest y
            float minY = borders.OrderBy(x => x.transform.position.y).First().transform.position.y + 5;
            float maxY = borders.OrderByDescending(x => x.transform.position.y).First().transform.position.y - 5;
            return new Vector4(minX, maxX, minY, maxY);
        }

       


        [ClientRpc]
        protected void RpcOnFinialCountDownStarted(float time)
        {
            Debug.Log("Target Start Final Countdown");
            this.SendEvent<OnClientFinalCountdownTimerStart>(new OnClientFinalCountdownTimerStart()
            {
                Time = time
            });
            this.SendEvent<OnClientNextCountdown>(new OnClientNextCountdown()
            {
                remainingTime = time,
                ShowAffinityForLastTime = false
            });
        }

        [ClientRpc]
        protected void RpcOnGameEnd(GameEndInfo gameEndInfo)
        {
            this.SendEvent<OnClientGameEnd>(new OnClientGameEnd()
            {
                GameEndInfo = gameEndInfo
            });

        }

        [ClientRpc]
        protected void RpcOnTieTimerStarted(float time)
        {
            this.SendEvent<OnTieTimerStart>(new OnTieTimerStart()
            {
                Time = time
            });
            this.SendEvent<OnClientNextCountdown>(new OnClientNextCountdown()
            {
                remainingTime = time,
                ShowAffinityForLastTime = false
            });
        }


        [ClientRpc]
        private void RpcOnBeginGameCountdownStart(float time)
        {
            this.SendEvent<OnClientBeginGameCountdownStart>(new OnClientBeginGameCountdownStart()
            {
                Time = time
            });
        }

        /*
        [ClientCallback]
        protected void ClientOnCountdownChange(int oldTime, int newTime) {
            if (newTime > 0) {
                this.SendEvent<OnClientFinalCountdownTimerChange>(new OnClientFinalCountdownTimerChange() {
                    Time = newTime
                });
            }
        }*/




        [ServerCallback]
        private GameEndInfo GetGameEndInfo(int winTeam)
        {
            float team1Affinity = globalTradingSystem.GetRelativeAffinityWithTeam(1);

            List<PlayerMatchInfo> allPlayers = roomMatchSystem.ServerGetAllPlayerMatchInfo(false);
            Dictionary<PlayerMatchInfo, IPlayerStatsSystem> playerToStats =
                new Dictionary<PlayerMatchInfo, IPlayerStatsSystem>();


            foreach (PlayerMatchInfo player in allPlayers)
            {
                playerToStats.Add(player, player.Identity.connectionToClient.identity
                    .GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.GetComponent<IPlayerStatsSystem>());
            }


            List<PlayerWinInfo> playerWinInfos = new List<PlayerWinInfo>();
            foreach (PlayerMatchInfo player in allPlayers)
            {
                playerWinInfos.Add(GetWinInfoForPlayer(player, playerToStats[player], winTeam));
            }

            playerWinInfos.Sort(((info1, info2) => -info1.Score.CompareTo(info2.Score)));
            List<CategoryWinner> categoryWinners = new List<CategoryWinner>();
            foreach (CategoryWinningType winningType in categoryWinningTypes)
            {
                categoryWinners.Add(GetWinnerForCategory(winningType, playerToStats, allPlayers));
            }

            return new GameEndInfo(winTeam, team1Affinity, playerWinInfos, categoryWinners);
        }

        private PlayerWinInfo GetWinInfoForPlayer(PlayerMatchInfo player, IPlayerStatsSystem statsSystem, int winTeam)
        {
            int score = statsSystem.Score;
            if (winTeam == player.Team)
            {
                score = (Mathf.RoundToInt(score * globalScoreSystem.WinningTeamScoreMultiplier));
            }

            return new PlayerWinInfo(player, score);
        }



        private CategoryWinner GetWinnerForCategory(CategoryWinningType winningType,
            Dictionary<PlayerMatchInfo, IPlayerStatsSystem> allPlayersAndStats, List<PlayerMatchInfo> allPlayers)
        {
            if (allPlayers.Any())
            {
                switch (winningType)
                {
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

            return null;

        }
    }
}

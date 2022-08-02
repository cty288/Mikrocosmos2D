using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnStandardGameProgressChanged {
        public float Progress;
        public ClientProgressInfo Info;
    }

    public struct OnClientMissionTimeCutoffGenerated {
        public List<float> cutoffs;
    }

    public struct OnProgressMissionFinished {
        public int WinTeam;
        public int MissionIndex;
    }

    public struct ClientProgressInfo {
        public bool IsReachMissionPoint;
        public float Affinity;
        public int NextMissionIndex;
        public bool IsReachGameEndPoint;
        public ClientProgressInfo(bool isReachMissionPoint, float affinity, int nextMissionIndex, bool isReachGameEndPoint = false) {
            this.IsReachMissionPoint = isReachMissionPoint;
            this.Affinity = affinity;
            this.IsReachGameEndPoint = isReachGameEndPoint;
            this.NextMissionIndex = nextMissionIndex;
        }
        
    }
    public class StandardGameProgressSystem : AbstractGameProgressSystem {
        //trading + deal damage to players add game progress, unless there's a mission ongoing.
     
        [SerializeField]
        protected float currentGameProgress = 0;

        //real -> divided by player num
        [SerializeField] private float gameProgressIncreasmentPerTransactionRevenue = 0.00067f;
        [SerializeField] private float gameProgressIncreasmentPerDamageToPlayer = 0.0005f;

        
        [SerializeField]
        protected List<float> missionStartAtProgress = new List<float>();

        protected int MissionCount {
            get {
                return missionStartAtProgress.Count;
            }
        }

        [SerializeField]
        protected Vector2Int missionCountRange = new Vector2Int(3, 5);

        protected int lastMissionIndex = -1;
        
        public override float GameProgress {
            get {
                return currentGameProgress;
            }
            protected set {
                currentGameProgress = value;
            }
        }

     

        private IGameMissionSystem gameMissionSystem;
        private IMission ongoingMission = null;

        public override void OnStartServer() {
            base.OnStartServer();
            float missionCount = Random.Range(missionCountRange.x, missionCountRange.y + 1);
            //evenly distribute mission to progress between 0.1 to 0.9, but add some offsets
            float lastProgress = 0f;
            float averageProgress = 1f / (missionCount + 1);
            for (int i = 0; i < missionCount; i++) {
                lastProgress = Mathf.Clamp(lastProgress + averageProgress + Random.Range(-averageProgress / 4, averageProgress / 4), 0.1f,
                    0.9f);
                missionStartAtProgress.Add(lastProgress);
            }

            if (missionCount > 0) {
                gameMissionSystem = this.GetSystem<IGameMissionSystem>();
                if (gameMissionSystem == null) {
                    Debug.LogError("GameMissionSystem is required for StandardGameProgressSystem!");
                }
            }
           
            this.RegisterEvent<OnServerTransactionFinished>(OnServerTransactionFinished).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnPlayerTakeDamage>(OnPlayerTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnMissionStop>(OnMissionStop).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnNetworkedMainGamePlayerConnected).UnRegisterWhenGameObjectDestroyed(gameObject);
            
        }

        private void OnNetworkedMainGamePlayerConnected(OnNetworkedMainGamePlayerConnected e) {
            TargetOnGameProgressCutOffGenerated(e.connection, missionStartAtProgress);
        }


        private void OnPlayerTakeDamage(OnPlayerTakeDamage e) {
           
            if (e.Killer && e.Killer.TryGetComponent<PlayerSpaceship>(out PlayerSpaceship killer)) {
                if (e.SpaceshipIdentity.TryGetComponent<PlayerSpaceship>(out PlayerSpaceship victim)) {
                    if (killer.ThisSpaceshipTeam != victim.ThisSpaceshipTeam) {
                        AddProgress(e.Damage * (gameProgressIncreasmentPerDamageToPlayer /
                                                roomMatchSystem.GetActivePlayerNumber()));
                    }
                }
            }
        }

        private void OnServerTransactionFinished(OnServerTransactionFinished e) {
            AddProgress(e.Price *
                        (gameProgressIncreasmentPerTransactionRevenue / roomMatchSystem.GetActivePlayerNumber()));
        }

        private void AddProgress(float rawProgress) {
            if (currentGameProgress < 1 && ongoingMission == null) {
                float team1Affinity = globalTradingSystem.GetRelativeAffinityWithTeam(1);
                
                if (lastMissionIndex + 1 >= MissionCount) {
                    //judge if game stop
                    if (currentGameProgress + rawProgress >= 1) {
                        currentGameProgress = 1;
                        StartFinalCountDown();
                        RpcOnStandardGameProgressChanged(currentGameProgress,
                            new ClientProgressInfo(false, team1Affinity,MissionCount, true));
                    }else {
                        currentGameProgress += rawProgress;
                        RpcOnStandardGameProgressChanged(currentGameProgress,
                            new ClientProgressInfo(false, team1Affinity, MissionCount));
                    }
                }
                else {
                    float nextMissionProgress = missionStartAtProgress[lastMissionIndex + 1];
                    if (currentGameProgress + rawProgress >= nextMissionProgress) {
                        currentGameProgress = nextMissionProgress;
                        lastMissionIndex++;
                      
                        RpcOnStandardGameProgressChanged(currentGameProgress,
                            new ClientProgressInfo(true, team1Affinity, lastMissionIndex));
                        //TODO: start mission
                        ongoingMission = gameMissionSystem.StartMission(10);
                    }
                    else {
                        currentGameProgress += rawProgress;
                        RpcOnStandardGameProgressChanged(currentGameProgress,
                            new ClientProgressInfo(false, team1Affinity, lastMissionIndex+1));
                    }
                }                    
            }
        }

        private void OnMissionStop(OnMissionStop e) {
            if (e.Mission == ongoingMission) {
                ongoingMission = null;
                RpcOnProgressMissionFinished(e.WinningTeam, lastMissionIndex);
            }
        }        

        [ClientRpc]
        private void RpcOnStandardGameProgressChanged(float newProgress, ClientProgressInfo info) {
            
            this.SendEvent<OnStandardGameProgressChanged>(new OnStandardGameProgressChanged()
                {Progress = newProgress, Info = info});
        }

        [ClientRpc]
        private void RpcOnProgressMissionFinished(int winningTeam, int previousMissionIndex) {
            this.SendEvent<OnProgressMissionFinished>(new OnProgressMissionFinished() {
                MissionIndex = previousMissionIndex,
                WinTeam = winningTeam
            });
        }


        [TargetRpc]
        private void TargetOnGameProgressCutOffGenerated(NetworkConnection conn, List<float> missionTimeCutoffs) {
            this.SendEvent<OnClientMissionTimeCutoffGenerated>(new OnClientMissionTimeCutoffGenerated() {
                cutoffs = missionTimeCutoffs
            });
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public abstract class AbstractMissionSystem : AbstractNetworkedSystem, IGameMissionSystem{
        [SerializeField] protected List<GameObject> allMissions;

        protected IGameProgressSystem progressSystem;
        protected IRoomMatchSystem roomMatchSystem;
        protected IGlobalTradingSystem globalTradingSystem;

        protected List<IMission> ongoingMissions = new List<IMission>();
        private void Awake()
        {
            Mikrocosmos.Interface.RegisterSystem<IGameMissionSystem>(this);
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
            progressSystem = this.GetSystem<IGameProgressSystem>();
            this.globalTradingSystem = this.GetSystem<IGlobalTradingSystem>();
            allMissions.Shuffle();
            
            roomMatchSystem = this.GetSystem<IRoomMatchSystem>();
            this.RegisterEvent<OnMissionStop>(OnMissionStop).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnMissionAnnounceWinners>(OnMissionAnnounceWinners)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

        }

        [ServerCallback]
        private void OnMissionAnnounceWinners(OnMissionAnnounceWinners e)
        {
            List<NetworkMainGamePlayer> allPlayers = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfo(true)
                .Select(info => info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>())
                .ToList();

            Dictionary<NetworkMainGamePlayer, List<string>> allPlayersWithLocalizedRewards = RewardsFactory.Singleton.AssignRewardsToPlayers(allPlayers, e.Winners, e.Difficulty, e.WinningTeam);
            int difficulty = (Mathf.CeilToInt(e.Difficulty / 0.333334f));

            List<string> winnerNames = new List<string>();
            foreach (NetworkMainGamePlayer winner in e.Winners)
            {
                winnerNames.Add(winner.matchInfo.Name);
            }

            foreach (NetworkMainGamePlayer player in allPlayersWithLocalizedRewards.Keys)
            {
                TargetNotifyRewardsGenerated(player.connectionToClient, allPlayersWithLocalizedRewards[player], winnerNames,
                    e.MissionNameLocalizedKey, difficulty);
            }
        }

        public IMission StartMission(float waitTime) {
            GameObject nextMission = Instantiate(allMissions[0]);
            allMissions.RemoveAt(0);
            NetworkServer.Spawn(nextMission);
            IMission mission = nextMission.GetComponent<IMission>();
            ongoingMissions.Add(mission);
            StartCoroutine(DoStartMission(waitTime, mission, nextMission));
            return mission;
        }

        private IEnumerator DoStartMission(float waitTime, IMission mission, GameObject nextMission) {
           
            if (waitTime > 0.5f) {
                RpcNotifyMissionStart(new ClientMissionReadyToStartInfo() {
                    MissionName = mission.MissionName,
                    waitTIme = waitTime
                });
            }
            yield return new WaitForSeconds(waitTime);
            SwitchToMission(mission, nextMission);
        }

        

        private void SwitchToMission(IMission mission, GameObject missionGameObject) {
            StartCoroutine(MissionMaximumTimeCheck(mission, missionGameObject));
            mission.OnMissionStart(progressSystem.GameProgress, roomMatchSystem.GetActivePlayerNumber());
            this.SendEvent<OnMissionStart>(new OnMissionStart() { MissionName = mission.MissionName });
            RpcNotifyMissionAlreadytart(new ClientMissionStartInfo() {
                MissionDescriptionLocalizedKey = mission.MissionDescriptionLocalizedKey(),
                MissionMaximumTime = mission.MaximumTime,
                MissionName = mission.MissionName,
                MissionNameLocalizedKey = mission.MissionNameLocalizedKey(),
                MissionInfoBarAssetName = mission.MissionBarAssetName,
                MissionSliderAssetName = mission.MissionSliderBGName
            });
        }

        private IEnumerator MissionMaximumTimeCheck(IMission mission, GameObject missionGameObject) {
            if (mission.MaximumTime > 0) {
                yield return new WaitForSeconds(mission.MaximumTime);
                if (!mission.IsFinished) {
                    StopMission(mission);
                }
            }
        }


        private void OnMissionStop(OnMissionStop e) {
            RpcNotifyMissionStop(new ClientMissionStopInfo() { MissionName = e.Mission.MissionName });
            OnMissionStopped(e);
            ongoingMissions.Remove(e.Mission);
        }

        protected abstract void OnMissionStopped(OnMissionStop e);


        public void StopMission(IMission mission) {
            mission.StopMission(true);
        }

        [ClientRpc]
        private void RpcNotifyMissionStart(ClientMissionReadyToStartInfo info)
        {
            Debug.Log($"Client Mission Start Info: {info.MissionName}");
            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(new ClientInfoMessage()
            {
                Name = info.MissionName,
                Title = Localization.Get("GAME_MISSION_UPCOMING"),
                RemainingTime = info.waitTIme,
                AutoDestroyWhenTimeUp = false,
                ShowRemainingTime = true,
                InfoElementPrefabAssetName = InfoElementPrefabNames.ICON_INFO_NORMAL,

            });

            //this.GetSystem<IAudioSystem>().PlaySound("MissionUpcoming", SoundType.Sound2D);
        }

        [ClientRpc]
        private void RpcNotifyMissionAlreadytart(ClientMissionStartInfo info)
        {
            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(new ClientInfoMessage()
            {
                Name = info.MissionName,
                Title = Localization.Get(info.MissionNameLocalizedKey),
                Description = Localization.Get(info.MissionDescriptionLocalizedKey),
                RemainingTime = info.MissionMaximumTime,
                AutoDestroyWhenTimeUp = false,
                ShowRemainingTime = true,
                InfoElementPrefabAssetName = InfoElementPrefabNames.ICON_INFO_NORMAL,
                InfoElementIconAssetName = info.MissionName + "InfoIcon",
                InfoContainerSpriteAssetName = info.MissionInfoBarAssetName,
                InfoContainerSliderAssetName = info.MissionSliderAssetName
            });
        }

        [ClientRpc]
        private void RpcNotifyMissionStop(ClientMissionStopInfo info)
        {
            this.GetSystem<IClientInfoSystem>().StopInfo(info.MissionName);
        }

        [TargetRpc]
        private void TargetNotifyRewardsGenerated(NetworkConnection conn, List<string> rewardNames, List<string> winnerNames, string missionNameLocalizedKey,
            int difficultyLevel)
        {
            this.SendEvent<OnClientRewardsGeneratedForMission>(new OnClientRewardsGeneratedForMission()
            {
                DifficultyLevel = difficultyLevel,
                MissionNameLocalized = Localization.Get(missionNameLocalizedKey),
                RewardNames = rewardNames,
                WinnerNames = winnerNames
            });
        }
    }
}

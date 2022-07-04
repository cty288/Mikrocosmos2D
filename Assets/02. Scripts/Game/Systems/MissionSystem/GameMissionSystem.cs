using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using Mirror.FizzySteam;
using Polyglot;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{

    public static class ListExtension {
        public static void Shuffle<T>(this List<T> arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                var index = Random.Range(i, arr.Count);
                var tmp = arr[i];
                var ran = arr[index];
                arr[i] = ran;
                arr[index] = tmp;
            }
        }
    }
    public struct OnMissionStop {
        public IMission Mission;
        public GameObject MissionGameObject;
        public string MissionName;
        public bool Finished;
    }

    public struct OnMissionStart {
        public string MissionName;
    }

    public struct OnClientRewardsGeneratedForMission {
        public string MissionNameLocalized;
        public List<string> WinnerNames;
        public List<string> RewardNames;
        public int DifficultyLevel;
    }
  
    public struct ClientMissionReadyToStartInfo {
        public string MissionName;
        /*
        public string MissionDescriptionLocalized;
        public string MissionNameLocalized;*/
    }

    public struct ClientMissionStopInfo
    {
        public string MissionName;
        /*
        public string MissionDescriptionLocalized;
        public string MissionNameLocalized;*/
    }

    public struct ClientMissionStartInfo {
        public string MissionName;
        public string MissionDescriptionLocalizedKey;
        public string MissionNameLocalizedKey;
        public float MissionMaximumTime;
        public string MissionInfoBarAssetName;
        public string MissionSliderAssetName;
    }
    public interface IMission {
        string MissionName { get;  }

        string MissionBarAssetName { get; }

        string MissionSliderBGName { get; }
        string MissionNameLocalizedKey();
        string MissionDescriptionLocalizedKey();

        float MaximumTime { get; set; }

        bool IsFinished { get; set; }

        void OnMissionStart(float overallGameProgress);
        void StopMission(bool runOutOfTime = true);
    }

    public interface IGameMissionSystem : ISystem {

    }
    public class GameMissionSystem : AbstractNetworkedSystem, IGameMissionSystem {
        [SerializeField] private int averageGapTimeBetweenMissions = 120;
        [SerializeField] private List<GameObject> allMissions;

        private IGameProgressSystem progressSystem;


        private void Awake() {
            Mikrocosmos.Interface.RegisterSystem<IGameMissionSystem>(this);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            progressSystem = this.GetSystem<IGameProgressSystem>();
            allMissions.Shuffle();
            StartCoroutine(WaitToSwitchMission());

            this.RegisterEvent<OnMissionStop>(OnMissionStop).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnMissionAnnounceWinners>(OnMissionAnnounceWinners)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

        }

        [ServerCallback]
        private void OnMissionAnnounceWinners(OnMissionAnnounceWinners e) {
            Dictionary<NetworkMainGamePlayer, List<string>> playerToRewardNames = RewardsFactory.Singleton.AssignRewardsToPlayers(e.Winners, e.Difficulty);
            int difficulty = (Mathf.CeilToInt(e.Difficulty / 0.333334f));

            List<string> winnerNames = new List<string>();
            foreach (NetworkMainGamePlayer winner in e.Winners) {
                winnerNames.Add(winner.matchInfo.Name);
            }

            foreach (NetworkMainGamePlayer player in playerToRewardNames.Keys) {
                TargetNotifyRewardsGenerated(player.connectionToClient, playerToRewardNames[player], winnerNames,
                    e.MissionNameLocalizedKey, difficulty);
            }
        }


        private IEnumerator WaitToSwitchMission() {
            int waitTime = Random.Range(averageGapTimeBetweenMissions - 20,
                averageGapTimeBetweenMissions + 21);

            Debug.Log($" Mission waitTime: {waitTime}");

            yield return new WaitForSeconds(waitTime - 10);

            //notify client that a mission is about to start
            //first check if it's possible to get a mission
            if (progressSystem.GetGameProgress() < 0.9) {
                if (allMissions.Count > 0) {
                    GameObject nextMission = Instantiate(allMissions[0]);
                    allMissions.RemoveAt(0);
                    NetworkServer.Spawn(nextMission);
                    IMission mission = nextMission.GetComponent<IMission>();
                    RpcNotifyMissionStart(new ClientMissionReadyToStartInfo() {
                        MissionName = mission.MissionName
                    });
                    yield return new WaitForSeconds(10);
                    SwitchToMission(mission, nextMission);
                }
            }
            
        }

       

        
       


        private void SwitchToMission(IMission mission, GameObject missionGameObject) {
            StartCoroutine(MissionMaximumTimeCheck(mission, missionGameObject));
            mission.OnMissionStart(progressSystem.GetGameProgress());
            this.SendEvent<OnMissionStart>(new OnMissionStart(){MissionName = mission.MissionName});
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
                    mission.StopMission(true);
                }
            }
        }


        private void OnMissionStop(OnMissionStop e) {
            RpcNotifyMissionStop(new ClientMissionStopInfo() {MissionName = e.Mission.MissionName});
            StartCoroutine(WaitToSwitchMission());
        }
        

        [ClientRpc]
        private void RpcNotifyMissionStart(ClientMissionReadyToStartInfo info) {
            Debug.Log($"Client Mission Start Info: {info.MissionName}");
            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(new ClientInfoMessage() {
                Name = info.MissionName,
                Title = Localization.Get("GAME_MISSION_UPCOMING"),
                RemainingTime = 10,
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
        private void RpcNotifyMissionStop(ClientMissionStopInfo info) {
            this.GetSystem<IClientInfoSystem>().StopInfo(info.MissionName);
        }

        [TargetRpc]
        private void TargetNotifyRewardsGenerated(NetworkConnection conn, List<string> rewardNames, List<string> winnerNames, string missionNameLocalizedKey,
            int difficultyLevel) {
            this.SendEvent<OnClientRewardsGeneratedForMission>(new OnClientRewardsGeneratedForMission() {
                DifficultyLevel = difficultyLevel,
                MissionNameLocalized = Localization.Get(missionNameLocalizedKey),
                RewardNames = rewardNames,
                WinnerNames = winnerNames
            });
        }
    }
}

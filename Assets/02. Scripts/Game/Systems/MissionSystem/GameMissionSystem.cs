using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;

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
        public int WinningTeam;
    }

    public struct OnMissionStart {
        public string MissionName;
    }

    public struct OnClientNextCountdown {
        public float remainingTime;
        public float Team1Affinity;
        public bool ShowAffinityForLastTime;
    }
    public struct OnClientRewardsGeneratedForMission {
        public string MissionNameLocalized;
        public List<string> WinnerNames;
        public List<string> RewardNames;
        public int DifficultyLevel;
    }
  
    public struct ClientMissionReadyToStartInfo {
        public string MissionName;

        public float waitTIme;
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

        void OnMissionStart(float overallGameProgress, int playerNum);
        void StopMission(bool runOutOfTime = true);
    }

    public interface IGameMissionSystem : ISystem {
        IMission StartMission(float waitTime);
        void StopMission(IMission mission);
    }
    public class GameMissionSystem : AbstractMissionSystem {
        [SerializeField] private int averageGapTimeBetweenMissions = 120;
      
        private int nextMissionRemainingTime = 0;


       

        public override void OnStartServer() {
            base.OnStartServer();
            StartCoroutine(WaitToSwitchMission());

        }

        
      
        private IEnumerator WaitToSwitchMission() {
            while (progressSystem.GameState!= GameState.InGame) {
                yield return null;
            }
            
            nextMissionRemainingTime = Random.Range(averageGapTimeBetweenMissions - 20,
                averageGapTimeBetweenMissions + 21);
            
            RpcNotifyClientNextMissionCountdown(globalTradingSystem.GetRelativeAffinityWithTeam(1),
                nextMissionRemainingTime + 10);
            yield return new WaitForSeconds(nextMissionRemainingTime);
           
      
            if (progressSystem.GameProgress < 1 && ongoingMissions.Count==0) {
                if (allMissions.Count > 0) {
                    StartMission(10);
                }
            }
        }
        

    

        protected override void OnMissionStopped(OnMissionStop e) {
            if (progressSystem.GameProgress < 1) {
                StartCoroutine(WaitToSwitchMission());
            }
        }
        
        
        [ClientRpc]
        private void RpcNotifyClientNextMissionCountdown(float currentTeam1Affinity, float time) {
            this.SendEvent<OnClientNextCountdown>(new OnClientNextCountdown() {
                remainingTime = time,
                Team1Affinity = currentTeam1Affinity,
                ShowAffinityForLastTime = true
            });
        }

    }
}

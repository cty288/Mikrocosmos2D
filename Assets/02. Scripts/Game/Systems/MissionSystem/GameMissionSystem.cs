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

    public struct OnMissionStop {
        public IMission Mission;
        public GameObject MissionGameObject;
    }
    public abstract class AbstractGameMission: AbstractNetworkedSystem, IMission {
        public abstract string MissionName { get; }
        public abstract string MissionNameLocalized();

        public abstract string MissionDescriptionLocalized();

        public abstract float MaximumTime { get; set; }
        public bool IsFinished { get; set; }
        public abstract void OnMissionStart();

        public void AssignPermanentBuffToPlayers(List<IBuffSystem> buffSystems)
        {
            PermanentBuffType buffType = (PermanentBuffType)(Random.Range(0, Enum.GetValues(typeof(PermanentBuffType)).Length));
            foreach (IBuffSystem buffSystem in buffSystems)
            {
                PermanentBuffFactory.AddPermanentBuffToPlayer(buffType, buffSystem, 3, 0);
            }
        }
        public void StopMission() {
            IsFinished = true;
            this.SendEvent<OnMissionStop>(new OnMissionStop() {
                Mission = this,
                MissionGameObject = gameObject
            });
            OnMissionStop();
            NetworkServer.Destroy(gameObject);
        }

        protected abstract void OnMissionStop();

        
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
        public string MissionDescriptionLocalized;
        public string MissionNameLocalized;
        public float MissionMaximumTime;
    }
    public interface IMission {
        string MissionName { get;  }
        string MissionNameLocalized();
        string MissionDescriptionLocalized();

        float MaximumTime { get; set; }

        bool IsFinished { get; set; }

        void OnMissionStart();
        void StopMission();
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
            allMissions = Shuffle(allMissions);
            StartCoroutine(WaitToSwitchMission());

            this.RegisterEvent<OnMissionStop>(OnMissionStop).UnRegisterWhenGameObjectDestroyed(gameObject);
          
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
        private List<T> Shuffle<T>(List<T> original) {
            System.Random randomNum = new System.Random();
            int index = 0;
            T temp;
            for (int i = 0; i < original.Count; i++)
            {
                index = randomNum.Next(0, original.Count - 1);
                if (index != i)
                {
                    temp = original[i];
                    original[i] = original[index];
                    original[index] = temp;
                }
            }
            return original;
        }


        private void SwitchToMission(IMission mission, GameObject missionGameObject) {
            StartCoroutine(MissionMaximumTimeCheck(mission, missionGameObject));
            mission.OnMissionStart();
            RpcNotifyMissionAlreadytart(new ClientMissionStartInfo() {
                MissionDescriptionLocalized = mission.MissionDescriptionLocalized(),
                MissionMaximumTime = mission.MaximumTime,
                MissionName = mission.MissionName,
                MissionNameLocalized = mission.MissionNameLocalized()
            });
        }

        private IEnumerator MissionMaximumTimeCheck(IMission mission, GameObject missionGameObject) {
            if (mission.MaximumTime > 0) {
                yield return new WaitForSeconds(mission.MaximumTime);
                if (!mission.IsFinished) {
                    mission.StopMission();
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
                InfoType = InfoType.LongInfo,
                Name = info.MissionName,
                Title = Localization.Get("GAME_MISSION_UPCOMING"),
                RemainingTime = 10
            });
        }

        [ClientRpc]
        private void RpcNotifyMissionAlreadytart(ClientMissionStartInfo info)
        {
            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(new ClientInfoMessage()
            {
                InfoType = InfoType.LongInfo,
                Name = info.MissionName,
                Title = info.MissionNameLocalized,
                Description = info.MissionDescriptionLocalized,
                RemainingTime = info.MissionMaximumTime
            });
        }

        [ClientRpc]
        private void RpcNotifyMissionStop(ClientMissionStopInfo info) {
            this.GetSystem<IClientInfoSystem>().StopInfo(info.MissionName);
        }
    }
}

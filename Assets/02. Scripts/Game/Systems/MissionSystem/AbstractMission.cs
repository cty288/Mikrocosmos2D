using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{

    public struct OnMissionAnnounceWinners {
        public List<NetworkMainGamePlayer> Winners;
        public float Difficulty;
        public string MissionNameLocalizedKey;
    }
    public abstract class AbstractGameMission : AbstractNetworkedSystem, IMission
    {
        [field: SerializeField]
        public string MissionName { get; protected set; }

        [field: SerializeField]
        public string MissionBarAssetName { get; protected set; }

        [field:SerializeField]
        public string MissionSliderBGName { get; protected set; }
        public abstract string MissionNameLocalizedKey();

        public abstract string MissionDescriptionLocalizedKey();

        public abstract float MaximumTime { get; set; }
        public bool IsFinished { get; set; }

        protected float startDifficulty;

        public void OnMissionStart(float overallProgress) {
            startDifficulty = overallProgress;
            OnStartMission(startDifficulty);
        }

        public abstract void OnStartMission(float overallProgress);

        [ServerCallback]
        public void AnnounceWinners(List<NetworkMainGamePlayer> players) {
            this.SendEvent<OnMissionAnnounceWinners>(new OnMissionAnnounceWinners() {
                Difficulty = startDifficulty,
                MissionNameLocalizedKey = MissionNameLocalizedKey(),
                Winners = players
            });
        }
     
        public void StopMission(bool runOutOfTime = true)
        {
            IsFinished = true;
            this.SendEvent<OnMissionStop>(new OnMissionStop()
            {
                Mission = this,
                MissionGameObject = gameObject,
                MissionName = MissionName,
                Finished = runOutOfTime
            });
            OnMissionStop(runOutOfTime);
            NetworkServer.Destroy(gameObject);
        }

        protected abstract void OnMissionStop(bool runOutOfTime = true);


    }
}

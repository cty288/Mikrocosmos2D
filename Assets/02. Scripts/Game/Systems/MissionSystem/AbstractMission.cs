using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public abstract class AbstractGameMission : AbstractNetworkedSystem, IMission
    {
        public abstract string MissionName { get; }
        public abstract string MissionNameLocalized();

        public abstract string MissionDescriptionLocalized();

        public abstract float MaximumTime { get; set; }
        public bool IsFinished { get; set; }
        public abstract void OnMissionStart(float overallProgress);

        public void AssignPermanentBuffToPlayers(List<IBuffSystem> buffSystems)
        {
            PermanentBuffType buffType = (PermanentBuffType)(Random.Range(0, Enum.GetValues(typeof(PermanentBuffType)).Length));
            foreach (IBuffSystem buffSystem in buffSystems)
            {
                PermanentBuffFactory.AddPermanentBuffToPlayer(buffType, buffSystem, 3, 0);
            }
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

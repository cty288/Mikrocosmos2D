using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using Polyglot;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{

    public enum RewardType {
        RandomBuff,
    }
    
    public class RewardsFactory  :  AbstractMikroController<Mikrocosmos>, ISingleton {
        public static RewardsFactory Singleton {
            get {
                return SingletonProperty<RewardsFactory>.Singleton;
            }
        }
        public List<string> AssignRewardsToPlayers(List<NetworkMainGamePlayer> players, float difficulty) {
            List<string> resultRewardNames = new List<string>();
            int rewardNumber = Mathf.CeilToInt(difficulty / 0.333334f) + 1;

            List<PermanentBuffType> allBuffs = GetAllBuffTypes();

            List<IBuffSystem> buffSystems = players.Select((info => {
                return info.ControlledSpaceship
                    .GetComponent<IBuffSystem>();
            })).ToList();

            for (int i = 0; i < rewardNumber; i++) {
                RewardType rewardType = (RewardType)(Random.Range(0, Enum.GetValues(typeof(RewardType)).Length));
                string rewardNameLocalized = "";
                switch (rewardType) {
                    case RewardType.RandomBuff:
                        int randomBuff = Random.Range(0, allBuffs.Count);
                        PermanentBuffType buff = allBuffs[randomBuff];
                        allBuffs.RemoveAt(randomBuff);
                        rewardNameLocalized = GetBuffNameLocalized(buff);
                        foreach (IBuffSystem buffSystem in buffSystems) {
                            PermanentBuffFactory.AddPermanentBuffToPlayer(buff, buffSystem, 3, 0);
                        }
                        break;
                }
                resultRewardNames.Add(rewardNameLocalized);
            }

            return resultRewardNames;
        }

        private List<PermanentBuffType> GetAllBuffTypes() {
            List<PermanentBuffType> allBuffs = new List<PermanentBuffType>();
            foreach (object value in Enum.GetValues(typeof(PermanentBuffType))) {
                allBuffs.Add((PermanentBuffType) value);
            }

            return allBuffs;
        }

        private string GetBuffNameLocalized(PermanentBuffType buffType) {
            string buffName = Localization.Get("GAME_BUFF");
            switch (buffType) {
                case PermanentBuffType.Affinity:
                    buffName+= Localization.Get("GAME_PERM_BUFF_AFFINITY");
                    break;
                case PermanentBuffType.Health:
                    buffName += Localization.Get("GAME_PERM_BUFF_HEALTH");
                    break;
                case PermanentBuffType.PowerUp:
                    buffName += Localization.Get("GAME_PERM_BUFF_POWER_UP");
                    break;
                case PermanentBuffType.Speed:
                    buffName += Localization.Get("GAME_PERM_BUFF_SPEED_UP");
                    break;
                case PermanentBuffType.VisionExpansion:
                    buffName += Localization.Get("GAME_PERM_BUFF_VISION_EXPANSION");
                    break;
            }

            return buffName;
        }

        public void OnSingletonInit() {
            
        }
    }
}

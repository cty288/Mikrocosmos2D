using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using Mirror;
using Polyglot;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{

    public enum RewardType {
        RandomBuff,
        Money,
        Slot
    }
    
    public class RewardsFactory  :  AbstractMikroController<Mikrocosmos>, ISingleton {
        public static RewardsFactory Singleton {
            get {
                return SingletonProperty<RewardsFactory>.Singleton;
            }
        }

        private IGlobalTradingSystem globalTradingSystem;

        private void Awake() {
            globalTradingSystem = this.GetSystem<IGlobalTradingSystem>();
        }

        /// <summary>
        /// Return a dictionary that stores a list of reward names for each player, where the names are localized for each player according to their languages
        /// </summary>
        /// <param name="players"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public Dictionary<NetworkMainGamePlayer, List<string>> AssignRewardsToPlayers(List<NetworkMainGamePlayer> allPlayers, List<NetworkMainGamePlayer> winners, float difficulty, int winnerTeam) {

            if (!NetworkServer.active) {
                return null;
            }
            int rewardNumber = Mathf.CeilToInt(difficulty / 0.333334f);

            List<PermanentBuffType> allBuffs = GetAllBuffTypes();

            List<IBuffSystem> buffSystems = winners.Select((info => {
                return info.ControlledSpaceship
                    .GetComponent<IBuffSystem>();
            })).ToList();


            Dictionary<NetworkMainGamePlayer, List<string>> allPlayersWithRewardNames =
                new Dictionary<NetworkMainGamePlayer, List<string>>();

            foreach (NetworkMainGamePlayer player in allPlayers) {
                allPlayersWithRewardNames.Add(player, new List<string>());
            }

            int totalMoney = 0;
            int totalSlots = 0;
            
            //affinity increasment is always rewarded
            float winningTeamAffinity = globalTradingSystem.GetRelativeAffinityWithTeam(winnerTeam);
            float losingTeamAffinity = 1 - winningTeamAffinity;
            if (winningTeamAffinity < losingTeamAffinity) {
                float affinityIncreasment = (losingTeamAffinity - winningTeamAffinity) / 2;
                if (affinityIncreasment >= 0.01f) {
                    affinityIncreasment += Random.Range(-0.03f, 0.03f);
                    affinityIncreasment = Mathf.Clamp(affinityIncreasment, 0.01f, 0.25f);
                    globalTradingSystem.AddAffinityToAllPlanets(winnerTeam, affinityIncreasment);
                }

            
                foreach (NetworkMainGamePlayer player in allPlayersWithRewardNames.Keys) {

                    Language languege = player.matchInfo.Language;
                    allPlayersWithRewardNames[player].Add(Localization.GetFormat("GAME_AFFINITY_INCREASMENT", languege,
                        Localization.Get($"GAME_AFFINITY_INCREASMENT_{Mathf.Clamp((Mathf.FloorToInt(affinityIncreasment / 0.08f))+1,1,3)}", languege)));
                }
            }

            
            for (int i = 0; i < rewardNumber; i++) {
                RewardType rewardType = (RewardType)(Random.Range(0, Enum.GetValues(typeof(RewardType)).Length));

                switch (rewardType) {
                    case RewardType.RandomBuff:
                        int randomBuff = Random.Range(0, allBuffs.Count);
                        PermanentBuffType buff = allBuffs[randomBuff];
                        allBuffs.RemoveAt(randomBuff);
                        foreach (NetworkMainGamePlayer player in allPlayersWithRewardNames.Keys) {
                            Language languege = player.matchInfo.Language;
                            allPlayersWithRewardNames[player].Add(GetBuffNameLocalized(buff, languege));
                        }
                        foreach (IBuffSystem buffSystem in buffSystems) {
                            PermanentBuffFactory.AddPermanentBuffToPlayer(buff, buffSystem, 3, 0);
                        }
                        break;
                    case RewardType.Money:
                        totalMoney += 100;
                        break;
                    case RewardType.Slot:
                        totalSlots++;
                        break;
                }
            }





            if (totalMoney > 0) {
                foreach (NetworkMainGamePlayer player in winners) {
                    player.ControlledSpaceship.GetComponent<IPlayerTradingSystem>().ReceiveMoney(totalMoney);
                }
                foreach (NetworkMainGamePlayer player in allPlayersWithRewardNames.Keys) {
                    Language languege = player.matchInfo.Language;
                    allPlayersWithRewardNames[player].Add(Localization.GetFormat("GAME_MISSION_REWARD_MONEY", languege, totalMoney));
                }
            }

            if (totalSlots > 0) {
                foreach (NetworkMainGamePlayer player in winners) {
                    player.ControlledSpaceship.GetComponent<IPlayerInventorySystem>().ServerAddSlots(totalSlots);
                }
                foreach (NetworkMainGamePlayer player in allPlayersWithRewardNames.Keys) {
                    
                    Language languege = player.matchInfo.Language;
                    allPlayersWithRewardNames[player].Add(Localization.GetFormat("GAME_MISSION_REWARD_SLOT",languege, totalSlots));
                }
            }
            

            return allPlayersWithRewardNames;
        }

        
        

        private List<PermanentBuffType> GetAllBuffTypes() {
            List<PermanentBuffType> allBuffs = new List<PermanentBuffType>();
            foreach (object value in Enum.GetValues(typeof(PermanentBuffType))) {
                allBuffs.Add((PermanentBuffType) value);
            }

            return allBuffs;
        }

        private string GetBuffNameLocalized(PermanentBuffType buffType, Language language) {
            string buffName = Localization.Get("GAME_BUFF");
            switch (buffType) {
                case PermanentBuffType.Affinity:
                    buffName+= Localization.Get("GAME_PERM_BUFF_AFFINITY", language);
                    break;
                case PermanentBuffType.Health:
                    buffName += Localization.Get("GAME_PERM_BUFF_HEALTH", language);
                    break;
                case PermanentBuffType.PowerUp:
                    buffName += Localization.Get("GAME_PERM_BUFF_POWER_UP", language);
                    break;
                case PermanentBuffType.Speed:
                    buffName += Localization.Get("GAME_PERM_BUFF_SPEED_UP", language);
                    break;
                case PermanentBuffType.VisionExpansion:
                    buffName += Localization.Get("GAME_PERM_BUFF_VISION_EXPANSION", language);
                    break;
            }

            return buffName;
        }

        public void OnSingletonInit() {
            
        }
    }
}

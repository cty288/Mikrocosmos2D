using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
using Mirror.FizzySteam;
#endif
using Polyglot;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Mikrocosmos
{

    public interface IBountyHunterSystem : ISystem {

    }

    public struct OnClientSpaceshipCriminalityUpdate {
        public NetworkIdentity SpaceshipIdentity;
        public int Criminality;
        public BountyType BountyType;
    }

    public struct OnNewCriminalGenerated {
        public NetworkIdentity Criminal;
    }
    public class KillerInfo {
        public int KillerTotalKills;
        public int KillerTeam;
        public int PreviousTotalKillsSinceLastUpdate;
        public int CurrentBounty;
        public string KillerName;
    }

    public enum BountyType {
        Self,
        Opponent,
        Teammate
    }

    public struct OnCriminalKilledByHunter {
        public NetworkIdentity Hunter;
        public NetworkIdentity Criminal;
    }
    public class BountyHunterSystem : AbstractNetworkedSystem, IBountyHunterSystem {
        [SerializeField] private int minimumVictiumToTriggerBountyHunter = 3;

        private Dictionary<NetworkIdentity, KillerInfo>
            currentCriminals = new Dictionary<NetworkIdentity, KillerInfo>();

        private Dictionary<int, List<PlayerMatchInfo>>
            allPlayersWithTeam = new Dictionary<int, List<PlayerMatchInfo>>();

        [SerializeField] private int baseBounty = 50;
        [SerializeField] private int additionalBountyPerKill = 20;

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnPlayerMultiKillUpdate>(OnPlayerMultiKillUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject, true);

            this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject, true);
            
            allPlayersWithTeam.Add(1, this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(1));
            allPlayersWithTeam.Add(2, this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(2));
        }

        private void OnPlayerDie(OnPlayerDie e) {
            
            if (currentCriminals.ContainsKey(e.SpaceshipIdentity) && e.Killer && e.Killer.GetComponent<PlayerSpaceship>()) {
                KillerInfo suspect = currentCriminals[e.SpaceshipIdentity];
                if (suspect.KillerTeam == e.Killer.GetComponent<PlayerSpaceship>().ThisSpaceshipTeam) {
                    return;
                }

                currentCriminals.Remove(e.SpaceshipIdentity);
                //reward killer and notify
                int bounty = suspect.CurrentBounty;
                if (e.Killer.TryGetComponent<IPlayerTradingSystem>(out IPlayerTradingSystem killerTradingSystem)) {
                    killerTradingSystem.ReceiveMoney(bounty);
                }

                this.SendEvent<OnCriminalKilledByHunter>(new OnCriminalKilledByHunter() {
                    Hunter = e.Killer,
                    Criminal = e.SpaceshipIdentity
                });
                RpcOnCriminalKilled(e.Killer.GetComponent<PlayerSpaceship>().Name, suspect.KillerName, bounty, e.SpaceshipIdentity);
            }
        }


        private void OnPlayerMultiKillUpdate(OnPlayerMultiKillUpdate e) {
            if (currentCriminals.ContainsKey(e.Victim)) {
                return;
            }
            if (!currentCriminals.ContainsKey(e.Player)) {
                if (e.MultiKillNumber >= minimumVictiumToTriggerBountyHunter) {
                    
                    //A new criminal
                    currentCriminals.Add(e.Player,
                        new KillerInfo() {
                            KillerTotalKills = e.MultiKillNumber, KillerTeam = e.PlayerTeam,
                            KillerName = e.Player.GetComponent<PlayerSpaceship>().Name,
                            CurrentBounty = baseBounty,
                            PreviousTotalKillsSinceLastUpdate = e.MultiKillNumber
                        });
                    OnNewCriminalGenerate(e.Player, currentCriminals[e.Player]);
                }
            }else { //existing criminal kill more players
                currentCriminals[e.Player].KillerTotalKills ++;
                currentCriminals[e.Player].CurrentBounty +=   additionalBountyPerKill;
                OnExistingCrimalBountyUpdate(e.Player, currentCriminals[e.Player]);
            }
        }


        [ServerCallback]
        private void OnNewCriminalGenerate(NetworkIdentity killer, KillerInfo killerInfo) {
            int killerTeam = killerInfo.KillerTeam;
            int otherTeam = killerTeam == 1 ? 2 : 1;

            bool killerMessageSent = false;
            this.SendEvent<OnNewCriminalGenerated>(new OnNewCriminalGenerated() {
                Criminal = killer
            });
            foreach (PlayerMatchInfo info in allPlayersWithTeam[killerTeam]) {
                if (!killerMessageSent && info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>()
                        .ControlledSpaceship == killer) {
                    killerMessageSent = true;
                    TargetOnNewCriminalGenerate(info.Identity.connectionToClient, killerInfo.KillerName,
                        BountyType.Self, killer, killerInfo.KillerTotalKills);
                }
                else {
                    TargetOnNewCriminalGenerate(info.Identity.connectionToClient, killerInfo.KillerName,
                        BountyType.Teammate,killer, killerInfo.KillerTotalKills);
                }
            }


            foreach (PlayerMatchInfo info in allPlayersWithTeam[otherTeam]) {
                TargetOnNewCriminalGenerate(info.Identity.connectionToClient, killerInfo.KillerName,
                    BountyType.Opponent, killer, killerInfo.KillerTotalKills);
            }
        }

        [ServerCallback]
        private void OnExistingCrimalBountyUpdate(NetworkIdentity killer, KillerInfo killerInfo) {
            if (killerInfo.KillerTotalKills - killerInfo.PreviousTotalKillsSinceLastUpdate >= 2) {
                killerInfo.PreviousTotalKillsSinceLastUpdate = killerInfo.KillerTotalKills;
                
                int killerTeam = killerInfo.KillerTeam;
                int otherTeam = killerTeam == 1 ? 2 : 1;

                bool killerMessageSent = false;

                foreach (PlayerMatchInfo info in allPlayersWithTeam[killerTeam]) {
                    if (!killerMessageSent && info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>()
                            .ControlledSpaceship == killer)
                    {
                        killerMessageSent = true;
                        TargetOnCriminalInfoUpdate(info.Identity.connectionToClient, killerInfo.KillerName,
                            BountyType.Self, killer, killerInfo.KillerTotalKills);
                    }
                    else {
                        TargetOnCriminalInfoUpdate(info.Identity.connectionToClient, killerInfo.KillerName,
                            BountyType.Teammate, killer, killerInfo.KillerTotalKills);
                    }
                }


                foreach (PlayerMatchInfo info in allPlayersWithTeam[otherTeam]) {
                    TargetOnCriminalInfoUpdate(info.Identity.connectionToClient, killerInfo.KillerName,
                        BountyType.Opponent, killer, killerInfo.KillerTotalKills);
                }
            }

        }

        [ClientRpc]
        private void RpcOnCriminalKilled(string hunterName, string criminalName, int bounty, NetworkIdentity killerIdentity) {
            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(new ClientInfoMessage() {
                AutoDestroyWhenTimeUp = true,
                // Description = "",
                Name = $"CriminalUpdate_{criminalName}",
                RemainingTime = 10f,
                ShowRemainingTime = false,
                InfoElementPrefabAssetName = InfoElementPrefabNames.ICON_WARNING_NORMAL,
                Title = Localization.GetFormat("BOUNTY_KILLED", criminalName, hunterName, bounty),
                InfoElementIconAssetName = "WantedInfoIcon"
            });
            this.SendEvent<OnClientSpaceshipCriminalityUpdate>(new OnClientSpaceshipCriminalityUpdate() {
                Criminality = 0,
                SpaceshipIdentity = killerIdentity
            });
        }

        [TargetRpc]
        private void TargetOnCriminalInfoUpdate(NetworkConnection connection, string killerName,
            BountyType bountyType, NetworkIdentity killerIdentity, int killNumber) {
            ClientInfoMessage message = new ClientInfoMessage() {
               // AutoDestroyWhenTimeUp = true,
                // Description = "",
                Name = $"CriminalUpdate_{killerName}",
                RemainingTime = -1f,
                //Title = Localization.Get("GAME_INFO_EYE_DETECT"),
                ShowRemainingTime = false,
                InfoElementPrefabAssetName = InfoElementPrefabNames.ICON_WARNING_NORMAL,
                InfoElementIconAssetName = "WantedInfoIcon"
            };
            switch (bountyType) {
                case BountyType.Self:
                    message.Title = Localization.Get("BOUNTY_SELF_UPDATE");
                    break;
                case BountyType.Opponent:
                    message.Title = Localization.GetFormat("BOUNTY_OPPONENT_UPDATE", killerName);
                    break;
                case BountyType.Teammate:
                    message.Title = Localization.GetFormat("BOUNTY_TEAMMATE_UPDATE", killerName);
                    break;
            }

            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(message);
            this.SendEvent<OnClientSpaceshipCriminalityUpdate>(new OnClientSpaceshipCriminalityUpdate()
            {
                Criminality = 1 + ((killNumber - minimumVictiumToTriggerBountyHunter) / 2),
                SpaceshipIdentity = killerIdentity,
                BountyType = bountyType                
            });
        }


        [TargetRpc]
        private void TargetOnNewCriminalGenerate(NetworkConnection connection, string killerName,
            BountyType bountyType, NetworkIdentity killerIdentity, int killNumber) {
            ClientInfoMessage message = new ClientInfoMessage() {
                //AutoDestroyWhenTimeUp = true,
                // Description = "",
                Name = $"CriminalUpdate_{killerName}",
                RemainingTime = -1f,
                //Title = Localization.Get("GAME_INFO_EYE_DETECT"),
                ShowRemainingTime = false,
                InfoElementPrefabAssetName = InfoElementPrefabNames.ICON_WARNING_NORMAL,
                InfoElementIconAssetName = "WantedInfoIcon"
            };
            switch (bountyType) {
                case BountyType.Self:
                    message.Title = Localization.Get("BOUNTY_SELF_BECOME_TARGET");
                    break;
                case BountyType.Opponent:
                    message.Title = Localization.GetFormat("BOUNTY_OPPONENT_BECOME_TARGET", killerName);
                    break;
                case BountyType.Teammate:
                    message.Title = Localization.GetFormat("BOUNTY_TEAMMATE_BECOME_TARGET", killerName);
                    break;
            }

            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(message);
            this.SendEvent<OnClientSpaceshipCriminalityUpdate>(new OnClientSpaceshipCriminalityUpdate() {
                Criminality = 1 + ((killNumber - minimumVictiumToTriggerBountyHunter) / 2),
                SpaceshipIdentity = killerIdentity,
                BountyType = bountyType
            });
        }
    }
}

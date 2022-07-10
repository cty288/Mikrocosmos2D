using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IPlayerStatsSystem : ISystem {
        public int Score { get; }
        
    }

    public struct OnPlayerMultiKillUpdate {
        public NetworkIdentity Player;
        public int PlayerTeam;
        public int MultiKillNumber;
        public NetworkIdentity Victim;
    }
    public class PlayerStatsSystem : AbstractNetworkedSystem, IPlayerStatsSystem {
        [SerializeField] private float multiKillTimeInterval = 30f;
        [field: SerializeField] public int Score { get; set; } = 0;

        private DateTime lastKillTime;
        private int thisSpaceshipTeam;

        //Not counting killing teammates
        private int effectiveKills = 0;
        private int totalKills = 0;
        private int killTeammatesCount = 0;

        private int effectiveDamage = 0;

        private int currentMultiKills = 0;

        private IGlobalScoreSystem globalScoreSystem;
        public override void OnStartServer() {
            base.OnStartServer();
            lastKillTime = DateTime.Now;
            thisSpaceshipTeam = connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
            
            globalScoreSystem = this.GetSystem<IGlobalScoreSystem>();
            
            this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnEntityTakeDamage>(OnEntityTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnServerTransactionFinished>(OnTransactionFinished)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCriminalKilledByHunter>(OnCriminalKilledByHunter)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnMissionAnnounceWinners>(OnMissionAnnounceWinners)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnMissionAnnounceWinners(OnMissionAnnounceWinners e) {
            if (e.Winners.Find((player => player.ControlledSpaceship == netIdentity))) {
                Score += globalScoreSystem.ScorePerMissionFinished;
            }
        }

        private void OnCriminalKilledByHunter(OnCriminalKilledByHunter e) {
            if (e.Hunter == netIdentity) {
                Score += globalScoreSystem.ScorePerBountyFinished;
            }
        }

        private void OnTransactionFinished(OnServerTransactionFinished e) {
            if (e.Trader == netIdentity) {
                Score += globalScoreSystem.ScorePerTransactionMoney * e.Price;
            }
        }

        private void OnEntityTakeDamage(OnEntityTakeDamage e) {
            if (e.DamageSource == netIdentity) {
                if (e.EntityIdentity.TryGetComponent<PlayerSpaceship>(out var spaceship)) {
                    if (spaceship.ThisSpaceshipTeam == thisSpaceshipTeam) {
                        return;
                    }
                }
                int damage = (e.OldHealth - e.NewHealth);
                effectiveDamage += damage;
                Score += globalScoreSystem.ScorePerEffectiveDamage * damage;
            }
        }

        private void OnPlayerDie(OnPlayerDie e) {
            //first, if somebody die
            if (e.SpaceshipIdentity != netIdentity) {
                //if this spaceship killed that
                if (e.Killer == netIdentity) {
                    if (e.SpaceshipIdentity.GetComponent<PlayerSpaceship>().ThisSpaceshipTeam != thisSpaceshipTeam) {
                        effectiveKills++;
                        Score += globalScoreSystem.ScorePerEffectiveKill;
                    }else {
                        killTeammatesCount++;
                    }

                    totalKills++;

                    if ((DateTime.Now - lastKillTime).TotalSeconds <= multiKillTimeInterval) {
                        currentMultiKills++;
                    }
                    else {
                        currentMultiKills = 1;
                    }

                    lastKillTime = DateTime.Now;
                    this.SendEvent<OnPlayerMultiKillUpdate>(new OnPlayerMultiKillUpdate() {
                        MultiKillNumber = currentMultiKills,
                        Player = netIdentity,
                        PlayerTeam = thisSpaceshipTeam,
                        Victim = e.SpaceshipIdentity
                    });
                }
            }
            else { //if the dead player is this player
                currentMultiKills = 0;
            }
        }

    
    }
}

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

    }

    public struct OnPlayerMultiKillUpdate {
        public NetworkIdentity Player;
        public int PlayerTeam;
        public int MultiKillNumber;
        public NetworkIdentity Victim;
    }
    public class PlayerStatsSystem : AbstractNetworkedSystem, IPlayerStatsSystem {
        [SerializeField] private float multiKillTimeInterval = 30f;


        private DateTime lastKillTime;
        private int thisSpaceshipTeam;

        //Not counting killing teammates
        private int effectiveKills = 0;
        private int totalKills = 0;
        private int killTeammatesCount = 0;

        private int currentMultiKills = 0;
        

        public override void OnStartServer() {
            base.OnStartServer();
            lastKillTime = DateTime.Now;
            thisSpaceshipTeam = connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().matchInfo.Team;

            this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnPlayerDie(OnPlayerDie e) {
            //first, if somebody die
            if (e.SpaceshipIdentity != netIdentity) {
                //if this spaceship killed that
                if (e.Killer == netIdentity) {
                    if (e.SpaceshipIdentity.GetComponent<PlayerSpaceship>().ThisSpaceshipTeam != thisSpaceshipTeam) {
                        effectiveKills++;
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

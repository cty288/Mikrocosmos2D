using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    [Serializable]
    public class PlayerMatchInfo {
        public int ID;
        public string Name;
        public int Team;
        public bool Prepared;
        public NetworkIdentity Identity;
    }

    public struct OnServerRoomPlayerJoin {

    }

    public struct OnMatchInfoSet {
        public NetworkConnection Connection;
        public PlayerMatchInfo MatchInfo;
    }

    public struct OnRoomMemberChange {
        public List<PlayerMatchInfo> MatchInfos;
        public NetworkIdentity Host;
    }

    public struct OnServerRoomPlayerLeave {

    }
    public partial interface IRoomMatchSystem : ISystem {

        void ServerRoomPlayerJoinMatch(string name, NetworkConnection conn);
      
        void ServerRoomPlayerLeaveMatch(int id);

        void ServerRoomPlayerChangeTeam(int id);

        PlayerMatchInfo ServerGetHostInfo();

        bool ServerGetIsTeamSizeEqual();
        void ServerRoomPlayerChangeReadyState(int id, bool isReady);

        [Command]
        void CmdRequestKickPlayer(int id, NetworkIdentity requester);

        void ChangeRoomHost(NetworkIdentity identity);
    }
    public partial class RoomMatchSystem : AbstractNetworkedSystem, IRoomMatchSystem {
        private NetworkIdentity host;
        private void Awake() {
            Mikrocosmos.Interface.RegisterSystem<IRoomMatchSystem>(this);
            Debug.Log("System registered");
        }

     
        #region Server
        private List<PlayerMatchInfo> playerMatchInfos = new List<PlayerMatchInfo>();
        private Dictionary<int, NetworkIdentity> playerIdentities = new Dictionary<int, NetworkIdentity>();

        private Dictionary<int, List<PlayerMatchInfo>> teamPlayers = new Dictionary<int, List<PlayerMatchInfo>>();

        private int maxId = 0;

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnServerStartGame>(OnServerStartGame);
        }

        private void OnServerStartGame(OnServerStartGame e) {
            Debug.Log("OnServerStartGameAddingTeamPlayers");
            teamPlayers = new Dictionary<int, List<PlayerMatchInfo>>();
            teamPlayers.Add(1, new List<PlayerMatchInfo>());
            teamPlayers.Add(2, new List<PlayerMatchInfo>());

            foreach (PlayerMatchInfo playerMatchInfo in playerMatchInfos) {
                teamPlayers[playerMatchInfo.Team].Add(playerMatchInfo);
            }
        }

        public override void OnStopServer() {
            base.OnStopServer();
            teamPlayers.Clear();
            this.UnRegisterEvent<OnServerStartGame>(OnServerStartGame);
            Destroy(this.gameObject);
        }

        [ServerCallback]
        public void ServerRoomPlayerJoinMatch(string name, NetworkConnection conn) {
            playerIdentities.Add(maxId, conn.identity);
            PlayerMatchInfo matchInfo = new PlayerMatchInfo()
                {ID = maxId, Name = name, Team = GetNewTeamNum(), Prepared = false, Identity = conn.identity};
            maxId++;
            
            playerMatchInfos.Add(matchInfo);
            
            this.SendEvent<OnMatchInfoSet>(new OnMatchInfoSet(){Connection = conn, MatchInfo = matchInfo});
            this.SendEvent<OnRoomMemberChange>(new OnRoomMemberChange() {
                MatchInfos = playerMatchInfos,
                Host = host
            });
            Debug.Log("Room member change");
        }

      
        public void ServerRoomPlayerLeaveMatch(int id) {
            PlayerMatchInfo infoToRemove = FindPlayerByID(id);
            
            playerMatchInfos.Remove(infoToRemove);
            this.SendEvent<OnRoomMemberChange>(new OnRoomMemberChange() {
                MatchInfos = playerMatchInfos,
                Host = host
            });
            playerIdentities.Remove(id);
        }

        public void ServerRoomPlayerChangeTeam(int id) {
            PlayerMatchInfo player = FindPlayerByID(id);
            player.Team = player.Team == 1 ? 2 : 1;
           
            this.SendEvent<OnRoomMemberChange>(new OnRoomMemberChange() {
                MatchInfos = playerMatchInfos,
                Host = host
            });
        }

        [ServerCallback]
        public PlayerMatchInfo ServerGetHostInfo() {
            foreach (KeyValuePair<int, NetworkIdentity> identity in playerIdentities) {
                if (identity.Value == host) {
                    return playerMatchInfos[identity.Key];
                }
            }

            return null;
        }

        [ServerCallback]
        public bool ServerGetIsTeamSizeEqual() {
            int t1=0, t2=0;
            foreach (PlayerMatchInfo playerMatchInfo in playerMatchInfos) {
                if (playerMatchInfo.Team == 1) {
                    t1++;
                }
                else {
                    t2++;
                }
            }

            return t1 == t2;
        }

        public void ServerRoomPlayerChangeReadyState(int id, bool isReady) {
            PlayerMatchInfo player = FindPlayerByID(id);
            player.Prepared = isReady;
            this.SendEvent<OnRoomMemberChange>(new OnRoomMemberChange() {
                MatchInfos = playerMatchInfos,
                Host = host
            });
        }

        [Command(requiresAuthority = false)]
        public void CmdRequestKickPlayer(int id, NetworkIdentity requester) {
            //Debug.Log(requester.GetComponent<NetworkedMenuRoomPlayer>().matchInfo.ID);
            if (requester == host) {
                //PlayerMatchInfo player = FindPlayerByID(id);
                KickPlayer(playerIdentities[id]);
            }
        }

        [ServerCallback]
        public void KickPlayer(NetworkIdentity target) {
            target.connectionToClient.Disconnect();
        }

        [ServerCallback]
        public void ChangeRoomHost(NetworkIdentity identity) {
            host = identity;
            this.SendEvent<OnRoomMemberChange>(new OnRoomMemberChange()
            {
                MatchInfos = playerMatchInfos,
                Host = host
            });
        }

        [ServerCallback]
        private int GetNewTeamNum() {
            int team1Num = 0, team2Num = 0;
            foreach (PlayerMatchInfo playerMatchInfo in playerMatchInfos) {
                if (playerMatchInfo.Team == 1) {
                    team1Num++;
                }
                else {
                    team2Num++;
                }
            }

            if (team1Num != team2Num) {
                return team1Num > team2Num ? 2 : 1;
            }
            else {
                return Random.Range(1, 3);
            }
        }


        [ServerCallback]
        private PlayerMatchInfo FindPlayerByID(int id) {
            foreach (PlayerMatchInfo playerMatchInfo in playerMatchInfos)
            {
                if (playerMatchInfo.ID == id) {
                    return playerMatchInfo;
                }
            }

            return null;
        }
        #endregion
       


    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using MikroFramework.TimeSystem;
using Mirror;
using Polyglot;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public enum GameMode
    {
        Normal,
        Tutorial
    }
    [Serializable]
    public class PlayerMatchInfo : IEquatable<PlayerMatchInfo> {
        public int ID;
        public string Name;
        public int Team;
        public bool Prepared;
        public NetworkIdentity Identity;
        public int TeamIndex;
        public Language Language;
        public Avatar Avatar;
        public bool IsSpectator;

        public bool Equals(PlayerMatchInfo other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ID == other.ID && Name == other.Name && Team == other.Team && Prepared == other.Prepared && Equals(Identity, other.Identity) ;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlayerMatchInfo) obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID, Name, Team, Prepared, Identity);
        }

        public PlayerMatchInfo Clone() {
            return new PlayerMatchInfo() {
                ID = this.ID,
                Identity = this.Identity,
                Name = this.Name,
                Prepared = this.Prepared,
                Team = this.Team,
                Avatar = this.Avatar
            };
        }
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
        GameMode GameMode { get; }
        void ServerChangeGameMode(GameMode newGameMode);

        void ServerRoomPlayerJoinMatch(string name, Avatar avatar, Language language, NetworkConnection conn);
      
        void ServerRoomPlayerLeaveMatch(int id);

        void ServerRoomPlayerChangeTeam(int id);

        PlayerMatchInfo ServerGetHostInfo();

        bool ServerGetIsTeamSizeEqual();

        bool ServerGetIsStartGameConditionSatisfied();
        void ServerRoomPlayerChangeReadyState(int id, bool isReady);

        List<PlayerMatchInfo> ServerGetAllPlayerMatchInfoByTeamID(int team);

        int GetActivePlayerNumber();

        List<PlayerMatchInfo> ServerGetAllPlayerMatchInfo(bool includeSpectators);

        [Command]
        void CmdRequestKickPlayer(int id, NetworkIdentity requester);

        [Command]
        void CmdQuitRoom(NetworkIdentity requester);
        
        void ServerChangeSpectatorInGame(NetworkIdentity requester);
        
        void ServerReadyToEnterGameplayScene();

        void ChangeRoomHost(NetworkIdentity identity);

        PlayerMatchInfo ClientGetMatchInfoCopy();

        void ClientRecordMatchInfoCopy(PlayerMatchInfo matchInfo);
    }
    public partial class RoomMatchSystem : AbstractNetworkedSystem, IRoomMatchSystem {
        private NetworkIdentity host;

        [SyncVar(hook = nameof(RpcOnGameModeChange)), SerializeField]
        private GameMode gameMode = GameMode.Normal;

        public GameMode GameMode => gameMode;


        private void Awake() {
            DontDestroyOnLoad(gameObject);
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
            


            int t1Index = -1, t2Index = -1;

            foreach (PlayerMatchInfo playerMatchInfo in playerMatchInfos) {
                if (playerMatchInfo.Team == 1) {
                    t1Index++;
                    playerMatchInfo.TeamIndex = t1Index;
                }
                else {
                    t2Index++;
                    playerMatchInfo.TeamIndex = t2Index;
                }

                this.SendEvent<OnRoomMemberChange>();
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
        public void ServerChangeGameMode(GameMode newGameMode) {
            this.gameMode = newGameMode;
        }

        [ServerCallback]
        public void ServerRoomPlayerJoinMatch(string name, Avatar avatar, Language language, NetworkConnection conn) {
            playerIdentities.Add(maxId, conn.identity);

            PlayerMatchInfo matchInfo = new PlayerMatchInfo() {
                ID = maxId, Name = name, Team = GetNewTeamNum(), Prepared = false, Identity = conn.identity,
                Avatar = avatar,
                Language = language
            };
            
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
            if (infoToRemove!=null && teamPlayers.Keys.Count >= infoToRemove.Team) {
                teamPlayers[infoToRemove.Team].Remove(infoToRemove);
            }
          
        }

        public void ServerRoomPlayerChangeTeam(int id) {
            PlayerMatchInfo player = FindPlayerByID(id);
            player.Team = player.Team == 1 ? 2 : 1;
           
            this.SendEvent<OnRoomMemberChange>(new OnRoomMemberChange() {
                MatchInfos = playerMatchInfos,
                Host = host
            });
        }

    
        public void ServerChangeSpectatorInGame(NetworkIdentity requester) {
            PlayerMatchInfo matchInfo = requester.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().matchInfo;
            int team = matchInfo.Team;
            
            if (matchInfo != null) {
                matchInfo.IsSpectator = !matchInfo.IsSpectator;
                
                if (matchInfo.IsSpectator) {
                    //playerMatchInfos.Remove(matchInfo);
                    //playerIdentities.Remove(matchInfo.ID);
                    teamPlayers[team].Remove(matchInfo);
                }
                else {
                   // playerMatchInfos.Add(matchInfo);
                  //  playerIdentities.Add(matchInfo.ID, requester);
                    teamPlayers[team].Add(matchInfo);
                }
            }
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
                if (!playerMatchInfo.IsSpectator) {
                    if (playerMatchInfo.Team == 1)
                    {
                        t1++;
                    }
                    else
                    {
                        t2++;
                    }
                }
              
            }

            return t1 == t2;
        }

        public bool ServerGetIsStartGameConditionSatisfied() {
            int t1 = 0, t2 = 0;
            foreach (PlayerMatchInfo playerMatchInfo in playerMatchInfos)
            {
                if (playerMatchInfo.Team == 1)
                {
                    t1++;
                }
                else
                {
                    t2++;
                }
            }

            return Mathf.Abs(t1 - t2) <= 1;
        }

        public void ServerRoomPlayerChangeReadyState(int id, bool isReady) {
            PlayerMatchInfo player = FindPlayerByID(id);
            player.Prepared = isReady;
            this.SendEvent<OnRoomMemberChange>(new OnRoomMemberChange() {
                MatchInfos = playerMatchInfos,
                Host = host
            });
        }

        public List<PlayerMatchInfo> ServerGetAllPlayerMatchInfoByTeamID(int team) {
            return teamPlayers[team];
        }

        public int GetActivePlayerNumber() {
            return ServerGetAllPlayerMatchInfo(false).Count();
        }

        public List<PlayerMatchInfo> ServerGetAllPlayerMatchInfo(bool includeSpectators) {
            if (!includeSpectators) {
                return playerMatchInfos.Where((info => !info.IsSpectator)).ToList();
            }
            return playerMatchInfos;
        }

        [Command(requiresAuthority = false)]
        public void CmdRequestKickPlayer(int id, NetworkIdentity requester) {
            //Debug.Log(requester.GetComponent<NetworkedMenuRoomPlayer>().matchInfo.ID);
            if (requester == host) {
                //PlayerMatchInfo player = FindPlayerByID(id);
                KickPlayer(playerIdentities[id]);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdQuitRoom(NetworkIdentity requester) {
            if (NetworkServer.active && requester.hasAuthority) {
                NetworkRoomManager.singleton.StopHost();
            }
            requester.connectionToClient.Disconnect();
        }

       

        [ServerCallback]
        public void ServerReadyToEnterGameplayScene() {
            ((NetworkedRoomManager)NetworkRoomManager.singleton).ServerChangeGameModeScene(GameMode);
            RpcReadyToEnterGameScene();

            this.GetSystem<ITimeSystem>().AddDelayTask(4, () => {
                NetworkRoomManager.singleton.ServerChangeScene(((NetworkRoomManager) NetworkRoomManager.singleton)
                    .GameplayScene);
                this.SendEvent<OnServerStartGame>();
            });
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

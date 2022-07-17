using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;


namespace Mikrocosmos
{

    public struct OnClientReceiveMessage {
        public string Message;
        public string Name;
        public Avatar avatar;
        public int Team;
    }
    public struct OnLogMessage {
        public string message;
    }

    public interface ICommandSystem: ISystem {
        void CmdRequestAddMoneyCommand(NetworkIdentity player, int money, string targetName);
        void CmdGiveGameManager(NetworkIdentity requester, string playerName, bool isManager);

        void CmdRequestAddBuff(NetworkIdentity requester, string playerName, int buffID, int level, string commandName);

        void CmdSendChatMessage(NetworkIdentity requester, string message);
    }
  public class CommandSystem : AbstractNetworkedSystem, ICommandSystem {

   
      private HashSet<NetworkIdentity> gameManagers = new HashSet<NetworkIdentity>();

      [SerializeField] private List<NetworkIdentity> allPlayers = new List<NetworkIdentity>();

      

      private void Awake() {
          Application.logMessageReceived += OnLogMessage;
          Mikrocosmos.Interface.RegisterSystem<ICommandSystem>(this);
      }

      public override void OnStartServer() {
          base.OnStartServer();
          this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnPlayerConnected)
              .UnRegisterWhenGameObjectDestroyed(gameObject);
        
      }

      [ServerCallback]
      private void OnPlayerConnected(OnNetworkedMainGamePlayerConnected e) {
          if (e.connection.identity) {
                if (!allPlayers.Contains(e.connection.identity)) {
                    allPlayers.Add(e.connection.identity);
                }

                if (e.connection.identity.hasAuthority) {
                    gameManagers.Add(e.connection.identity);
                }
            }
      }

      private void OnLogMessage(string condition, string stacktrace, LogType type) {
          if (type == LogType.Exception || type == LogType.Error) {
              string message = $"<color=red>condition = {condition}\nstackTrace = {stacktrace}\ntype ={type}</color>";
              this.SendEvent<OnLogMessage>(new OnLogMessage() {message = message});
          }
      }

      [ServerCallback]
      private bool CheckPlayerIsManager(NetworkIdentity player, string commandName) {
          bool success = gameManagers.Contains(player);
          if (!success) {
              foreach (NetworkIdentity networkIdentity in gameManagers) {
                  TargetGetLogMessage(networkIdentity.connectionToClient, $"<b><color=orange>Player {player.GetComponent<NetworkMainGamePlayer>().matchInfo.Name} " +
                                                                          $"tries to execute: {commandName} but failed because they don't have authority!</color></b>");
              }
          }
          else {
              foreach (NetworkIdentity networkIdentity in gameManagers) {
                  if (networkIdentity != player) {
                      TargetGetLogMessage(networkIdentity.connectionToClient, $"<color=orange>Player {player.GetComponent<NetworkMainGamePlayer>().matchInfo.Name} " +
                                                                              $"tries to execute command: {commandName}</color>");
                    }
                 
              }
          }
          return success;
      }

      [TargetRpc]
      private void TargetGetLogMessage(NetworkConnection conn, string message) {
          this.SendEvent<OnLogMessage>(new OnLogMessage() {message = message});
      }

      [ClientRpc]
      private void RpcGetLogMessage(string message)
      {
          this.SendEvent<OnLogMessage>(new OnLogMessage() { message = message });
      }

      private List<PlayerMatchInfo> FindAllPlayersWithName(string name) {
          List<PlayerMatchInfo> matchInfos = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfo();
          List<PlayerMatchInfo> allPlayersWithTheName = matchInfos.FindAll(x => x.Name == name);
          return allPlayersWithTheName;
      }

        #region NetworkCommands
        [Command(requiresAuthority = false)]
        public void CmdRequestAddMoneyCommand(NetworkIdentity player, int money, string targetName) {
            if (CheckPlayerIsManager(player, "addMoney")) {

                List<PlayerMatchInfo> allPlayersWithTheName = FindAllPlayersWithName(targetName);
                if (allPlayersWithTheName.Count == 0) {
                    TargetGetLogMessage(player.connectionToClient, "<color=red>Failed to execute this command: no such player exists</color>");
                    return;
                }

                
                foreach (PlayerMatchInfo info in allPlayersWithTheName) {
                     info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship
                        .GetComponent<IPlayerTradingSystem>().ReceiveMoney(money);

                     RpcGetLogMessage($"<color=green>{info.Name} added {money} money</color>");
                }
            }
            else {
                TargetGetLogMessage(player.connectionToClient, "<color=red>Failed to execute this command: no authority</color>");
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdGiveGameManager(NetworkIdentity requester, string playerName, bool isManager) {
            if (CheckPlayerIsManager(requester, "gameManager")) {

                List<PlayerMatchInfo> allPlayersWithTheName = FindAllPlayersWithName(playerName);

                if (allPlayersWithTheName.Count == 0) {
                    TargetGetLogMessage(requester.connectionToClient, "<color=red>Failed to execute this command: no such player exists</color>");
                    return;
                }
                
                foreach (PlayerMatchInfo info in allPlayersWithTheName) {
                    if (isManager) {
                        gameManagers.Add(info.Identity.connectionToClient.identity);
                        RpcGetLogMessage("<color=green>Player " + playerName + " is now a game manager</color>");
                    }
                    else {
                        if (info.Identity.hasAuthority) {
                            TargetGetLogMessage(requester.connectionToClient, "<color=red>Failed to execute this command: you can't remove the host from the game manager list!</color>");
                            return;
                        }
                        gameManagers.Remove(info.Identity.connectionToClient.identity);
                        RpcGetLogMessage("<color=green>Player " + playerName + " is no longer a game manager</color>");
                    }
                }
            }
            else {
                TargetGetLogMessage(requester.connectionToClient, "<color=red>Failed to execute this command: no authority</color>");
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdRequestAddBuff(NetworkIdentity requester, string playerName, int buffID, int level,
            string commandName) {
            List<PlayerMatchInfo> allPlayersWithTheName = FindAllPlayersWithName(playerName);
            if (CheckPlayerIsManager(requester, commandName)) {
                if (allPlayersWithTheName.Count == 0) {
                    TargetGetLogMessage(requester.connectionToClient, "<color=red>Failed to execute this command: no such player exists</color>");
                    return;
                }

                if (buffID < 0 || buffID >= Enum.GetValues(typeof(PermanentBuffType)).Length) {
                    TargetGetLogMessage(requester.connectionToClient, "<color=red>Failed to execute this command: invalid buff ID</color>");
                    return;
                }

                foreach (PlayerMatchInfo info in allPlayersWithTheName) {
                    IBuffSystem buffSystem = info.Identity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship
                        .GetComponent<IBuffSystem>();
                    if (level >= 0) {
                        PermanentBuffFactory.AddPermanentBuffToPlayer((PermanentBuffType)buffID, buffSystem, level);
                    }
                    else {
                        PermanentBuffFactory.ReducePermanentBuffForPlayer((PermanentBuffType)buffID, buffSystem, -level);
                    }
                    
                    RpcGetLogMessage("<color=green>Player " + playerName + $" added level {level} to buff {(PermanentBuffType)buffID}" + "</color>");
                }
            }
            else {
                TargetGetLogMessage(requester.connectionToClient, "<color=red>Failed to execute this command: no authority</color>");
            }
        }



        #endregion


        #region Chat
        [Command(requiresAuthority = false)]
        public void CmdSendChatMessage(NetworkIdentity requester, string message) {
            PlayerMatchInfo matchInfo = requester.GetComponent<NetworkMainGamePlayer>().matchInfo;
            Avatar avatar = matchInfo.Avatar;
            string name = matchInfo.Name;
            int team = matchInfo.Team;
            RpcReceiveChatMessage(message, name, team, avatar);
        }


        [ClientRpc]
        private void RpcReceiveChatMessage(string message, string name, int team, Avatar avatar) {
            this.SendEvent<OnClientReceiveMessage>(new OnClientReceiveMessage() {
                avatar = avatar,
                Message = message,
                Name = name,
                Team = team
            });
        }


        #endregion


    }
}

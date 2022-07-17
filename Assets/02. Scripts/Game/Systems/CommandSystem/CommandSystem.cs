using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;


namespace Mikrocosmos
{
    public struct OnLogMessage {
        public string message;
    }

    public interface ICommandSystem: ISystem {
        void CmdRequestAddMoneyCommand(NetworkIdentity player, int money);
        void CmdGiveGameManager(NetworkIdentity requester, string playerName, bool isManager);
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

        #region NetworkCommands
        [Command(requiresAuthority = false)]
        public void CmdRequestAddMoneyCommand(NetworkIdentity player, int money) {
            if (CheckPlayerIsManager(player, "addMoney")) {
                PlayerMatchInfo playerInfo = player.GetComponent<NetworkMainGamePlayer>().matchInfo;
                player.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.GetComponent<IPlayerTradingSystem>()
                    .ReceiveMoney(money);
                RpcGetLogMessage($"<color=green>{playerInfo.Name} added {money} money</color>");
            }
            else {
                TargetGetLogMessage(player.connectionToClient, "Failed to execute this command: no authority");
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdGiveGameManager(NetworkIdentity requester, string playerName, bool isManager) {
            if (CheckPlayerIsManager(requester, "gameManager")) {
                List<PlayerMatchInfo> matchInfos = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfo();
                List<PlayerMatchInfo> allPlayersWithTheName = matchInfos.FindAll(x => x.Name == playerName);

                if (allPlayersWithTheName.Count == 0) {
                    TargetGetLogMessage(requester.connectionToClient, "Failed to execute this command: no such player exists");
                    return;
                }
                
                foreach (PlayerMatchInfo info in allPlayersWithTheName) {
                    if (isManager) {
                        gameManagers.Add(info.Identity.connectionToClient.identity);
                        RpcGetLogMessage("<color=green>Player " + playerName + " is now a game manager</color>");
                    }
                    else {
                        if (info.Identity.hasAuthority) {
                            TargetGetLogMessage(requester.connectionToClient, "Failed to execute this command: you can't remove the host from the game manager list!");
                            return;
                        }
                        gameManagers.Remove(info.Identity.connectionToClient.identity);
                        RpcGetLogMessage("<color=green>Player " + playerName + " is no longer a game manager</color>");
                    }
                }
            }
            else {
                TargetGetLogMessage(requester.connectionToClient, "Failed to execute this command: no authority");
            }
        }

        #endregion

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnRoomPlayerJoinGame {
        public PlayerMatchInfo MatchInfo;
        public NetworkConnection Connection;
        public Language ClientLanguage;
    }
    public partial class NetworkedMenuRoomPlayer : NetworkRoomPlayer, IController, ICanSendEvent {
        [SerializeField]
        private PlayerMatchInfo matchInfo = null;

        private Language clientLanguage;
        
        public PlayerMatchInfo MatchInfo {
            get => matchInfo;
            set => matchInfo = value;
        }

        #region Server
        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log("Server Start");
            this.RegisterEvent<OnMatchInfoSet>(OnMatchInfoSet).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnRoomMemberChange>(ServerOnRoomMemberChange).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnNetworkedMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnNetworkedMainGamePlayerConnected(OnNetworkedMainGamePlayerConnected obj) {
            if (obj.connection == connectionToClient) {
                this.SendEvent<OnRoomPlayerJoinGame>(new OnRoomPlayerJoinGame() {
                    Connection = obj.connection,
                    MatchInfo = matchInfo,
                    ClientLanguage = clientLanguage
                });
            }
        }
        
        public override void OnStopServer() {
            base.OnStopServer();
            this.GetSystem<IRoomMatchSystem>().ServerRoomPlayerLeaveMatch(matchInfo.ID);
        }

        [ServerCallback]
        private void ServerJoinMatch(string name) {
            
            this.GetSystem<IRoomMatchSystem>().ServerRoomPlayerJoinMatch(name, connectionToClient);
        }

        [Command] 
        private void CmdJoinMatch(string name) {
           Debug.Log("CMD Join Match");
           ServerJoinMatch(name);
        }

        [Command]
        private void CmdSwitchTeam() {
            this.GetSystem<IRoomMatchSystem>().ServerRoomPlayerChangeTeam(matchInfo.ID);
        }

        [Command]
        private void CmdUpdateReady( bool ready) {
            this.GetSystem<IRoomMatchSystem>().ServerRoomPlayerChangeReadyState(matchInfo.ID, ready);
        }

        
        private void ServerOnRoomMemberChange(OnRoomMemberChange e) {
            Debug.Log("Server on room member change");
            if (NetworkServer.active) {
                //e.MatchInfos.Find(m => m.ID ==)
                //this.matchInfo = e.MatchInfos;
                TargetOnRoomMemberChange(e.MatchInfos, matchInfo, e.Host == netIdentity);
            }
         
        }

        
        private void OnMatchInfoSet(OnMatchInfoSet obj) {
            if (obj.Connection == connectionToClient) {
                matchInfo = obj.MatchInfo;
                
                Debug.Log($"Match info changed: Team: {obj.MatchInfo.Team}");
            }
        }

        [Command]
        private void CmdSetLanguage(Language language) {
            this.clientLanguage = language;
        }

      
        #endregion


      
        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}

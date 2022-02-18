using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public partial class NetworkedMenuRoomPlayer : NetworkRoomPlayer, IController {
       // [SyncVar]
        private PlayerMatchInfo matchInfo = null;

        #region Server
       
        
        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log("Server Start");
            this.RegisterEvent<OnMatchInfoSet>(OnMatchInfoSet).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnRoomMemberChange>(ServerOnRoomMemberChange).UnRegisterWhenGameObjectDestroyed(gameObject);
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
            TargetOnRoomMemberChange(e.MatchInfos,  matchInfo ,e.Host == netIdentity);
        }

        private void OnMatchInfoSet(OnMatchInfoSet obj) {
            if (obj.Connection == connectionToClient) {
                matchInfo = obj.MatchInfo;
                
                Debug.Log($"Match info changed: Team: {obj.MatchInfo.Team}");
            }
        }

      
        #endregion


      
        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}

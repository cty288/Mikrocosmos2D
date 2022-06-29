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
    public partial class NetworkedMenuRoomPlayer : NetworkRoomPlayer, IController
    {

        public override void OnStartAuthority()
        {

            this.RegisterEvent<OnClientRequestChangeTeam>(OnClientRequestChangeTeam)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientRequestPrepare>(OnClientRequestPrepare)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            CmdSetLanguage(Localization.Instance.SelectedLanguage);
            
            if (isServer) {
                CmdJoinMatch(this.GetModel<ILocalPlayerInfoModel>().NameInfo.Value);
            }
            else {
                StartCoroutine(DelayJoinMatch());
            }
            
            Debug.Log("On start authority");
        }

        IEnumerator DelayJoinMatch() {
            yield return new WaitForSeconds(0.3f);
            CmdJoinMatch(this.GetModel<ILocalPlayerInfoModel>().NameInfo.Value);
        }
        

        private void OnClientRequestPrepare(OnClientRequestPrepare obj) {
            if (NetworkClient.active && isLocalPlayer) {
                Debug.Log("Client request prepare");
                
                CmdChangeReadyState(!readyToBegin);
            }
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) {
            base.ReadyStateChanged(oldReadyState, newReadyState);
            if (isLocalPlayer) {
                CmdUpdateReady(readyToBegin);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (hasAuthority && isServer) {
                Debug.Log("I am the host");
                this.GetSystem<IRoomMatchSystem>().ChangeRoomHost(netIdentity);
            }
        }

        private void OnClientRequestChangeTeam(OnClientRequestChangeTeam obj) {
            CmdSwitchTeam();
        }

        [TargetRpc]
        private void TargetOnRoomMemberChange(List<PlayerMatchInfo> infos, PlayerMatchInfo selfInfo, bool isHost)
        {
            this.SendCommand<ChangePrepareRoomPlayerListCommand>(new ChangePrepareRoomPlayerListCommand(infos,
                selfInfo, isHost));
            Debug.Log(selfInfo.Team);
            this.GetSystem<IRoomMatchSystem>().ClientRecordMatchInfoCopy(selfInfo);
            // Debug.Log("Target room member change "+infos[1].ID);
        }

        public override void OnStopClient() {
            base.OnStopClient();
            Destroy(this.gameObject);
        }
    }
}

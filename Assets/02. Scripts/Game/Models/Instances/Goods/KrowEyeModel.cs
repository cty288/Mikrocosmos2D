using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class KrowEyeModel : BasicGoodsModel, ICanGetSystem {
        [SerializeField]
        protected List<NetworkIdentity> clientCanSee = new List<NetworkIdentity>();

        public List<NetworkIdentity> ClientCanSee => clientCanSee;

        private BindableProperty<int> teamBelongTo = new BindableProperty<int>(-1);

        public BindableProperty<int> TeamBelongTo => teamBelongTo;


        public override void OnStartServer() {
            base.OnStartServer();
            teamBelongTo.RegisterWithInitValue(OnTeamBelongToChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        

        [ServerCallback]

        protected override void OnServerItemBought(NetworkIdentity buyer) {
            base.OnServerItemBought(buyer);
            teamBelongTo.Value = buyer.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
        }

        protected override void OnServerHooked() {
            base.OnServerHooked();
            teamBelongTo.Value = HookedByIdentity.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>()
                .matchInfo.Team;
            Debug.Log("OnServerHooked");
        }

        [ServerCallback]
        private void OnTeamBelongToChanged(int oldTeam, int newTeam) {
            if (newTeam != -1) {
                StopVisionForAllCurrentTeams();
                StartVisionForAllTeamMembers(newTeam);

            }else {
                StopVisionForAllCurrentTeams();
            }
        }

        [ServerCallback]
        private void StopVisionForAllCurrentTeams() {
            foreach (NetworkIdentity identity in clientCanSee) {
                GetComponent<KrewEyeViewController>().ServerRemoveClient(identity);
            }
            clientCanSee.Clear();
        }

        [ServerCallback]
        private void StartVisionForAllTeamMembers(int team)
        {
            List<PlayerMatchInfo> allMatchInfos = this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(team);
            foreach (PlayerMatchInfo info in allMatchInfos) {
                clientCanSee.Add(info.Identity);
                GetComponent<KrewEyeViewController>().ServerAllowClientToSee(info.Identity);
            }
        }
    }
}

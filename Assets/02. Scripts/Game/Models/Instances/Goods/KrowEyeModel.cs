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

        [SerializeField] private float deactiveTimeAfterHooked = 10;

        public override void OnStartServer() {
            base.OnStartServer();
            teamBelongTo.RegisterWithInitValue(OnTeamBelongToChanged).UnRegisterWhenGameObjectDestroyed(gameObject, true);
        }

        

        [ServerCallback]

        protected override void OnServerItemBought(NetworkIdentity buyer) {
            base.OnServerItemBought(buyer);
            //Debug.Log("233333: "+ (buyer.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>() == null).ToString());
            teamBelongTo.Value = buyer.connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
        }

        public override void OnServerHooked() {
            base.OnServerHooked();
            if (teamBelongTo.Value == -1) {
                teamBelongTo.Value = HookedByIdentity.GetComponent<PlayerSpaceship>()
                    .matchInfo.Team;
                Debug.Log("OnServerHooked");
            }
            
        }

        [ServerCallback]
        private void OnTeamBelongToChanged(int oldTeam, int newTeam) {
            if (newTeam != -1) {
                StopVisionForAllCurrentTeams();
                StartVisionForAllTeamMembers(newTeam);
                StartCoroutine(DeactiveAfterHooked());

            }else {
                StopVisionForAllCurrentTeams();
            }
        }

        private IEnumerator DeactiveAfterHooked() {
            yield return new WaitForSeconds(deactiveTimeAfterHooked);
            TeamBelongTo.Value = -1;
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

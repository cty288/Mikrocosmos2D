using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using Polyglot;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public class KrewEyeViewController : BasicGoodsViewController, ICanCreateVisionViewController{
        [field: SerializeField, SyncVar(hook = nameof(OnTurnOnStateChanged))]
        public bool IsOn { get; set; }


        private StrangeMeteorTrigger spaceshipDetectTrigger;
        private KrowEyeModel krowEyeModel;

        private Light2DBase[] visionLights;

        private GameObject mapUI;

        private Animator animator;

        
        protected override void Awake() {
            base.Awake();
            visionLights = GetComponentsInChildren<Light2DBase>();
            foreach (Light2DBase visionLight in visionLights) {
                visionLight.enabled = false;
            }

            mapUI = transform.Find("MapUI").gameObject;
            mapUI.SetActive(false);
            krowEyeModel = GetComponent<KrowEyeModel>();
            animator = GetComponent<Animator>();
            spaceshipDetectTrigger = GetComponentInChildren<StrangeMeteorTrigger>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            //visionLights.enabled = IsOn;
            GetComponent<KrowEyeModel>().TeamBelongTo.RegisterWithInitValue(OnServerTeamChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnNetworkedMainGamePlayerConnected>(OnNetworkedMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            spaceshipDetectTrigger.OnPlayerEnterTrigger += OnSpaceshipEnterTrigger;

        }

        private void OnSpaceshipEnterTrigger(PlayerSpaceship e) {
            if (krowEyeModel.TeamBelongTo.Value != -1) {
                if (e.ThisSpaceshipTeam != krowEyeModel.TeamBelongTo.Value) {
                    if (!e.matchInfo.IsSpectator) {
                        List<PlayerMatchInfo> matchInfo =
                            this.GetSystem<IRoomMatchSystem>().ServerGetAllPlayerMatchInfoByTeamID(krowEyeModel.TeamBelongTo.Value);

                        List<NetworkConnectionToClient> connections = matchInfo.Select((info => {
                            return info.Identity.connectionToClient;
                        })).ToList();

                        foreach (NetworkConnectionToClient connection in connections)
                        {
                            TargetAlertTeamMembers(connection);
                        }

                        if (e.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem))
                        {
                            buffSystem.AddBuff<KrowEyeSpeedDownDeBuff>(new KrowEyeSpeedDownDeBuff(
                                UntilAction.Allocate(() => !spaceshipDetectTrigger.PlayersInTrigger.Contains(e))));
                        }
                    }
                }
            }
        }

        private void OnNetworkedMainGamePlayerConnected(OnNetworkedMainGamePlayerConnected e) {
            OnServerTeamChange(-1, GetComponent<KrowEyeModel>().TeamBelongTo.Value);
        }

        private void OnServerTeamChange(int oldTeam, int newTeam) {
            Debug.Log($"OnTeamChange: {newTeam}");
            RpcOnTeamChange(oldTeam, newTeam);
            /*
            if (newTeam == -1) {
                animator.SetTrigger("Idle");
            }else if (newTeam == 1) {
                animator.SetTrigger("Team1");
            }else if (newTeam == 2) {
                animator.SetTrigger("Team2");
            }*/
        }

        [ServerCallback]
        public void ServerTurnOn() {
            IsOn = true;
        }

        [ServerCallback]
        public void ServerTurnOff() {
            IsOn = false;
        }


        [ServerCallback]
        public void ServerAllowClientToSee(NetworkIdentity identity) {
            //GetComponent<KrowEyeModel>().ClientCanSee.Add(identity);
            TargetOpenVision(identity.connectionToClient);
        }

        public void ServerRemoveClient(NetworkIdentity identity) {
           // GetComponent<KrowEyeModel>().ClientCanSee.Remove(identity);
            TargetCloseVision(identity.connectionToClient);
        }



        #region Client

        [ClientRpc]
        private void RpcOnTeamChange(int oldTeam, int newTeam) {
            if (newTeam == -1) {
                animator.SetTrigger("Idle");
            } else if (oldTeam != newTeam) {
                int thisClientTeam = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>().matchInfo
                    .Team;
                mapUI.SetActive(thisClientTeam == newTeam);

                if (newTeam == 1) {
                    animator.SetTrigger("Team1");
                    mapUI.GetComponent<Animator>().SetTrigger("Team1");
                }
                if (newTeam == 2) {
                    animator.SetTrigger("Team2");
                    mapUI.GetComponent<Animator>().SetTrigger("Team2");
                }
            }
           
        }
        
        private bool isClientCanSee = false;

        private void Start() {
            if (isClient) {
                StartCoroutine(RefreshLight());
            }
        }

        private IEnumerator RefreshLight() {

            if (isClientCanSee && IsOn) {
                foreach (Light2DBase visionLight in visionLights)
                {
                    visionLight.enabled = false;
                }
                yield return new WaitForSeconds(0.1f);
                foreach (Light2DBase visionLight in visionLights)
                {
                    visionLight.enabled = true;
                }
            }
        }


        [TargetRpc]
        private void TargetOpenVision(NetworkConnection connection) {
            isClientCanSee = true;
            ClientUpdateLight();
        }
        [TargetRpc]
        private void TargetCloseVision(NetworkConnection connection) {
            isClientCanSee = false;
            ClientUpdateLight();
        }

        [ClientCallback]
        protected void ClientUpdateLight() {
            if (!isClientCanSee) {
                if (visionLights[0].enabled) {
                    foreach (Light2DBase visionLight in visionLights)
                    {
                        visionLight.enabled = false;
                    }
                }
                return;
            }
            foreach (Light2DBase visionLight in visionLights)
            {
                visionLight.enabled = IsOn;
            }
        }

        public void OnTurnOnStateChanged(bool lastIsTurnOn, bool currentIsTurnOn) {
            if (!isClientCanSee) return;
            ClientUpdateLight();
            if (currentIsTurnOn)
            {
                //OnClientTurnOn
            }
            else
            {
                //OnClientTurnOff
            }
        }
        #endregion

        [TargetRpc]
        private void TargetAlertTeamMembers(NetworkConnection conn) {
            this.GetSystem<IClientInfoSystem>().AddOrUpdateInfo(new ClientInfoMessage() {
                AutoDestroyWhenTimeUp = true,
                Description = "",
                Name = $"EyeAlert_{GetHashCode()}",
                RemainingTime = 6f,
                Title = Localization.Get("GAME_INFO_EYE_DETECT"),
                ShowRemainingTime = false,
                InfoElementPrefabAssetName = InfoElementPrefabNames.ICON_WARNING_NORMAL
            });
        }
    }
}

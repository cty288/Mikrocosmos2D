using System.Collections;
using System.Collections.Generic;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public class KrewEyeViewController : BasicGoodsViewController, ICanCreateVisionViewController{
        [field: SerializeField, SyncVar(hook = nameof(OnTurnOnStateChanged))]
        public bool IsOn { get; set; }

       

        private Light2DBase[] visionLights;
    

        private NetworkAnimator animator;
        protected override void Awake() {
            base.Awake();
            visionLights = GetComponentsInChildren<Light2DBase>();
            foreach (Light2DBase visionLight in visionLights) {
                visionLight.enabled = false;
            }
            
            animator = GetComponent<NetworkAnimator>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            //visionLights.enabled = IsOn;
            GetComponent<KrowEyeModel>().TeamBelongTo.RegisterWithInitValue(OnTeamChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnTeamChange(int oldTeam, int newTeam) {
            Debug.Log($"OnTeamChange: {newTeam}");
            if (newTeam == -1) {
                animator.SetTrigger("Idle");
            }else if (newTeam == 1) {
                animator.SetTrigger("Team1");
            }else if (newTeam == 2) {
                animator.SetTrigger("Team2");
            }
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
    }
}

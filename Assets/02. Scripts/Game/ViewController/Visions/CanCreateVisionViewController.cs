using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public interface ICanCreateVisionViewController {
        bool IsOn { get; set; }

        void ServerTurnOn();
        void ServerTurnOff();
        void ServerAllowClientToSee(NetworkIdentity identity);

        void ServerRemoveClient(NetworkIdentity identity);

    }
    
    //TODO: Star add script
    public abstract class CanCreateVisionViewController : AbstractNetworkedController<Mikrocosmos>, ICanCreateVisionViewController {

        #region Server

        [field: SerializeField, SyncVar(hook = nameof(OnTurnOnStateChanged))]
        public bool IsOn { get; set; } = false;

        [SerializeField]
        protected List<NetworkIdentity> clientCanSee = new List<NetworkIdentity>();

        private Light2DBase visionLight;

        protected virtual void Awake() {
            visionLight = GetComponentInChildren<Light2DBase>();
            visionLight.enabled = true;
        }

        public override void OnStartServer() {
            base.OnStartServer();
            visionLight.enabled = IsOn;
        }


        public void ServerTurnOn() {
            IsOn = true;
        }

        public void ServerTurnOff() {
            IsOn = false;
        }

        [ServerCallback]
        public void ServerAllowClientToSee(NetworkIdentity identity) {
            clientCanSee.Add(identity);
            TargetOpenVision(identity.connectionToClient);
        }


        [ServerCallback]
        public void ServerRemoveClient(NetworkIdentity identity) {
            clientCanSee.Remove(identity);
            TargetCloseVision(identity.connectionToClient);
        }

      


        #endregion


        #region Client
        private bool isClientCanSee = false;
        [SerializeField] private float ClientRefreshRate = 2f;

        private void Start() {
            if (isClient) {
                //StartCoroutine(RefreshLight());
            }
        }

        private IEnumerator RefreshLight() {
            while (true) {
                if (isClientCanSee && IsOn) {
                    visionLight.enabled = false;
                    yield return new WaitForSeconds(0.01f);
                    visionLight.enabled = true;
                }
                yield return new WaitForSeconds(ClientRefreshRate);
            }
        }

        public override void OnStartAuthority() {
            base.OnStartAuthority();
            visionLight.enabled = false;
            StartCoroutine(StartChangeLight());
        }

        private IEnumerator StartChangeLight() {
            yield return new WaitForSeconds(0.5f);
            visionLight.enabled = IsOn;
        }


        [TargetRpc]
        private void TargetOpenVision(NetworkConnection connection)
        {
            isClientCanSee = true;
            ClientUpdateLight();
        }
        [TargetRpc]
        private void TargetCloseVision(NetworkConnection connection)
        {
            isClientCanSee = false;
            ClientUpdateLight();
        }


        [ClientCallback]
        protected void ClientUpdateLight()
        {
            if (!isClientCanSee)
            {
                if (visionLight.enabled)
                {
                    visionLight.enabled = false;
                    OnClientVisionLightTurnOff();
                }
                return;
            }

            visionLight.enabled = IsOn;
        }

        public void OnTurnOnStateChanged(bool lastIsTurnOn, bool currentIsTurnOn)
        {
            if (!isClientCanSee) return;
            ClientUpdateLight();
            if (currentIsTurnOn)
            {
                OnClientVisionLightTurnOn();
            }
            else
            {
                OnClientVisionLightTurnOff();
            }
        }

      //  [ClientCallback]
        protected abstract void OnClientVisionLightTurnOff();

       // [ClientCallback]
        protected abstract void OnClientVisionLightTurnOn();


        #endregion

    }
}

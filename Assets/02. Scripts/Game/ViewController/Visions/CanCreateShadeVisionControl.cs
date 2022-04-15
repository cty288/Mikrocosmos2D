using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{
    public interface ICanCreateShadeVisionControllor { 
        bool IsMaskable { get; set; }

        void ServerSetClientAlwaysUnMaskable(NetworkConnectionToClient connection);

        void ServerTurnOffClientAlwaysUnMaskable(NetworkConnectionToClient connection);
    }


    public  class CanCreateShadeVisionControl : AbstractNetworkedController<Mikrocosmos>, ICanCreateShadeVisionControllor {
        [field: SyncVar(hook = nameof(OnMaskableChanged)), SerializeField]
        public bool IsMaskable { get; set; } = true;

        private ShadowCaster2D shaderCaster;

        private void Awake() {
            shaderCaster = GetComponent<ShadowCaster2D>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            shaderCaster.enabled = IsMaskable;
            
        }

       

        [ServerCallback]
        public void ServerSetClientAlwaysUnMaskable(NetworkConnectionToClient connection) {
            TargetClientAlwaysUnMaskable(connection);
        }

        [ServerCallback]
        public void ServerTurnOffClientAlwaysUnMaskable(NetworkConnectionToClient connection) {
            TargetClientTurnOffAlwaysUnMaskable(connection);
        }












        protected bool clientAlwaysUnmaskable = false;
        public override void OnStartClient() {
            base.OnStartClient();
            shaderCaster.enabled = IsMaskable;
        }


        private void OnMaskableChanged(bool oldMaskable, bool currentMaskable) {
            shaderCaster.enabled = currentMaskable;
            if (currentMaskable) {
                if (!clientAlwaysUnmaskable) {
                    OnClientMaskable();
                }
            }
            else {
                if (!clientAlwaysUnmaskable) {
                    OnClientUnMaskable();
                }
            }
        }

        [TargetRpc]
        public void TargetClientAlwaysUnMaskable(NetworkConnection connection) {
            clientAlwaysUnmaskable = true;
           
            if (shaderCaster.enabled) {
                shaderCaster.enabled = false;
                OnClientUnMaskable();
            }
        }

        [TargetRpc]
        public void TargetClientTurnOffAlwaysUnMaskable(NetworkConnection connection)
        {
            clientAlwaysUnmaskable = false;

            if (!shaderCaster.enabled && !IsMaskable) {
                shaderCaster.enabled = true;
                OnClientMaskable();
            }
        }
        protected virtual void OnClientMaskable(){}

        protected virtual void OnClientUnMaskable(){}
    }
}

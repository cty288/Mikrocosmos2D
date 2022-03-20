using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class VisionLightExample : CanCreateVisionViewController
    {


        #region Server

        public override void OnStartServer() {
            base.OnStartServer();
            StartCoroutine(LightSwitch());
        }

        private IEnumerator LightSwitch() {
            while (true) {
                yield return new WaitForSeconds(5f);
                IsOn = !IsOn;
            }
        }


        private void Update() {
            if (isServer) {
                if (Input.GetMouseButtonDown(1)) {
                    if (clientCanSee.Count > 0) {
                        ServerRemoveClient(clientCanSee[Random.Range(0, clientCanSee.Count)]);
                    }
                }
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdAddToLightList(NetworkIdentity identity) {
            if (Random.Range(0, 100) >= 50) {
                ServerAllowClientToSee(identity);
            }
        }
        

        #endregion

        #region Client

      

        
        protected override void OnClientVisionLightTurnOff()
        {

        }

        protected override void OnClientVisionLightTurnOn() {

        }

        public override void OnStartClient() {
            base.OnStartClient();
            
            CmdAddToLightList(NetworkClient.connection.identity);
            Debug.Log("Vision Light Start Client");
        }

        #endregion

    }
}

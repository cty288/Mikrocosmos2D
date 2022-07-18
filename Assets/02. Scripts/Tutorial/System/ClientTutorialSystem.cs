using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Mikrocosmos
{
    public class ClientTutorialSystem : MonoBehaviour , ICanSendEvent, ICanRegisterEvent{
        private void Awake() {
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e) {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority) {
                this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange() {
                    InnerAddition = 12,
                    OuterAddition = 25
                });
            }
        }

        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}

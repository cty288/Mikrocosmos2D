using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class GameCamera : AbstractMikroController<Mikrocosmos> {
        [SerializeField]
        public GameObject following;

        [SerializeField] private float lerp = 0.1f;

        private CinemachineTargetGroup cinemachineTargetGroup;
        private void Awake() {
            cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCameraViewChange>(OnCameraViewChange).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnCameraViewChange(OnCameraViewChange e) {
            cinemachineTargetGroup.m_Targets[0].radius = e.NewRadius;
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e) {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority) {
                AddFollowingPlayer(e.playerSpaceship.transform, true);
            }
                
        }

        public void AddFollowingPlayer(Transform followingObj, bool isLocalPlayer ) {
            cinemachineTargetGroup.AddMember(followingObj, isLocalPlayer ? 3 : 1, 20);
        }
    }
}

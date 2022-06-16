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

        private int minCameraRadius = 25;

        private int currentMinCameraRadius;

        private CinemachineTargetGroup cinemachineTargetGroup;
        private void Awake() {
            cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCameraViewChange>(OnCameraViewChange).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnVisionPermanentChange>(OnVisionPermanentChange).UnRegisterWhenGameObjectDestroyed(gameObject);

            currentMinCameraRadius = minCameraRadius;
        }

        private void OnVisionPermanentChange(OnVisionPermanentChange e) {
            currentMinCameraRadius += (int) (minCameraRadius * e.IncreasePercentage);
            cinemachineTargetGroup.m_Targets[0].radius = Mathf.Max(currentMinCameraRadius, cinemachineTargetGroup.m_Targets[0].radius);
        }

        private void OnCameraViewChange(OnCameraViewChange e) {
            cinemachineTargetGroup.m_Targets[0].radius = currentMinCameraRadius + e.RadiusAddition;
            cinemachineTargetGroup.m_Targets[0].radius = Mathf.Max(currentMinCameraRadius, cinemachineTargetGroup.m_Targets[0].radius);
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e) {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority) {
                AddFollowingPlayer(e.playerSpaceship.transform, true);
            }
                
        }

        public void AddFollowingPlayer(Transform followingObj, bool isLocalPlayer ) {
            cinemachineTargetGroup.AddMember(followingObj, isLocalPlayer ? 3 : 1, 25);
        }
    }
}

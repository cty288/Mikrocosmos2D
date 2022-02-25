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
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e) {
            AddFollowingPlayer(e.playerSpaceship.transform, true);
        }

        public void AddFollowingPlayer(Transform followingObj, bool isLocalPlayer ) {
            cinemachineTargetGroup.AddMember(followingObj, isLocalPlayer ? 3 : 1, 10);
        }
    }
}

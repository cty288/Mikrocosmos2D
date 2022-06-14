using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class MinimapCamera : AbstractMikroController<Mikrocosmos>
    {
        //[SerializeField]
        private GameObject following;

        [SerializeField] private float lerp = 0.1f;

        private float minMainCameraRange = 25;

        private Camera camera;
        private void Awake() {
            camera = GetComponent<Camera>();

           
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCameraViewChange>(OnCameraViewChange).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnCameraViewChange(OnCameraViewChange e) {
            float targetRadius = e.RadiusAddition * 2.04f;

            DOTween.To(() => camera.orthographicSize, x => camera.orthographicSize = x,  Mathf.Max(minMainCameraRange * 2.04f, camera.orthographicSize + targetRadius), 0.3f);
            //cinemachineTargetGroup.m_Targets[0].radius = e.NewRadius;
        }

        private void FixedUpdate() {
            if (following) {
                Vector3 targetPos = new Vector3(following.transform.position.x, following.transform.position.y, -10);
                transform.position = Vector3.Lerp(transform.position, targetPos,
                    lerp * Time.fixedDeltaTime);
            }
           
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e)
        {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority) {
                following = e.playerSpaceship;
            }

        }

     
    }
}

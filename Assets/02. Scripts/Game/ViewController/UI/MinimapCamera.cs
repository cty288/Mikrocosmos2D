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
        private float currentMinCameraRadius;

        private Camera camera;
        private void Awake() {
            camera = GetComponent<Camera>();

           
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCameraViewChange>(OnCameraViewChange).UnRegisterWhenGameObjectDestroyed(gameObject);

            currentMinCameraRadius = minMainCameraRange;

            this.RegisterEvent<OnVisionPermanentChange>(OnVisionPermanentChange).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnVisionPermanentChange(OnVisionPermanentChange e) {
            currentMinCameraRadius += minMainCameraRange * e.IncreasePercentage;
           

            DOTween.To(() => camera.orthographicSize, x => camera.orthographicSize = x, Mathf.Max(currentMinCameraRadius * 2.5f, camera.orthographicSize), 0.3f);
        }

        private void OnCameraViewChange(OnCameraViewChange e) {
            float targetRadius = e.RadiusAddition * 2.5f;

            DOTween.To(() => camera.orthographicSize, x => camera.orthographicSize = x,  Mathf.Max(currentMinCameraRadius * 2.5f, camera.orthographicSize + targetRadius), 0.3f);
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

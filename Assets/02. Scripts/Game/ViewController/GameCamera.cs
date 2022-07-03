using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct ShakeCamera {
        public float Duration;
        public float Strength;
        public int Viberato;
    }
    public class GameCamera : AbstractMikroController<Mikrocosmos>, ISingleton {
      
        private GameObject following;

        [SerializeField] private float lerp = 0.1f;

        private int minCameraRadius = 32;

        private int currentMinCameraRadius;

        private Camera camera;

        
        
        private void Awake() {
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCameraViewChange>(OnCameraViewChange).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnVisionPermanentChange>(OnVisionPermanentChange).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<ShakeCamera>(OnShakeCamera).UnRegisterWhenGameObjectDestroyed(gameObject);
            currentMinCameraRadius = minCameraRadius;
            camera = GetComponentInChildren<Camera>();
         
        }

        public void OnShakeCamera(ShakeCamera e) {
            
           
            //lerp = 20;
            camera.DOShakePosition(e.Duration, e.Strength, e.Viberato, 100f).OnComplete((() => {

            }));
         
        }

        
        private void LateUpdate() {
            var position = following.transform.position;
            Vector3 targetPos = new Vector3(position.x, position.y, -10);
            
            transform.position = Vector3.Lerp(transform.position, targetPos, lerp * Time.deltaTime);
            
            //transform.position = targetPos;
            
        }

        private void OnVisionPermanentChange(OnVisionPermanentChange e) {
            currentMinCameraRadius += (int) (minCameraRadius * e.IncreasePercentage);
            camera.DOOrthoSize(Mathf.Max(currentMinCameraRadius, camera.orthographicSize), 0.1f);
            //cinemachineTargetGroup.m_Targets[0].radius = 
        }

        

        private void OnCameraViewChange(OnCameraViewChange e) {
            float targetRadius = currentMinCameraRadius + e.RadiusAddition;
            camera.DOOrthoSize(Mathf.Max(currentMinCameraRadius, targetRadius), 0.5f);
        //    cinemachineTargetGroup.m_Targets[0].radius = Mathf.Max(currentMinCameraRadius, cinemachineTargetGroup.m_Targets[0].radius);
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e) {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority) {
                AddFollowingPlayer(e.playerSpaceship.transform, true);
            }
                
        }

        public void AddFollowingPlayer(Transform followingObj, bool isLocalPlayer ) {
            following = followingObj.gameObject;
        }

        public static GameCamera Singleton {
            get {
                return SingletonProperty<GameCamera>.Singleton;
            }
        }
        public void OnSingletonInit() {
            
        }
    }
}

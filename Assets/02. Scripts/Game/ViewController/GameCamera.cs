using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using MikroFramework.TimeSystem;
using Mirror;
using Steamworks;
using UnityEngine;

namespace Mikrocosmos
{

    public struct OnShakeCamera {
        public float Strength;
        public float Duration;
        public float Viberato;
    }
    public struct OnCameraStartShake{}
    public struct OnCameraShakeEnd {

    }
    public class GameCamera : AbstractMikroController<Mikrocosmos>, ISingleton, ICanSendEvent
    {
       
        public GameObject following;

        [SerializeField] private float lerp = 0.1f;

        private int minCameraRadius = 25;

        private int currentMinCameraRadius;

        private CinemachineTargetGroup cinemachineTargetGroup;
        private CinemachineVirtualCamera vmCamera;
        private CinemachineBasicMultiChannelPerlin virtualCameraNoise;
        [SerializeField]
        private float shakeDuration = 0;
        private bool shakeEndTriggered = true;

        private void Awake()
        {
            cinemachineTargetGroup = GameObject.Find("TargetGroupCamera").GetComponent<CinemachineTargetGroup>();
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnClientMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnCameraViewChange>(OnCameraViewChange).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnVisionPermanentChange>(OnVisionPermanentChange).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnShakeCamera>(OnShakeCamera).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnLocalPlayerKillEntity>(OnLocalPlayerKillEntity)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            currentMinCameraRadius = minCameraRadius;
            vmCamera = GameObject.Find("CM vcam").GetComponent<CinemachineVirtualCamera>();
            virtualCameraNoise = vmCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            
        }

        private void OnLocalPlayerKillEntity(OnLocalPlayerKillEntity e) {
            if (e.KilledEntity.GetComponent<ISpaceshipConfigurationModel>()!=null) {
                OnShakeCamera(new OnShakeCamera()
                {
                    Duration = 0.5f,
                    Strength = 20,
                    Viberato = 15
                });
            }
           
        }

        public void OnShakeCamera(OnShakeCamera e) {
            virtualCameraNoise.ReSeed();
            virtualCameraNoise.m_AmplitudeGain = e.Strength;
            shakeDuration = e.Duration;
            virtualCameraNoise.m_FrequencyGain = e.Viberato;
            
            shakeEndTriggered = false;
            this.SendEvent<OnCameraStartShake>();
            
        }

        private void Update() {
            if (shakeDuration > 0f) {
                shakeDuration -= Time.deltaTime;
                if (shakeDuration <= 0f)
                {
                    virtualCameraNoise.m_AmplitudeGain = 0;
                    if (!shakeEndTriggered)
                    {
                        shakeEndTriggered = true;
                        this.GetSystem<ITimeSystem>().AddDelayTask(0.3f, () => {
                            if (shakeEndTriggered) {
                                this.SendEvent<OnCameraShakeEnd>();
                            }
                        });
                    }
                }
            }
           
        }

        private void OnVisionPermanentChange(OnVisionPermanentChange e) {
            currentMinCameraRadius += (int)(minCameraRadius * e.IncreasePercentage);
            if (e.IncreasePercentage > 0) {
                cinemachineTargetGroup.m_Targets[0].radius = Mathf.Max(currentMinCameraRadius, cinemachineTargetGroup.m_Targets[0].radius);
            }
            else {
                cinemachineTargetGroup.m_Targets[0].radius += (int) (minCameraRadius * e.IncreasePercentage);
                cinemachineTargetGroup.m_Targets[0].radius = Mathf.Max(currentMinCameraRadius, cinemachineTargetGroup.m_Targets[0].radius);
            }
           
        }

        private void OnCameraViewChange(OnCameraViewChange e)
        {
            cinemachineTargetGroup.m_Targets[0].radius = currentMinCameraRadius + e.RadiusAddition;
            cinemachineTargetGroup.m_Targets[0].radius = Mathf.Max(currentMinCameraRadius, cinemachineTargetGroup.m_Targets[0].radius);
        }

        private void OnClientMainGamePlayerConnected(OnClientMainGamePlayerConnected e)
        {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority)
            {
                AddFollowingPlayer(e.playerSpaceship.transform, true);
            }

        }

        public void AddFollowingPlayer(Transform followingObj, bool isLocalPlayer) {
            if (!following) {
                following = followingObj.gameObject;
                cinemachineTargetGroup.AddMember(followingObj, isLocalPlayer ? 3 : 1, 25);
            }
           
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
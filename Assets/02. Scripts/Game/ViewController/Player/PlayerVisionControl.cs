using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Mikrocosmos
{
    //all client side
    public class PlayerVisionControl : CanBeMaskedViewController {
  
      

        private GameObject visionRenderLight;
        private GameObject fovVision;
        //private GameObject playerNameShade;
        [SerializeField]
        private Transform player;

        private void Awake() {
            
            visionRenderLight = transform.Find("VisionControl/VisionRenderLight").gameObject;
            fovVision = transform.Find("VisionControl/FOV Vision").gameObject;
            this.RegisterEvent<OnVisionRangeChange>(OnVisionRangeChange).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnVisionRangeChange(OnVisionRangeChange e) {
            Light2D light = fovVision.GetComponent<Light2D>();
            DOTween.To(() => light.pointLightInnerRadius, x => light.pointLightInnerRadius = x, e.Inner, 0.3f);
            DOTween.To(() => light.pointLightOuterRadius, x => light.pointLightOuterRadius = x, e.Outer, 0.3f);
        }

        void Start() {
            if (hasAuthority) {
               
              
                visionRenderLight.SetActive(true);
                fovVision.SetActive(true);
            }
            else {
               
             
                visionRenderLight.SetActive(false);
                fovVision.SetActive(false);
                player = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.transform;
            }
        }


    
        void Update() {
          
            if (!hasAuthority) {
                if (player == null) {
                    player = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.transform;
                }
            }
        }


        protected override void ClientUpdateCanBeMasked() {
            Material mat;
            if (hasAuthority) {
                mat = Material.Instantiate(defaultSpriteLitMaterial);
            }
            else {
                if (!CanBeMasked) {
                    mat = Material.Instantiate(defaultSpriteLitMaterial);
                }
                else {
                    mat = Material.Instantiate(visionEntityMaterial);
                }
            }
            foreach (SpriteRenderer sprite in visionAffectedSprites)
            {
                sprite.material = mat;
            }
        }
    }
}

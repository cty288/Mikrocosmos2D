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

        private GameObject mapVisionRenderLight;

        private GameObject mapFovVision;

        private bool mapIconCanAlwaysSeenByLocalPlayer;
        //private GameObject playerNameShade;

        private int minInnerRadius ;
        private int minOuterRadius ;

        private int currentMinInnerRadius;
        private int currentMinOuterRadius;
        private void Awake() {
            
            visionRenderLight = transform.Find("VisionControl/VisionRenderLight").gameObject;
            fovVision = transform.Find("VisionControl/FOV Vision").gameObject;
            mapVisionRenderLight = transform.Find("VisionControl/VisionRenderLight - FullMap").gameObject;
            mapFovVision = transform.Find("VisionControl/FOV Vision - FullMap").gameObject;
            minInnerRadius = (int) fovVision.GetComponent<Light2D>().pointLightInnerRadius;
            minOuterRadius = (int) fovVision.GetComponent<Light2D>().pointLightOuterRadius;

            mapFovVision.GetComponent<Light2D>().pointLightInnerRadius = minInnerRadius;
            mapFovVision.GetComponent<Light2D>().pointLightOuterRadius = minOuterRadius;

            currentMinInnerRadius = minInnerRadius;
            currentMinOuterRadius = minOuterRadius;
            
            this.RegisterEvent<OnVisionRangeChange>(OnVisionRangeChange).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnVisionPermanentChange>(OnVisionPermanentChange).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnVisionPermanentChange(OnVisionPermanentChange e) {
            if (hasAuthority) {
                currentMinOuterRadius += (int)(minOuterRadius * e.IncreasePercentage * 1.5f);
                currentMinInnerRadius += (int)(minInnerRadius * e.IncreasePercentage * 1.5f);

                Light2D light = fovVision.GetComponent<Light2D>();
                Light2D mapLight = fovVision.GetComponent<Light2D>();
                
                if (e.IncreasePercentage > 0) {
                  
                    DOTween.To(() => light.pointLightInnerRadius, x => light.pointLightInnerRadius = x, Mathf.Max(light.pointLightInnerRadius, currentMinInnerRadius), 0.3f);
                    DOTween.To(() => light.pointLightOuterRadius, x => light.pointLightOuterRadius = x, Mathf.Max(light.pointLightOuterRadius, currentMinOuterRadius), 0.3f);

                    
                    DOTween.To(() => mapLight.pointLightInnerRadius, x => mapLight.pointLightInnerRadius = x, Mathf.Max(mapLight.pointLightInnerRadius, currentMinInnerRadius), 0.3f);
                    DOTween.To(() => mapLight.pointLightOuterRadius, x => mapLight.pointLightOuterRadius = x, Mathf.Max(mapLight.pointLightOuterRadius, currentMinOuterRadius), 0.3f);
                }
                else {
                    light.pointLightInnerRadius += (int)(minInnerRadius * e.IncreasePercentage);
                    light.pointLightOuterRadius += (int) (minOuterRadius * e.IncreasePercentage);
                    mapLight.pointLightInnerRadius += (int)(minInnerRadius * e.IncreasePercentage);
                    mapLight.pointLightOuterRadius += (int)(minOuterRadius * e.IncreasePercentage);

                    DOTween.To(() => light.pointLightInnerRadius, x => light.pointLightInnerRadius = x, Mathf.Max(light.pointLightInnerRadius, currentMinInnerRadius), 0.3f);
                    DOTween.To(() => light.pointLightOuterRadius, x => light.pointLightOuterRadius = x, Mathf.Max(light.pointLightOuterRadius, currentMinOuterRadius), 0.3f);


                    DOTween.To(() => mapLight.pointLightInnerRadius, x => mapLight.pointLightInnerRadius = x, Mathf.Max(mapLight.pointLightInnerRadius, currentMinInnerRadius), 0.3f);
                    DOTween.To(() => mapLight.pointLightOuterRadius, x => mapLight.pointLightOuterRadius = x, Mathf.Max(mapLight.pointLightOuterRadius, currentMinOuterRadius), 0.3f);
                    
                }
             
            }
           
        }

        private void OnVisionRangeChange(OnVisionRangeChange e) {
            if (hasAuthority) {
                
                Light2D light = fovVision.GetComponent<Light2D>();
                DOTween.To(() => light.pointLightInnerRadius, x => light.pointLightInnerRadius = x, Mathf.Max(currentMinInnerRadius + e.InnerAddition, currentMinInnerRadius), 0.3f);
                DOTween.To(() => light.pointLightOuterRadius, x => light.pointLightOuterRadius = x, Mathf.Max(currentMinOuterRadius + e.InnerAddition, currentMinOuterRadius), 0.3f);

                Light2D mapLight = fovVision.GetComponent<Light2D>();
                DOTween.To(() => mapLight.pointLightInnerRadius, x => mapLight.pointLightInnerRadius = x,   Mathf.Max(currentMinInnerRadius + e.InnerAddition, currentMinInnerRadius), 0.3f);
                DOTween.To(() => mapLight.pointLightOuterRadius, x => mapLight.pointLightOuterRadius = x, Mathf.Max(currentMinOuterRadius + e.OuterAddition, currentMinOuterRadius), 0.3f);
            }
            
        }


        public override void OnStartAuthority() {
            base.OnStartAuthority();
            visionRenderLight.SetActive(true);
            mapVisionRenderLight.SetActive(true);
            mapFovVision.SetActive(true);
            fovVision.SetActive(true);
            
        }

        public override void OnStartClient() {
            base.OnStartClient();
            this.RegisterEvent<OnClientSpaceshipCriminalityUpdate>(OnClientSpaceshipCriminalityUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            if (!hasAuthority) {
                visionRenderLight.SetActive(false);
                fovVision.SetActive(false);
                mapVisionRenderLight.SetActive(false);
                mapFovVision.SetActive(false);
            }

            int currentTeam = this.GetSystem<IRoomMatchSystem>().ClientGetMatchInfoCopy().Team;
            if (GetComponent<PlayerSpaceship>().ThisSpaceshipTeam == currentTeam) {
                mapIconCanAlwaysSeenByLocalPlayer = true;
               // mapVisionRenderLight.SetActive(true);
                mapFovVision.SetActive(true);
                //if (AlsoMaskedOnMap) {
                foreach (SpriteRenderer sprite in visionAffectedSpritesOnMap) {
                    sprite.material = Material.Instantiate(defaultSpriteLitMaterial);
                    }
                //}
            }
        }

        private void OnClientSpaceshipCriminalityUpdate(OnClientSpaceshipCriminalityUpdate e) {
            if (e.SpaceshipIdentity == netIdentity) {
                if (e.Criminality > 0) {
                    mapIconCanAlwaysSeenByLocalPlayer = true;
                    foreach (SpriteRenderer sprite in visionAffectedSpritesOnMap) {
                        sprite.material = Material.Instantiate(defaultSpriteLitMaterial);
                    }
                }
                else {
                    int currentTeam = this.GetSystem<IRoomMatchSystem>().ClientGetMatchInfoCopy().Team;
                    if (GetComponent<PlayerSpaceship>().ThisSpaceshipTeam != currentTeam) {
                        mapIconCanAlwaysSeenByLocalPlayer = false;
                        
                        foreach (SpriteRenderer sprite in visionAffectedSpritesOnMap) {
                            sprite.material = Material.Instantiate(visionEntityMaterial);
                        }
                        
                    }

                }
            }
        }

        /*
        void Update() {
          
            if (!isClient) {
                if (player == null) {
                    //player = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.transform;
                }
            }
        }*/


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

            if (AlsoMaskedOnMap) {
                if (CanBeMasked && mapIconCanAlwaysSeenByLocalPlayer) {
                    return;
                }

                foreach (SpriteRenderer sprite in visionAffectedSpritesOnMap) {
                    sprite.material = mat;
                }
            }
        }
    }
}

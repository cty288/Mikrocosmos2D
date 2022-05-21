using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    //all client side
    public class PlayerVisionControl : CanBeMaskedViewController {
  
      

        private GameObject visionRenderLight;
        private GameObject fovVision;
        //private GameObject playerNameShade;
        private SpriteRenderer playerNameShadeSprite;
        private GameObject playerInfoCanvas;
        [SerializeField]
        private Transform player;

        private void Awake() {
            
            visionRenderLight = transform.Find("VisionControl/VisionRenderLight").gameObject;
            fovVision = transform.Find("VisionControl/FOV Vision").gameObject;
            playerInfoCanvas = transform.Find("VisionControl/PlayerInfoCanvas").gameObject;
            
            
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

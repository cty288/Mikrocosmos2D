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
        private GameObject playerNameShade;
        private SpriteRenderer playerNameShadeSprite;
        private GameObject playerInfoCanvas;
        [SerializeField]
        private Transform player;

        private void Awake() {
            
            visionRenderLight = transform.Find("VisionControl/VisionRenderLight").gameObject;
            fovVision = transform.Find("VisionControl/FOV Vision").gameObject;
            playerInfoCanvas = transform.Find("VisionControl/PlayerInfoCanvas").gameObject;
            playerNameShade = transform.Find("VisionControl/PlayerInfoCanvas/NameShade").gameObject;
            playerNameShadeSprite = playerNameShade.GetComponent<SpriteRenderer>();
            
        }

        void Start() {
            if (hasAuthority) {
               
                playerNameShade.SetActive(false);
                visionRenderLight.SetActive(true);
                fovVision.SetActive(true);
            }
            else {
               
                playerNameShade.SetActive(true);
                visionRenderLight.SetActive(false);
                fovVision.SetActive(false);
                player = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.transform;
            }
        }


        [SerializeField] private LayerMask mask;
        void Update() {
            if (Input.GetKeyDown(KeyCode.V) && isServer) {
                //CanBeMasked = false;
            }
            if (!hasAuthority) {
                if (player == null) {
                    player = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.transform;
                }else {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up,  (player.position - transform.position).normalized, 15,
                        mask);
                    if (hit.collider ) {
                       // Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider.gameObject == player.gameObject) {
                            playerInfoCanvas.SetActive(true);
                        }
                        else {
                            playerInfoCanvas.SetActive(false);
                        }
                    }
                    else {
                        playerInfoCanvas.SetActive(false);
                    }
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

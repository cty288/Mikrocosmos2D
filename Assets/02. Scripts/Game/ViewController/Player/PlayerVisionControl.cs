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
    public class PlayerVisionControl : MonoBehaviour {
        private bool hasControlAuthority;

        [SerializeField] private Material defaultSpriteLitMaterial;
        [SerializeField] private Material visionEntityMaterial;
        private SpriteRenderer sprite;

        private GameObject visionRenderLight;
        private GameObject fovVision;
        private GameObject playerNameShade;
        private SpriteRenderer playerNameShadeSprite;
        private GameObject playerInfoCanvas;
        [SerializeField]
        private Transform player;

        private void Awake() {
            
            sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
            visionRenderLight = transform.Find("VisionRenderLight").gameObject;
            fovVision = transform.Find("FOV Vision").gameObject;
            playerInfoCanvas = transform.Find("PlayerInfoCanvas").gameObject;
            playerNameShade = transform.Find("PlayerInfoCanvas/NameShade").gameObject;
            playerNameShadeSprite = playerNameShade.GetComponent<SpriteRenderer>();
            
        }

        void Start() {
            hasControlAuthority = GetComponentInParent<NetworkIdentity>().hasAuthority;
            if (hasControlAuthority) {
                sprite.material = Material.Instantiate(defaultSpriteLitMaterial); ;
                playerNameShade.SetActive(false);
                visionRenderLight.SetActive(true);
                fovVision.SetActive(true);
            }
            else {
                sprite.material =  Material.Instantiate(visionEntityMaterial);
                playerNameShade.SetActive(true);
                visionRenderLight.SetActive(false);
                fovVision.SetActive(false);
                player = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.transform;
            }
        }


        [SerializeField] private LayerMask mask;
        void Update() {
            if (!hasControlAuthority) {
                if (player == null) {
                    player = NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.transform;
                }else {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up,  (player.position - transform.position).normalized, 20,
                        mask);
                    if (hit.collider ) {
                       // Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider.gameObject == player.gameObject) {
                            playerInfoCanvas.SetActive(true);
                        }
                    }
                    else {
                        playerInfoCanvas.SetActive(false);
                    }
                }

                
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class ItemUsageInGameText : MonoBehaviour {
        
        private Transform childTransform;
        private GameObject localPlayer;
        [SerializeField] private float time = 0.3f;
        private void Awake() {
            childTransform = transform.GetChild(0);
            childTransform.localScale = new Vector3(1, 0, 1);

        }

        private void OnTriggerEnter2D(Collider2D collider) {
            if (NetworkClient.active) {
                if (localPlayer == null) {
                    if (collider.TryGetComponent<PlayerSpaceship>(out PlayerSpaceship playerSpaceship)) {
                        if (playerSpaceship.hasAuthority) {
                            localPlayer = playerSpaceship.gameObject;
                        }
                    }
                }

                if (collider.gameObject == localPlayer) {
                    childTransform.DOScaleY(1, time);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (NetworkClient.active) {
                if (other.gameObject == localPlayer) {
                    childTransform.DOScaleY(0, time);
                }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class GameOuterWallViewController : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D col) {
            if (NetworkServer.active) {
                if (col.gameObject.CompareTag("Meteor"))
                {
                    if (col.TryGetComponent<IMeteorModel>(out var meteorModel))
                    {
                        meteorModel.TakeRawDamage(meteorModel.MaxHealth);
                    }
                }
            }
            
        }

        private void OnCollisionEnter2D(Collision2D col) {
            if (NetworkServer.active) {
                if (col.collider.gameObject.CompareTag("Meteor"))
                {
                    if (col.collider.TryGetComponent<IMeteorModel>(out var meteorModel))
                    {
                        meteorModel.TakeRawDamage(meteorModel.MaxHealth);
                    }
                }
            }
           
        }
    }
}

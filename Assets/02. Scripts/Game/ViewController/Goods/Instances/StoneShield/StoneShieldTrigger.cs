using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class StoneShieldTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayer;


        private StoneShieldModel model;

        private void Awake() {
            model = GetComponentInParent<StoneShieldModel>();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (NetworkServer.active)
            {
                if (PhysicsUtility.IsInLayerMask(collider.gameObject, targetLayer) &&
                    collider.TryGetComponent<IBulletModel>(out IBulletModel m)) {
                    model.CurrCharge++;
                    NetworkServer.Destroy(collider.gameObject);
                }
            }

        }
    }
}

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
        [SerializeField] private int currCharge;

        public int GetCurrCharge()
        {
            return currCharge;
        }


        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (NetworkServer.active)
            {
                if (PhysicsUtility.IsInLayerMask(collider.gameObject, targetLayer))
                {
                    currCharge = currCharge + 1;
                    Destroy(collider);
                }
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class DestroyAfterSpawn : MonoBehaviour{
        [SerializeField] private float destroyTime = 3f;

        private void Start() {
            if (GetComponent<NetworkIdentity>()) {
                if (NetworkServer.active) {
                    Invoke(nameof(ServerDestroy), destroyTime);
                }
                if (NetworkClient.active){return;}
            }
            else {
                Destroy(gameObject, destroyTime);
            }
        }

        private void ServerDestroy() {
            NetworkServer.Destroy(gameObject);
        }
    }
}

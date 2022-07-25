using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class StrangeMeteorTrigger : MonoBehaviour {
        public Action<PlayerSpaceship> OnPlayerEnterTrigger;
        public Action<PlayerSpaceship> OnPlayerExitTrigger;

        private HashSet<PlayerSpaceship> playersInTrigger = new HashSet<PlayerSpaceship>();

        public HashSet<PlayerSpaceship> PlayersInTrigger => playersInTrigger;

     
        private void OnTriggerStay2D(Collider2D col) {
            if (NetworkServer.active) {
                if (col.gameObject.TryGetComponent<PlayerSpaceship>(out PlayerSpaceship spaceship)) {
                    if (!playersInTrigger.Contains(spaceship) && !spaceship.matchInfo.IsSpectator) {
                        OnPlayerEnterTrigger?.Invoke(spaceship);
                        playersInTrigger.Add(spaceship);
                    }
                }
            }
        }

        public void Clear() {
            foreach (PlayerSpaceship spaceship in playersInTrigger) {
                if (spaceship.matchInfo.IsSpectator) {
                    continue;
                }
                OnPlayerExitTrigger?.Invoke(spaceship);
            }
            playersInTrigger.Clear();
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (NetworkServer.active) {
                if (other.gameObject.TryGetComponent<PlayerSpaceship>(out PlayerSpaceship spaceship)) {
                    if (spaceship.matchInfo.IsSpectator) {
                        return;
                    }
                    OnPlayerExitTrigger?.Invoke(spaceship);
                    playersInTrigger.Remove(spaceship);
                }
            }
            
        }
    }
}

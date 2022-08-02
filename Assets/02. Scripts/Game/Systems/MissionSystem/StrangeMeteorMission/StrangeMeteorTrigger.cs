using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class StrangeMeteorTrigger : AbstractMikroController<Mikrocosmos> {
        public Action<PlayerSpaceship> OnPlayerEnterTrigger;
        public Action<PlayerSpaceship> OnPlayerExitTrigger;

        private HashSet<PlayerSpaceship> playersInTrigger = new HashSet<PlayerSpaceship>();

        public HashSet<PlayerSpaceship> PlayersInTrigger => playersInTrigger;

        [SerializeField]
        private bool countDeadPlayer = false;

        private void Start() {
            if (NetworkServer.active) {
                if (!countDeadPlayer) {
                    this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
                }
            }
        }

        private void OnPlayerDie(OnPlayerDie e) {
            if (!countDeadPlayer) {
                PlayerSpaceship spaceship = e.SpaceshipIdentity.GetComponent<PlayerSpaceship>();
                if (playersInTrigger.Contains(spaceship)) {
                    OnPlayerExitTrigger?.Invoke(spaceship);
                    playersInTrigger.Remove(spaceship);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D col) {
            if (NetworkServer.active) {
                if (col.gameObject.TryGetComponent<PlayerSpaceship>(out PlayerSpaceship spaceship)) {
                    if (!playersInTrigger.Contains(spaceship) && !spaceship.matchInfo.IsSpectator) {
                        if (!countDeadPlayer &&
                            spaceship.GetComponent<ISpaceshipConfigurationModel>().CurrentHealth <= 0) {
                            return;
                        }
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

                    if (playersInTrigger.Contains(spaceship)) {
                        OnPlayerExitTrigger?.Invoke(spaceship);
                        playersInTrigger.Remove(spaceship);
                    }
                }
            }
            
        }
    }
}

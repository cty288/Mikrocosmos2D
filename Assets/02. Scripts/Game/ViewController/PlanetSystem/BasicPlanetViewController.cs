using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mikrocosmos
{
    public class BasicPlanetViewController : AbstractPlanetViewController {
        [SerializeField]
        private SpriteRenderer mapPlanetUI;

        [SerializeField] private Color[] teamColors;
       
        [SerializeField] private List<GameObject> hitByObjectParticles;
        public override void OnStartClient() {
            base.OnStartClient();
            this.RegisterEvent<OnClientPlanetAffinityWithTeam1Changed>(OnClientAffinityWithTeam1Changed)
                .UnRegisterWhenGameObjectDestroyed(gameObject, true);
        }

        protected override void OnCollisionEnter2D(Collision2D collision) {
            base.OnCollisionEnter2D(collision);
            if (isServer) {
                if (collision.collider.TryGetComponent<IBulletModel>(out var bullet))
                {
                   
                        ClientMessagerForDestroyedObjects.Singleton.ServerSpawnParticleOnClient(
                            collision.GetContact(0).point, 1);
                
                }
            }
           
        }

        protected override void OnHitByObject(float force, Vector2 contactPoint) {
            base.OnHitByObject(force, contactPoint);
            if (!hitByObjectParticles.Any()) return;
                
            if (force >= 5 && force <= 15) {
                Instantiate(hitByObjectParticles[0], transform);
                Debug.Log($"Small Bump {force}");
            }else if (force > 15) {
                Instantiate(hitByObjectParticles[1], transform);
                Debug.Log($"Big Bump {force}");
            }
        }

        private void OnClientAffinityWithTeam1Changed(OnClientPlanetAffinityWithTeam1Changed e) {
            if (mapPlanetUI) {
                if (e.PlanetIdentity == netIdentity)
                {
                    if (e.NewAffinity == 0.5f)
                    {
                        mapPlanetUI.DOColor(Color.white, 1f);
                    }
                    else if (e.NewAffinity > 0.5f)
                    {
                        mapPlanetUI.DOColor(teamColors[0], 1f);
                    }
                    else
                    {
                        mapPlanetUI.DOColor(teamColors[1], 1f);
                    }
                }
            }
            
        }
    }
}

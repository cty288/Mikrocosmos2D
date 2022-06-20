using System.Collections;
using System.Collections.Generic;
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

        public override void OnStartClient() {
            base.OnStartClient();
            this.RegisterEvent<OnClientPlanetAffinityWithTeam1Changed>(OnClientAffinityWithTeam1Changed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    [Serializable]
    public class OrbitalGroup {
        public List<GameObject> Planets;
    }
    public class PlanetOrbitManagementSystem : AbstractNetworkedSystem {
        [SerializeField] private List<OrbitalGroup> orbitalGroups;
        [SerializeField] private float minimumAngle = 50;
        
        
        private void Start() {
            foreach (OrbitalGroup orbitalGroup in orbitalGroups) {
                float startAngle = Random.Range(0, 360f);
                float maxAngle = (360f / orbitalGroup.Planets.Count) - minimumAngle;
                float angle = startAngle;
                

                
                for (int i = 0; i < orbitalGroup.Planets.Count; i++) {
                    IPlanetViewController planetViewController =
                        orbitalGroup.Planets[i].GetComponent<IPlanetViewController>();
                    if (i == 0) {
                        planetViewController.OrbitalProgress = angle;
                    }
                    else {
                        
                        angle = (angle + Random.Range(minimumAngle, maxAngle)) % 360f;
                        planetViewController.OrbitalProgress = angle;
                    }

                }
            }
        }
    }
}

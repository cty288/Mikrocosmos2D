using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{

    [Serializable]
    public class OrbitalInfo {
        public GameObject OrbitTarget;
        public float OrbitSpeed;
        public float X;
        public float Z;
        public int MaxPlanetsOnThisOrbit;
        public Vector3 StartPosition;
    }
    
    [Serializable]
    public class OrbitalGroup {
        public List<GameObject> FixedPlanets;
        public OrbitalInfo OrbitalInfo;
    }

    public struct OnAllPlanetsSpawned {
        public List<GameObject> AllPlanets;
    }
    public class PlanetOrbitManagementSystem : AbstractNetworkedSystem {
        [SerializeField] private List<OrbitalGroup> orbitalGroups;
        [SerializeField] private float minimumAngle = 50;
        [SerializeField] private List<GameObject> allAvailablePlanets;

        private List<GameObject> allPlanets = new List<GameObject>();

        private void Awake() {
            //iterate through fixPlanets in each orbital group and remove them from the allAvailablePlanets list
            foreach (var orbitalGroup in orbitalGroups) {
                foreach (var fixedPlanet in orbitalGroup.FixedPlanets) {
                    allAvailablePlanets.Remove(fixedPlanet);
                }
            }
        }

        private void Start() {
            foreach (OrbitalGroup orbitalGroup in orbitalGroups) {
                int additionalPlanet = orbitalGroup.OrbitalInfo.MaxPlanetsOnThisOrbit - orbitalGroup.FixedPlanets.Count;
                additionalPlanet = Mathf.Clamp(additionalPlanet, 0, allAvailablePlanets.Count);
                
                for (int i = 0; i < additionalPlanet; i++) {
                    GameObject planet = allAvailablePlanets[Random.Range(0, allAvailablePlanets.Count)];
                    allAvailablePlanets.Remove(planet);
                    orbitalGroup.FixedPlanets.Add(planet);
                }

                float startAngle = Random.Range(0, 360f);
                float maxAngle = (360f / orbitalGroup.FixedPlanets.Count) - minimumAngle;
                float angle = startAngle;
                

                
                for (int i = 0; i < orbitalGroup.FixedPlanets.Count; i++) {
                    GameObject planet =
                        Instantiate(orbitalGroup.FixedPlanets[i], orbitalGroup.OrbitalInfo.StartPosition,
                            Quaternion.identity);
                    IPlanetViewController planetViewController = planet.GetComponent<IPlanetViewController>();
                    planetViewController.Target = orbitalGroup.OrbitalInfo.OrbitTarget;
                    
                    planetViewController.X = orbitalGroup.OrbitalInfo.X;
                    planetViewController.Z = orbitalGroup.OrbitalInfo.Z;
                    planetViewController.OrbitalSpeed = orbitalGroup.OrbitalInfo.OrbitSpeed;
                    if (i == 0) {
                        planetViewController.OrbitalProgress = angle;
                    }
                    else {
                        
                        angle = (angle + Random.Range(minimumAngle, maxAngle)) % 360f;
                        planetViewController.OrbitalProgress = angle;
                    }
                    NetworkServer.Spawn(planet);
                    allPlanets.Add(planet);
                }
            }

            this.SendEvent<OnAllPlanetsSpawned>(new OnAllPlanetsSpawned(){AllPlanets = allPlanets});
        }
    }
}

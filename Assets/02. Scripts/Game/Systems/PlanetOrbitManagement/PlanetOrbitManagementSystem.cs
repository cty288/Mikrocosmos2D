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
    public class PlanetOrbitManagementSystem : AbstractNetworkedSystem {
        [SerializeField] private List<OrbitalGroup> orbitalGroups;
        [SerializeField] private float minimumAngle = 50;
        
        private List<GameObject> allAvailablePlanets;
        

        public override void OnStartServer() {
            base.OnStartServer();
            LoadAllPlanets();
            
            foreach (OrbitalGroup orbitalGroup in orbitalGroups) {
                int additionalPlanet = orbitalGroup.OrbitalInfo.MaxPlanetsOnThisOrbit - orbitalGroup.FixedPlanets.Count;
                additionalPlanet = Mathf.Clamp(additionalPlanet, 0, allAvailablePlanets.Count);

                for (int i = 0; i < additionalPlanet; i++)
                {
                    GameObject planet = allAvailablePlanets[Random.Range(0, allAvailablePlanets.Count)];
                    allAvailablePlanets.Remove(planet);
                    orbitalGroup.FixedPlanets.Add(planet);
                }

                float startAngle = Random.Range(0, 360f);
                float maxAngle = (360f / orbitalGroup.FixedPlanets.Count) - minimumAngle;
                float angle = startAngle;



                for (int i = 0; i < orbitalGroup.FixedPlanets.Count; i++)
                {
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
                }
            }
        }

        private void LoadAllPlanets() {
            IGameResourceModel resourceModel = this.GetModel<IGameResourceModel>();
            allAvailablePlanets = resourceModel.GetAllPlanetPrefabs();
            foreach (var orbitalGroup in orbitalGroups) {
                foreach (var fixedPlanet in orbitalGroup.FixedPlanets) {
                    allAvailablePlanets.Remove(fixedPlanet);
                }
            }
        }
    }
}

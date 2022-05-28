using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using UnityEngine;

namespace Mikrocosmos
{
    public class PointerManager : AbstractMikroController<Mikrocosmos>
    {
        [SerializeField] private GameObject pointerPrefab;


        private string currentSelectedName;
        private Dictionary<GameObject, GameObject> planetToPointer = new Dictionary<GameObject, GameObject>();
        private void Awake()
        {
            this.RegisterEvent<OnClientInventoryUpdate>(OnClientInventoryUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientPlanetSwitchBuyItem>(OnClientPlanetSwitchBuyItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientPlanetSwitchBuyItem(OnClientPlanetSwitchBuyItem e)
        {
            if (e.NewBuyItemName == currentSelectedName)
            {
                CreatePointerWindowForPlanet(e.TargetPlanet);
            }

            if (e.OldBuyItemName == currentSelectedName && planetToPointer.ContainsKey(e.TargetPlanet) && e.OldBuyItemName!= e.NewBuyItemName)
            {
                Debug.Log($"Old Buy Item: {e.OldBuyItemName}");
                Destroy(planetToPointer[e.TargetPlanet]);
                planetToPointer.Remove(e.TargetPlanet);
            }
        }


        private void OnClientInventoryUpdate(OnClientInventoryUpdate e)
        {
            if (currentSelectedName != e.AllSlots[e.SelectedIndex].PrefabName || e.AllSlots[e.SelectedIndex].ClientSlotCount == 0)
            {
                if (e.AllSlots[e.SelectedIndex].ClientSlotCount > 0)
                {
                    currentSelectedName = e.AllSlots[e.SelectedIndex].PrefabName;
                }
                else
                {
                    currentSelectedName = "";
                }

                //delete all target planets and pointerWindows
                foreach (var planet in planetToPointer.Keys)
                {
                    Destroy(planetToPointer[planet]);
                }
                planetToPointer.Clear();
            }


            if (e.AllSlots[e.SelectedIndex].ClientSlotCount > 0)
            {
                //find planets which buy current item
                List<GameObject> planets = GetAllPlanetsBuyItem(e.AllSlots[e.SelectedIndex].PrefabName);

                //create pointerWindow for each planet
                foreach (var planet in planets)
                {
                    CreatePointerWindowForPlanet(planet);
                }

                
              
            }
        }

        private void CreatePointerWindowForPlanet(GameObject planet)
        {
          
            if (!planetToPointer.ContainsKey(planet)) {
                GameObject pointer = Instantiate(pointerPrefab, transform);
                pointer.SetActive(false);
                this.GetSystem<ITimeSystem>().AddDelayTask(0.02f, () => {
                    if (pointer) {
                        pointer.SetActive(true);
                    }
                });
                pointer.GetComponent<Window_Pointer>().target = planet.transform;
                planetToPointer.Add(planet, pointer);
            }
             
         
        }

        protected List<GameObject> GetAllPlanetsBuyItem(string itemName)
        {
            return GameObject.FindGameObjectsWithTag("Planet").Where(planet =>
                planet.GetComponent<IPlanetTradingSystem>().CurrentBuyItemName == itemName).ToList();

        }
    }
}

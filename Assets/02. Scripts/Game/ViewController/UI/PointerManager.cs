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
       
        private Dictionary<GameObject, List<MapPointerViewController>> planetToPointers = new Dictionary<GameObject, List<MapPointerViewController>>();
        private void Awake()
        {
            this.RegisterEvent<OnClientInventoryUpdate>(OnClientInventoryUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientPlanetGenerateBuyItem>(OnClientPlanetGenerateBuyItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientPlanetGenerateBuyItem(OnClientPlanetGenerateBuyItem e)
        {
           
            if (planetToPointers.ContainsKey(e.TargetPlanet)) {
                MapPointerViewController oldPointer = planetToPointers[e.TargetPlanet].Find(controller => controller.GoodsName == e.OldBuyItemName);
                if (oldPointer) {
                    Debug.Log($"Old Buy Item: {e.OldBuyItemName}");
                    planetToPointers[e.TargetPlanet].Remove(oldPointer);
                    Destroy(oldPointer.gameObject);
                }
            }
            
            CreatePointerWindowForPlanet(e.TargetPlanet, e.NewBuyItemName, e.MaxBuyTime);
        }


        private void OnClientInventoryUpdate(OnClientInventoryUpdate e)
        {
            if (currentSelectedName != e.AllSlots[e.SelectedIndex].PrefabName || e.AllSlots[e.SelectedIndex].ClientSlotCount == 0) {
                
                if (e.AllSlots[e.SelectedIndex].ClientSlotCount > 0) {
                    currentSelectedName = e.AllSlots[e.SelectedIndex].PrefabName;
                }
                else {
                    currentSelectedName = "";
                }

                foreach (var planet in planetToPointers.Keys) {
                    planetToPointers[planet].ForEach(controller => controller.SetPointerActive(false));
                }
            }


            if (e.AllSlots[e.SelectedIndex].ClientSlotCount > 0)
            {
                
                currentSelectedName = e.AllSlots[e.SelectedIndex].PrefabName;
                
                foreach (var planet in planetToPointers.Keys) {
                    planetToPointers[planet].ForEach(controller => {
                        if (controller.GoodsName == currentSelectedName) {
                            controller.SetPointerActive(true);
                        }
                    });
                }



            }
        }

        private void CreatePointerWindowForPlanet(GameObject planet, string goodsName, float time) {
          
            if (!planetToPointers.ContainsKey(planet)) {
                planetToPointers.Add(planet, new List<MapPointerViewController>());
            }

            if (!String.IsNullOrEmpty(goodsName) &&
                planetToPointers[planet].Find(controller => controller.GoodsName == goodsName) == null) {

                MapPointerViewController pointer = Instantiate(pointerPrefab, transform).GetComponent<MapPointerViewController>();
                pointer.GoodsName = goodsName;
                pointer.Time = time;
                pointer.timer = time;


                pointer.SetPointerActive(false);
                
                this.GetSystem<ITimeSystem>().AddDelayTask(0.02f, () => {
                    if (pointer) {
                        if (currentSelectedName == goodsName) {
                            pointer.SetPointerActive(true);
                        }
                       
                    }
                });
                pointer.GetComponent<Window_Pointer>().target = planet.transform;
                planetToPointers[planet].Add(pointer);
            }

            


        }

        
    }
}

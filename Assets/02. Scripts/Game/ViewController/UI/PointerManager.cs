using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mikrocosmos
{
    public class PointerManager : AbstractMikroController<Mikrocosmos>
    {
        [FormerlySerializedAs("pointerPrefab")] [SerializeField] private GameObject planetPointerPrefab;

        private string currentSelectedName;
       
        private Dictionary<GameObject, List<IMapPointerViewController>> planetToPointers = new Dictionary<GameObject, List<IMapPointerViewController>>();
        private void Awake()
        {
            this.RegisterEvent<OnClientHookIdentityChanged>(OnClientInventoryUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientPlanetGenerateBuyItem>(OnClientPlanetGenerateBuyItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientPlanetGenerateBuyItem(OnClientPlanetGenerateBuyItem e)
        {
           
            if (planetToPointers.ContainsKey(e.TargetPlanet)) {
                IMapPointerViewController oldPointer = planetToPointers[e.TargetPlanet].Find(controller => controller.Name == e.OldBuyItemName);
                if (oldPointer!=null && oldPointer.BindedGameObject) {
                    Debug.Log($"Old Buy Item: {e.OldBuyItemName}");
                    planetToPointers[e.TargetPlanet].Remove(oldPointer);
                    Destroy(oldPointer.BindedGameObject);
                }
            }
            
            CreatePointerWindowForPlanet(e.TargetPlanet, e.NewBuyItemName, e.MaxBuyTime);
        }


        private void OnClientInventoryUpdate(OnClientHookIdentityChanged e)
        {
            
            if (currentSelectedName != e.NewHookItemName) {
                
                if (!string.IsNullOrEmpty(e.NewHookItemName)) {
                    currentSelectedName = e.NewHookItemName;
                }
                else {
                    currentSelectedName = "";
                }

                foreach (var planet in planetToPointers.Keys) {
                    planetToPointers[planet].ForEach(controller => controller.SetPointerActive(false));
                }
            }


            if (!string.IsNullOrEmpty(currentSelectedName)) {
                
                foreach (var planet in planetToPointers.Keys) {
                    planetToPointers[planet].ForEach(controller => {
                        if (controller.Name == currentSelectedName) {
                            controller.SetPointerActive(true);
                        }
                    });
                }
            }
        }

        private void CreatePointerWindowForPlanet(GameObject planet, string goodsName, float time) {
          
            if (!planetToPointers.ContainsKey(planet)) {
                planetToPointers.Add(planet, new List<IMapPointerViewController>());
            }

            if (!String.IsNullOrEmpty(goodsName) &&
                planetToPointers[planet].Find(controller => controller.Name == goodsName) == null) {

                MapPointerViewController pointer = Instantiate(planetPointerPrefab, transform).GetComponent<MapPointerViewController>();
                pointer.Name = goodsName;
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

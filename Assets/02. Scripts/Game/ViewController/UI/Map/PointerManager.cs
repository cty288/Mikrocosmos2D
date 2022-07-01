using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using MikroFramework.TimeSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mikrocosmos
{

    public struct OnClientAddOrUpdatePointer {
        public string PointerName;
        public GameObject PointerFollowing;
        public bool IsActive;
        public GameObject PointerPrefab;
    }

    public struct OnClientRemovePointer {
        public string PointerName;
    }
    public class PointerManager : AbstractMikroController<Mikrocosmos>, ISingleton
    {
        [FormerlySerializedAs("pointerPrefab")] [SerializeField] private GameObject planetPointerPrefab;

        private string currentSelectedName;
       
        private Dictionary<GameObject, List<IMapPointerViewController>> planetToPointers = new Dictionary<GameObject, List<IMapPointerViewController>>();

        private Dictionary<string, IMapPointerViewController> nameToPointer =
            new Dictionary<string, IMapPointerViewController>();


        public static PointerManager Singleton {
            get {
                return SingletonProperty<PointerManager>.Singleton;
            }
        }

        private void Awake()
        {
            this.RegisterEvent<OnClientHookIdentityChanged>(OnClientInventoryUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientPlanetGenerateBuyItem>(OnClientPlanetGenerateBuyItem)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientAddOrUpdatePointer>(OnClientAddOrUpdatePointer)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientRemovePointer>(OnClientRemovePointer);
        }

        public void OnClientRemovePointer(OnClientRemovePointer e) {
            if (nameToPointer.ContainsKey(e.PointerName)) {
                if (nameToPointer[e.PointerName] != null) {
                    if (nameToPointer[e.PointerName].BindedGameObject) {
                        Destroy(nameToPointer[e.PointerName].BindedGameObject);
                    }
                }
                nameToPointer.Remove(e.PointerName);
            }
        }

        public void OnClientAddOrUpdatePointer(OnClientAddOrUpdatePointer e) {
            if (nameToPointer.ContainsKey(e.PointerName)) {
                if (nameToPointer[e.PointerName]!=null) {
                    nameToPointer[e.PointerName].BindedGameObject = e.PointerFollowing;
                    nameToPointer[e.PointerName].SetPointerActive(e.IsActive);
                }
            }
            else {
                CreatePointerWindow(e.PointerPrefab, e.PointerFollowing, e.PointerName, e.IsActive);
            }
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
            
            CreatePointerWindowForPlanet(e.TargetPlanet, e.NewBuyItemName, e.MaxBuyTime, e.PointerPrefab);
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

        private void CreatePointerWindow(GameObject prefab, GameObject following, string name, bool isActive) {
            if (!nameToPointer.ContainsKey(name)) {
                GameObject pointerObj =
                    Instantiate(prefab, transform);
                IMapPointerViewController pointer = pointerObj.GetComponent<IMapPointerViewController>();
                pointer.Name = name;
                pointer.BindedGameObject = pointerObj;
                pointer.SetPointerActive(isActive);
                pointerObj.GetComponent<Window_Pointer>().target = following.transform;
                nameToPointer.Add(name, pointer);
            }
        }
        private void CreatePointerWindowForPlanet(GameObject planet, string goodsName, float time, GameObject pointerPrefab) {
          
            if (!planetToPointers.ContainsKey(planet)) {
                planetToPointers.Add(planet, new List<IMapPointerViewController>());
            }

            if (!String.IsNullOrEmpty(goodsName) &&
                planetToPointers[planet].Find(controller => controller.Name == goodsName) == null) {

                GameObject pointerGameObject = null;

                if (pointerPrefab) {
                    pointerGameObject = Instantiate(pointerPrefab, transform);
                }
                else {
                    pointerGameObject = Instantiate(planetPointerPrefab, transform);
                }

                IMapPointerViewController pointer = pointerGameObject.GetComponent<IMapPointerViewController>();
              
                pointer.Name = goodsName;

                if (pointer is MapPointerViewController planetPointer) {
                    planetPointer.Time = time;
                    planetPointer.timer = time;
                }
              


                pointer.SetPointerActive(false);
                
                this.GetSystem<ITimeSystem>().AddDelayTask(0.02f, () => {
                    if (pointerGameObject) {
                        if (currentSelectedName == goodsName) {
                            pointer.SetPointerActive(true);
                        }
                       
                    }
                });
                pointerGameObject.GetComponent<Window_Pointer>().target = planet.transform;
                planetToPointers[planet].Add(pointer);
            }
        }


        public void OnSingletonInit() {
            
        }
    }
}

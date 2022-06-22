using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;

namespace Mikrocosmos
{
    public class GameInfoPanel : AbstractMikroController<Mikrocosmos> {
        [SerializeField] private List<GameObject> InfoElementPrefabs;

        private Transform layoutGroup;

        private Dictionary<string, InfoElement> infoNameToElement = new Dictionary<string, InfoElement>();

        
        private void Awake() {
            this.RegisterEvent<OnInfoStartOrUpdate>(OnInfoStartOrUpdate).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnInfoStop>(OnInfoStop).UnRegisterWhenGameObjectDestroyed(gameObject);
            layoutGroup = transform.Find("List/LayoutGroup");
        }

        private void OnInfoStop(OnInfoStop e) {
            if (infoNameToElement.ContainsKey(e.InfoName)) {
                InfoElement infoElement = infoNameToElement[e.InfoName];
                if (infoElement) {
                    infoElement.StopInfo();
                }
                infoNameToElement.Remove(e.InfoName);
            }
        }

        private void OnInfoStartOrUpdate(OnInfoStartOrUpdate e) {
            if (!infoNameToElement.ContainsKey(e.Info.Name)) {
                InfoElement spawnedInfoElement = Instantiate(GetInfoPrefabFromInfoType(e.Info.InfoType), layoutGroup)
                    .GetComponent<InfoElement>();
                spawnedInfoElement.transform.SetAsFirstSibling();
                infoNameToElement.Add(e.Info.Name, spawnedInfoElement);
                spawnedInfoElement.SetInfo(e.Info, false);
            }
            else {
                InfoElement infoElement = infoNameToElement[e.Info.Name];
                if (infoElement) {
                    infoElement.SetInfo(e.Info, true);
                }
                else {
                    infoNameToElement.Remove(e.Info.Name);
                }
            }
        }

        public void InfoElementSelfDestroy(string name) {
            if (infoNameToElement.ContainsKey(name)) {
                InfoElement infoElement = infoNameToElement[name];
                if (infoElement) {
                    infoElement.StopInfo();
                }
                infoNameToElement.Remove(name);
            }
        }

        private GameObject GetInfoPrefabFromInfoType(InfoType infoType) {
            return InfoElementPrefabs[(int)infoType];
        }
    }
}

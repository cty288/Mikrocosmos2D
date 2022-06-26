using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mikrocosmos
{
    public class BuffInfoPanelViewController : AbstractMikroController<Mikrocosmos> {
        private Transform nonPermanentBuffLayoutGroup;
        private Transform permanentBuffLayoutGroup;

        [FormerlySerializedAs("buffElementPrefab")] [SerializeField] 
        private GameObject nonPermanentBuffElementPrefab;

        [SerializeField]
        private GameObject permanentBuffElementPrefab;

        private ResLoader resLoader;

        private Dictionary<string, BuffElementViewController> buffNameToElement = new Dictionary<string, BuffElementViewController>();
        
        private void Awake() {
            ResLoader.Create((loader => resLoader = loader));
            nonPermanentBuffLayoutGroup = transform.Find("BuffList1/BuffLayoutGroup");
            permanentBuffLayoutGroup = transform.Find("BuffList2/BuffLayoutGroup");
        }

        private void Start() {
            this.RegisterEvent<ClientOnBuffUpdate>(OnBuffUpdate);
        }


        
        private void OnBuffUpdate(ClientOnBuffUpdate e) {
            
            switch (e.UpdateMode) {
                case BuffUpdateMode.Start:
                    if (!buffNameToElement.ContainsKey(e.BuffInfo.Name)) {
                        BuffElementViewController buffElement = null;
                        if (e.BuffInfo.PermanentRawMaterialBuffInfo.MaxLevel == 0) { //non-permanent buff
                            buffElement = Instantiate(nonPermanentBuffElementPrefab, nonPermanentBuffLayoutGroup).GetComponent<BuffElementViewController>();
                        }
                        else {
                            buffElement = Instantiate(permanentBuffElementPrefab, permanentBuffLayoutGroup).GetComponent<BuffElementViewController>(); 
                        }
                       
                            
                        buffNameToElement.Add(e.BuffInfo.Name, buffElement);
                        
                        GameObject buffIconPrefab = resLoader.LoadSync<GameObject>("buff_icons", e.BuffInfo.Name+ "Icon");
                        Debug.Log($"Buff Icon Prefab: {buffIconPrefab == null}");
                        Instantiate(buffIconPrefab, buffElement.transform.Find("BuffIconSpawnPosition"));

                        buffElement.SetBuffInfo(e.BuffInfo);
                    }
                    break;
                case BuffUpdateMode.Update:
                    
                    if (buffNameToElement[e.BuffInfo.Name] != null) {
                        buffNameToElement[e.BuffInfo.Name].SetBuffInfo(e.BuffInfo);
                    }
                    break;
                case BuffUpdateMode.Stop:
                    if (buffNameToElement[e.BuffInfo.Name] != null) {
                        buffNameToElement.Remove(e.BuffInfo.Name, out BuffElementViewController buffElement);
                        Destroy(buffElement.gameObject);
                    }
                    break;
            }
        }
    }
}

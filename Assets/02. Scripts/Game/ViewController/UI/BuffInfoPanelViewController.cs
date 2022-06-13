using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using UnityEngine;

namespace Mikrocosmos
{
    public class BuffInfoPanelViewController : AbstractMikroController<Mikrocosmos> {
        private Transform buffLayoutGroup;

        [SerializeField] 
        private GameObject buffElementPrefab;

        private ResLoader resLoader;

        private Dictionary<string, BuffElementViewController> buffNameToElement = new Dictionary<string, BuffElementViewController>();
        
        private void Awake() {
            ResLoader.Create((loader => resLoader = loader));
            buffLayoutGroup = transform.Find("BuffList/BuffLayoutGroup");
        }

        private void Start() {
            this.RegisterEvent<ClientOnBuffUpdate>(OnBuffUpdate);
        }

        private void OnBuffUpdate(ClientOnBuffUpdate e) {
            
            switch (e.UpdateMode) {
                case BuffUpdateMode.Start:
                    if (!buffNameToElement.ContainsKey(e.BuffInfo.Name)) {
                        BuffElementViewController buffElement = Instantiate(buffElementPrefab, buffLayoutGroup)
                            .GetComponent<BuffElementViewController>();
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

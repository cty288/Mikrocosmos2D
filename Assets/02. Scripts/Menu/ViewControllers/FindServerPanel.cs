using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;

namespace Mikrocosmos {
	public partial class FindServerPanel : AbstractMikroController<Mikrocosmos> {
        private bool isFinding = false;
        [SerializeField]
        private Dictionary<long, DiscoveryResponse> allSearchedServers = new Dictionary<long, DiscoveryResponse>();
        
        private void Awake() {
            this.RegisterEvent<OnStartNetworkDiscovery>(OnNetworkDiscoveryStart)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnStopNetworkDiscovery>(OnNetworkDiscoveryStop)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnNetworkDiscoveryStop(OnStopNetworkDiscovery obj) {
            isFinding = false;
        }

        private void OnNetworkDiscoveryStart(OnStartNetworkDiscovery e) {
            Debug.Log("Start discovery");
            e.FoundEvent.AddListener(OnNetworkRefresh);
            isFinding = true;
            //(NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).StartDiscovery();
            StartCoroutine(RefreshServerList());
        }

        IEnumerator RefreshServerList() {
            while (isFinding) {
                allSearchedServers.Clear();
                for (int i = 0; i < TrRoomLayoutGroup.childCount; i++)
                {

                    TrRoomLayoutGroup.GetChild(i).gameObject.SetActive(false);

                }
                (NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).StartDiscovery();
                yield return new WaitForSeconds(3f);
               
            }
        }

        private void OnNetworkRefresh(DiscoveryResponse room) {
            // Debug.Log($"Find a room: Room owner: {room.HostName}; Room Player Count: {room.ServerPlayerNum}; uri: {room.Uri};");
            if (allSearchedServers.ContainsKey(room.ServerID)) {
                allSearchedServers[room.ServerID] = room;
            }
            else {
                allSearchedServers.Add(room.ServerID, room);
            }

            var enumerator = allSearchedServers.GetEnumerator();
            
            for (int i = 0; i < TrRoomLayoutGroup.childCount; i++) {
                if (i < allSearchedServers.Count) {
                    enumerator.MoveNext();
                    TrRoomLayoutGroup.GetChild(i).gameObject.SetActive(true);
                    DiscoveryResponse currentResponse = allSearchedServers[enumerator.Current.Key];
                    TrRoomLayoutGroup.GetChild(i).GetComponent<RoomInfo>().SetRoomInfo(currentResponse);
                }
                else {
                    TrRoomLayoutGroup.GetChild(i).gameObject.SetActive(false);
                }
            }
          
        }
    }
}
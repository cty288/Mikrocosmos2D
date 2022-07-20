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
using Steamworks;

namespace Mikrocosmos {
	public partial class FindServerPanel : AbstractMikroController<Mikrocosmos> {
        private bool isFinding = false;
        [SerializeField]
        private Dictionary<long, DiscoveryResponse> allSearchedServers = new Dictionary<long, DiscoveryResponse>();

        [SerializeField] private float joinRoomTimeout = 10f;
        private float joinRoomTimeoutTimer = 0f;
        

        protected Callback<LobbyMatchList_t> OnSteamLobbyGetCallback;
        protected Callback<LobbyDataUpdate_t> OnSteamLobbyDataGetCallback;
        private void Awake() {
            this.RegisterEvent<OnStartNetworkDiscovery>(OnNetworkDiscoveryStart)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnStopNetworkDiscovery>(OnNetworkDiscoveryStop)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnJoinRoomButtonClicked>(OnJoinRoomButtonClicked)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            BtnAddServerJoinRoom.onClick.AddListener(OnAddServerJoinRoomButtonClicked);
        }

        private void OnJoinRoomButtonClicked(OnJoinRoomButtonClicked e) {
            ObjJoiningRoomPanel.gameObject.SetActive(true);
            joinRoomTimeoutTimer = joinRoomTimeout;
            TextJoinRoomPanelInfo.text = "Joining room...";
            BtnCloseJoinRoomPanel.gameObject.SetActive(false);
        }

        private void Start() {
            OnSteamLobbyGetCallback = Callback<LobbyMatchList_t>.Create(OnSteamLobbyGet);
            OnSteamLobbyDataGetCallback = Callback<LobbyDataUpdate_t>.Create(OnSteamLobbyDataGet);
        }

        
        

        private void OnAddServerJoinRoomButtonClicked() {
            if (InputPort.text != "") {
                NetworkManager.singleton.GetComponent<TelepathyTransport>().port = ushort.Parse(InputPort.text);
                NetworkManager.singleton.networkAddress = InputIPInput.text;
                ((NetworkedRoomManager)NetworkManager.singleton).StartJoiningClient(NetworkingMode.Normal);
            }
            else {
                //NetworkManager.singleton.GetComponent<TelepathyTransport>().port = 
                ((NetworkedRoomManager)NetworkManager.singleton).StartJoiningClient(NetworkingMode.Normal);


            }


        }

        private void OnNetworkDiscoveryStop(OnStopNetworkDiscovery obj) {
            isFinding = false;
        }

        private void OnNetworkDiscoveryStart(OnStartNetworkDiscovery e) {
            Debug.Log("Start discovery");
            e.FoundEvent.AddListener(OnNetworkRefreshRooms);
            isFinding = true;
            //(NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).StartDiscovery();
            StartCoroutine(RefreshServerList());
        }

        private void Update() {
            if (String.IsNullOrEmpty(InputIPInput.text)) {
                BtnAddServerJoinRoom.enabled = false;
            }
            else {
                BtnAddServerJoinRoom.enabled = true;
            }

            joinRoomTimeoutTimer -= Time.deltaTime;
            if (joinRoomTimeoutTimer <= 0 && ObjJoiningRoomPanel.activeInHierarchy) {
                TextJoinRoomPanelInfo.text = "Join Room Failed!";
                BtnCloseJoinRoomPanel.gameObject.SetActive(true);
            }
        }

        IEnumerator RefreshServerList() {
            while (isFinding) {
                allSearchedServers.Clear();
                for (int i = 0; i < TrRoomLayoutGroup.childCount; i++) {
                    TrRoomLayoutGroup.GetChild(i).gameObject.SetActive(false);
                }
                (NetworkManager.singleton.GetComponent<MenuNetworkDiscovery>()).StartDiscovery();
                GetSteamLobbies();
                yield return new WaitForSeconds(3f);
               
            }
        }

        private void GetSteamLobbies() {
            if (SteamManager.Initialized) {
                SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
                SteamMatchmaking.RequestLobbyList();
              
            }
        }
        private void OnSteamLobbyGet(LobbyMatchList_t result)
        {
            for (int i = 0; i < result.m_nLobbiesMatching; i++) {
                CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                SteamMatchmaking.RequestLobbyData(lobbyID);

                string hostName = SteamMatchmaking.GetLobbyData(lobbyID, "HostName");
                int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                string gameName = SteamMatchmaking.GetLobbyData(lobbyID, "GameName");

                if (gameName == "Mikrocosmos" && !String.IsNullOrEmpty(hostName) && playerCount > 0) {
                    DiscoveryResponse response = new DiscoveryResponse() {
                        HostName = hostName,
                        IsGaming = SteamMatchmaking.GetLobbyData(lobbyID, "IsGaming") == "true",
                        IsLAN = false,
                        HostSteamAddress = SteamMatchmaking.GetLobbyData(lobbyID, "HostAddress"),
                        ServerPlayerNum = playerCount,
                        ServerMaxPlayerNum = int.Parse(SteamMatchmaking.GetLobbyData(lobbyID, "ServerMaxPlayerNum")),
                        HostSteamLobbyID = lobbyID,
                        ServerID = (long) lobbyID.m_SteamID
                    };
                    OnNetworkRefreshRooms(response);
                }
                Debug.Log($"Lobby member host name : {hostName}, count: {playerCount}");
            }
        }
        private void OnSteamLobbyDataGet(LobbyDataUpdate_t result) {
            
        }
        private void OnNetworkRefreshRooms(DiscoveryResponse room) {
            
            // Debug.Log($"Find a room: Room owner: {room.HostName}; Room Player Count: {room.ServerPlayerNum}; uri: {room.Uri};");
           // if (room.IsLAN) {
           if (room.ServerPlayerNum <= 0) {
               return;
           }
           if (allSearchedServers.ContainsKey(room.ServerID))
                {
                    allSearchedServers[room.ServerID] = room;
                }
                else
                {
                    allSearchedServers.Add(room.ServerID, room);
                }

                var enumerator = allSearchedServers.GetEnumerator();
                if (TrRoomLayoutGroup) {
                    for (int i = 0; i < TrRoomLayoutGroup.childCount; i++)
                    {
                        if (i < allSearchedServers.Count)
                        {
                            enumerator.MoveNext();
                            TrRoomLayoutGroup.GetChild(i).gameObject.SetActive(true);
                            DiscoveryResponse currentResponse = allSearchedServers[enumerator.Current.Key];
                            TrRoomLayoutGroup.GetChild(i).GetComponent<RoomInfo>().SetRoomInfo(currentResponse);
                        }
                        else
                        {
                            TrRoomLayoutGroup.GetChild(i).gameObject.SetActive(false);
                        }
                    }
            }
               
           // }
           
          
        }
    }
}
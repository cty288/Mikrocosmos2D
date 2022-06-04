using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using Mirror;
using Mirror.FizzySteam;
using NHibernate.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mikrocosmos
{

    public enum NetworkingMode {
        Normal,
        Steam
    }
    public struct OnAllPlayersReadyStatusChanged {
        public bool IsAllPlayerReady;
    }

    public class NetworkedRoomManager : NetworkRoomManager, IController, ICanSendEvent {
        [SerializeField] private GameObject matchSystemPrefab;

        public bool IsInGame = false;

        [HideInInspector] public NetworkingMode NetworkingMode;

        private TelepathyTransport telepathyTransport;
        private FizzySteamworks steamworksTransport;

        protected Callback<LobbyCreated_t> OnSteamLobbyCreatedEvent;
        protected Callback<LobbyEnter_t> OnSteamLobbyEnteredEvent;

        private CSteamID joinedSteamGame;
        public override void Awake() {
            base.Awake();
            networkAddress = NetworkUtility.GetLocalIPAddress();
            showRoomGUI = false;
            telepathyTransport = GetComponent<TelepathyTransport>();
            steamworksTransport = GetComponent<FizzySteamworks>();

            OnSteamLobbyCreatedEvent = Callback<LobbyCreated_t>.Create(OnSteamLobbyCreated);
            OnSteamLobbyEnteredEvent = Callback<LobbyEnter_t>.Create(OnSteamLobbyEntered);

        }

        private void OnSteamLobbyEntered(LobbyEnter_t callback) {
            if (!NetworkServer.active && !NetworkClient.active) {
                
                
                string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
                    "HostAddress");
                networkAddress = hostAddress;
                Debug.Log(hostAddress);
                StartJoiningClient(NetworkingMode.Steam);
            }
        }

        private void OnSteamLobbyCreated(LobbyCreated_t callback) {
            if (callback.m_eResult != EResult.k_EResultOK) {
                SceneManager.LoadScene(0);
                
                return;
            }
           
            NetworkManager.singleton.StartHost();

            joinedSteamGame = new CSteamID(callback.m_ulSteamIDLobby);
            
            SteamMatchmaking.SetLobbyData( joinedSteamGame, "HostAddress",
                SteamUser.GetSteamID().ToString());

            SteamMatchmaking.SetLobbyData(joinedSteamGame, "IsGaming",
                "false");

            SteamMatchmaking.SetLobbyData(joinedSteamGame, "ServerMaxPlayerNum",
                maxConnections.ToString());

            SteamMatchmaking.SetLobbyData(joinedSteamGame, "HostName",
                this.GetModel<ILocalPlayerInfoModel>().NameInfo.Value);
            
            SteamMatchmaking.SetLobbyData(joinedSteamGame, "GameName",
               "Mikrocosmos");
            Debug.Log("Room player count:" + SteamMatchmaking.GetNumLobbyMembers(joinedSteamGame));
        }


        #region Server
        
        public override void OnRoomServerConnect(NetworkConnectionToClient conn) {
            base.OnRoomServerConnect(conn);
            
        }

        public override void OnStopServer() {
            base.OnStopServer();
            
           
        }

        /*
        public override void OnRoomClientAddPlayerFailed() {
            base.OnRoomClientAddPlayerFailed();
            Debug.Log("ROOM ADD CLIENT FAILED!");
        }*/

        public override void OnRoomStartServer() {
            base.OnRoomStartServer();
            if (NetworkingMode == NetworkingMode.Normal)
            {
                GetComponent<MenuNetworkDiscovery>().AdvertiseServer();
            }
           
            GameObject matchSystem = Instantiate(matchSystemPrefab);
            NetworkServer.Spawn(matchSystem);
            GameObject.DontDestroyOnLoad(matchSystem.gameObject);
            
        }

        //host button show
        public override void OnRoomServerPlayersReady() {
            this.SendEvent<OnAllPlayersReadyStatusChanged>(new OnAllPlayersReadyStatusChanged(){IsAllPlayerReady = true});
        }



        public override void OnRoomServerPlayersNotReady() {
            this.SendEvent<OnAllPlayersReadyStatusChanged>(new OnAllPlayersReadyStatusChanged() { IsAllPlayerReady = false });
        }

        public override void OnServerSceneChanged(string sceneName) {
            base.OnServerSceneChanged(sceneName);
            if (sceneName == RoomScene || sceneName == offlineScene) {
                IsInGame = false;
               
               
            }
            else if (sceneName == GameplayScene) {
                IsInGame = true;
                if (NetworkingMode == NetworkingMode.Steam) {
                    SteamMatchmaking.SetLobbyData(joinedSteamGame, "IsGaming",
                        "true");
                }
            }
        }



        public string GetHostName() {
            if (NetworkServer.active) {
               return  this.GetSystem<IRoomMatchSystem>().ServerGetHostInfo().Name;
            }

            return "";
        }
     
        #endregion

        public override void OnRoomStopClient() {
            base.OnRoomStopClient();
            if (NetworkingMode == NetworkingMode.Steam) {
                SteamMatchmaking.LeaveLobby(joinedSteamGame);
                joinedSteamGame = new CSteamID();
            }
        }

        public void StartHosting(NetworkingMode networkingMode) {
           // telepathyTransport.enabled = false;
            steamworksTransport.enabled = false;

            switch (networkingMode) {
                case NetworkingMode.Normal:
                    telepathyTransport.enabled = true;
                    transport = telepathyTransport;
                    telepathyTransport.port = (ushort)Random.Range(7777, 15000);
                    NetworkingMode = NetworkingMode.Normal;
                    StartHost();
                    break;
                case NetworkingMode.Steam:
                    steamworksTransport.enabled = true;
                    transport = steamworksTransport;
                    SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxConnections);
                    NetworkingMode = NetworkingMode.Steam;
                    break;
            }
        }

        public void StartJoiningClient(NetworkingMode networkingMode)
        {
          //  telepathyTransport.enabled = false;
            steamworksTransport.enabled = false;

            switch (networkingMode)
            {
                case NetworkingMode.Normal:
                    telepathyTransport.enabled = true;
                    transport = telepathyTransport;
                    NetworkingMode = NetworkingMode.Normal;
                    StartClient();
                    break;
                case NetworkingMode.Steam:
                    steamworksTransport.enabled = true;
                    transport = steamworksTransport;
                    NetworkingMode = NetworkingMode.Steam;
                    StartClient();
                    break;
            }
        }




        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}

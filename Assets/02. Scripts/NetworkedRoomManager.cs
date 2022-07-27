using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GoogleSheetsToUnity;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using MikroFramework.TimeSystem;
using Mirror;
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
using Steamworks;
using Mirror.FizzySteam;
#endif


using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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

        [SerializeField]
        private List<string> GameModeSceneNames;
        

        private TelepathyTransport telepathyTransport;
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
        private FizzySteamworks steamworksTransport;

        protected Callback<LobbyCreated_t> OnSteamLobbyCreatedEvent;
        protected Callback<LobbyEnter_t> OnSteamLobbyEnteredEvent;

        private CSteamID joinedSteamGame;
#endif

        private ResLoader resLoader;
        private string mapSceneName;

        
        public override void Awake() {
            base.Awake();
            this.RegisterEvent<OnGoodsPropertiesUpdated>(OnGoodsPropertiesUpdated)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            ResLoader.Create((loader => resLoader = loader));
            StartCoroutine(LoadScenes());

            
            if (Application.isEditor)
                Application.runInBackground = true;
            networkAddress = NetworkUtility.GetLocalIPAddress();
            showRoomGUI = false;
            telepathyTransport = GetComponent<TelepathyTransport>();
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
            steamworksTransport = GetComponent<FizzySteamworks>();

            OnSteamLobbyCreatedEvent = Callback<LobbyCreated_t>.Create(OnSteamLobbyCreated);
            OnSteamLobbyEnteredEvent = Callback<LobbyEnter_t>.Create(OnSteamLobbyEntered);
         
            if (SteamManager.Initialized) {
                GCHandle gchandle = GCHandle.Alloc(128 * 128, GCHandleType.Pinned);
                SteamNetworkingUtils.SetConfigValue(
                    ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize,
                    ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
                    IntPtr.Zero,
                    ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                    gchandle.AddrOfPinnedObject());
                gchandle.Free();                
            }
#endif
            List<GoodsPropertiesItem> allGoodsProperties =
                this.GetModel<IGoodsConfigurationModel>().GetAllGoodsProperties();
            spawnPrefabs.AddRange(allGoodsProperties.Select((item => item.GoodsPrefab)));

        }

        private void OnGoodsPropertiesUpdated(OnGoodsPropertiesUpdated e) {
            List<GoodsPropertiesItem> allGoodsProperties = e.allItemProperties;
            foreach (GoodsPropertiesItem goodsProperties in allGoodsProperties) {
                spawnPrefabs.Add(goodsProperties.GoodsPrefab);
            }
            
        }


        private IEnumerator LoadScenes() {
            yield return new WaitForSeconds(1f);
            while (resLoader ==null ||  !resLoader.IsReady || !ResData.Exists) {
                yield return null;
            }
          
            resLoader.LoadSync<AssetBundle>("map");
        }


        public void ServerChangeGameModeScene(GameMode gamemode) {
            mapSceneName = GameModeSceneNames[(int)gamemode].ToString();
        }
        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer) {
            NetworkedMenuRoomPlayer player = roomPlayer.GetComponent<NetworkedMenuRoomPlayer>();
            int team = player.MatchInfo.Team;
            int teamIndex = player.MatchInfo.TeamIndex;
            Vector2 startPos = Vector2.zero;
            if (GameObject.FindGameObjectsWithTag("Team1Spawn").Length > 0) {
                if (team == 1)
                {
                    startPos = GameObject.FindGameObjectsWithTag("Team1Spawn")[teamIndex].transform.position;
                }
                else if (team == 2)
                {
                    startPos = GameObject.FindGameObjectsWithTag("Team2Spawn")[teamIndex].transform.position;
                }
            }
           

            GameObject gamePlayer = startPos != null
                ? Instantiate(playerPrefab, startPos, Quaternion.identity)
                : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            return gamePlayer;
        }
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
        private void OnSteamLobbyEntered(LobbyEnter_t callback) {
            if (!NetworkServer.active && !NetworkClient.active) {

                CSteamID steamID = new CSteamID(callback.m_ulSteamIDLobby);
                string hostAddress = SteamMatchmaking.GetLobbyData(steamID,
                    "HostAddress");
                networkAddress = hostAddress;
                Debug.Log(hostAddress);
                StartJoiningClient(NetworkingMode.Steam, steamID);
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

          //  StartCoroutine(LoadScene());
        }
#endif

#region Server
        
        public override void OnRoomServerConnect(NetworkConnectionToClient conn) {
            base.OnRoomServerConnect(conn);
            
        }

        public override void OnStopServer() {
            base.OnStopServer();
        }

        public override void OnStopClient() {
            base.OnStopClient();
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
            if (NetworkingMode == NetworkingMode.Steam) {
                SteamMatchmaking.LeaveLobby(joinedSteamGame);
                joinedSteamGame = new CSteamID();
                DebugCanvas.IsOpening = false;
                
            }
#endif
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
            else {
                
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

        protected override void SceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer) {
            Debug.Log($"NetworkRoom SceneLoadedForPlayer scene: {SceneManager.GetActiveScene().path} {conn}");

            if (IsSceneActive(RoomScene))
            {
                // cant be ready in room, add to ready list
                PendingPlayer pending;
                pending.conn = conn;
                pending.roomPlayer = roomPlayer;
                pendingPlayers.Add(pending);
                return;
            }

            StartCoroutine(CreatePlayer(conn, roomPlayer));
        }

        private IEnumerator CreatePlayer(NetworkConnectionToClient conn, GameObject roomPlayer) {
            while (!subscenesLoaded)
                yield return null;

            GameObject gamePlayer = OnRoomServerCreateGamePlayer(conn, roomPlayer);
            if (gamePlayer == null)
            {
                // get start position from base class
                Transform startPos = GetStartPosition();
                gamePlayer = startPos != null
                    ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                    : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            }

            if (!OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer))
                yield break;

            // replace room player with game player
            NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, true);
        }

        public override void OnServerSceneChanged(string sceneName) {
            base.OnServerSceneChanged(sceneName);
            if (sceneName == RoomScene || sceneName == offlineScene) {
                IsInGame = false;
            }
            else if (sceneName == GameplayScene) {
                IsInGame = true;
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
                if (NetworkingMode == NetworkingMode.Steam) {
                    SteamMatchmaking.SetLobbyData(joinedSteamGame, "IsGaming",
                        "true");
                }
#endif
                StartCoroutine(ServerLoadSubScenes());
              
                //StartCoroutine(LoadScene());
            }
        }

        private bool subscenesLoaded = false;
        IEnumerator ServerLoadSubScenes() {
            yield return SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Additive);


            subscenesLoaded = true;
        }
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
            if (sceneOperation == SceneOperation.UnloadAdditive)
                StartCoroutine(UnloadAdditive(newSceneName));

            if (sceneOperation == SceneOperation.LoadAdditive && mode != NetworkManagerMode.ClientOnly) {
                StartCoroutine(LoadAdditive(newSceneName));
            }else if (sceneOperation == SceneOperation.Normal && mode == NetworkManagerMode.ClientOnly) {
                StartCoroutine(LoadAdditive(newSceneName));
            }
               
            
        }


        bool isInTransition;



        IEnumerator LoadAdditive(string sceneName) {
            isInTransition = true;
            
            // host client is on server...don't load the additive scene again
            if (mode == NetworkManagerMode.ClientOnly)
            {
                if (sceneName == GameplayScene) {
                    // Start loading the additive subscene
                    yield return null;
                    loadingSceneAsync = SceneManager.LoadSceneAsync(
                        GameModeSceneNames[(int) this.GetSystem<IRoomMatchSystem>().GameMode].ToString(),
                        LoadSceneMode.Additive);

                    while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                        yield return null;
                }
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
            isInTransition = false;
            OnClientSceneChanged();

        }

        IEnumerator UnloadAdditive(string sceneName)
        {
            isInTransition = true;

            if (mode == NetworkManagerMode.ClientOnly)
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
                yield return Resources.UnloadUnusedAssets();
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
            isInTransition = false;
            OnClientSceneChanged();

            // There is no call to FadeOut here on purpose.
            // Expectation is that a LoadAdditive will follow
            // that will call FadeOut after that scene loads.
        }


        public override void OnClientSceneChanged()
        {
            //Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientSceneChanged {isInTransition}");

            // Only call the base method if not in a transition.
            // This will be called from DoTransition after setting doingTransition to false
            // but will also be called first by Mirror when the scene loading finishes.
            if (!isInTransition)
                base.OnClientSceneChanged();
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
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
            if (NetworkingMode == NetworkingMode.Steam) {
                SteamMatchmaking.LeaveLobby(joinedSteamGame);
                joinedSteamGame = new CSteamID();
            }
#endif
        }

        public void StartHosting(NetworkingMode networkingMode) {
            telepathyTransport.enabled = false;
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
            if (steamworksTransport)
            {
                steamworksTransport.enabled = false;
            }
#endif
            offlineScene = SceneManager.GetSceneByName("Menu").name;
            switch (networkingMode) {
                case NetworkingMode.Normal:
                    telepathyTransport.enabled = true;
                    Transport.activeTransport = telepathyTransport;                    
                    transport = telepathyTransport;
                    telepathyTransport.port = (ushort)Random.Range(7777, 15000);
                    NetworkingMode = NetworkingMode.Normal;
                    StartHost();
                 //   StartCoroutine(LoadScene());
                    break;
                case NetworkingMode.Steam:
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
                    steamworksTransport.enabled = true;
                    Transport.activeTransport = steamworksTransport;
                    transport = steamworksTransport;
                    SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxConnections);
                    NetworkingMode = NetworkingMode.Steam;
#endif
                    break;
            }
        }
        
        

        public void StartJoiningClient(Uri uri) {
#if !DISABLESTEAMWORKS && !UNITY_ANDROID
            if (steamworksTransport)
            {
                steamworksTransport.enabled = false;
            }
#endif
            telepathyTransport.enabled = true;
            Transport.activeTransport = telepathyTransport;
            transport = telepathyTransport;

            NetworkingMode = NetworkingMode.Normal;
            offlineScene = SceneManager.GetSceneByName("Menu").name;
            if (NetworkClient.active)
            {
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
            }

            StartClient(uri);
        }


#if !DISABLESTEAMWORKS && !UNITY_ANDROID
        public void StartJoiningClient(NetworkingMode networkingMode, CSteamID steamId = new CSteamID())
#else
        public void StartJoiningClient(NetworkingMode networkingMode)
#endif
        {
            switch (networkingMode)
            {
                case NetworkingMode.Normal:
#if !DISABLESTEAMWORKS && !UNITY_ANDROID

                    if (steamworksTransport)
                    {
                        steamworksTransport.enabled = false;
                    }
#endif
                    networkAddress = "localhost";//NetworkUtility.GetLocalIPAddress();
                    telepathyTransport.enabled = true;
                    Transport.activeTransport = telepathyTransport;
                    transport = telepathyTransport;
                   
                    NetworkingMode = NetworkingMode.Normal;
                   
                    break;
                case NetworkingMode.Steam:
#if !DISABLESTEAMWORKS && !UNITY_ANDROID

                    telepathyTransport.enabled = false;                    
                    steamworksTransport.enabled = true;
                    Transport.activeTransport = steamworksTransport;
                    transport = steamworksTransport;
                    NetworkingMode = NetworkingMode.Steam;
                    joinedSteamGame = steamId;
#endif
                    break;
            }
            if (NetworkClient.active)
            {
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
            }
            offlineScene = SceneManager.GetSceneByName("Menu").name;
            StartClient();
        }




        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}

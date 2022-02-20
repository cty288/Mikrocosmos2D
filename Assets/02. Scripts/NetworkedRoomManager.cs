using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnAllPlayersReadyStatusChanged {
        public bool IsAllPlayerReady;
    }

    public class NetworkedRoomManager : NetworkRoomManager, IController, ICanSendEvent {
        [SerializeField] private GameObject matchSystemPrefab;

        public bool IsInGame = false;

        public override void Awake() {
            base.Awake();
            networkAddress = NetworkUtility.GetLocalIPAddress();
            showRoomGUI = false;
        }

        #region Server

        public override void OnRoomStartServer() {
            base.OnRoomStartServer();
            GetComponent<MenuNetworkDiscovery>().AdvertiseServer();
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
            }else if (sceneName == GameplayScene) {
                IsInGame = true;
            }
        }

        public string GetHostName() {
            if (NetworkServer.active) {
               return  this.GetSystem<IRoomMatchSystem>().ServerGetHostInfo().Name;
            }

            return "";
        }
     
        #endregion

       

        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}

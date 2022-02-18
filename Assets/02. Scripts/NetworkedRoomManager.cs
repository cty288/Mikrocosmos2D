using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{

    public class NetworkedRoomManager : NetworkRoomManager, IController, ICanSendEvent {
        [SerializeField] private GameObject matchSystemPrefab;
        
        public override void Awake() {
            base.Awake();
            networkAddress = NetworkUtility.GetLocalIPAddress();
            showRoomGUI = false;
        }

        #region Server

        public override void OnRoomStartServer() {
            base.OnRoomStartServer();
            
            GameObject matchSystem = Instantiate(matchSystemPrefab);
            NetworkServer.Spawn(matchSystem);
            GameObject.DontDestroyOnLoad(matchSystem.gameObject);
            
        }


        
        #endregion

       

        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}

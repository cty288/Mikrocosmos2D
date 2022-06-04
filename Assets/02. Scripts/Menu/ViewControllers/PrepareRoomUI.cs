using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine.SceneManagement;

namespace Mikrocosmos {
	public partial class PrepareRoomUI : AbstractMikroController<Mikrocosmos> {
        private TelepathyTransport transport;

        private void Awake() {
            this.RegisterEvent<OnClientPrepareRoomPlayerListChange>(OnClientPrepareRoomPlayerListChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnAllPlayersReadyStatusChanged>(OnAllPlayerReadyStatusChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            BtnChangeSide.onClick.AddListener(OnSwitchSideClicked);
            BtnTestMode.onClick.AddListener(OnTestModeButtonClicked);
            BtnRoomLeaderStartRoom.onClick.AddListener(OnHostStartGameButtonClicked);
            BtnBack.onClick.AddListener(OnBackToMenuClicked);
            BtnRoomLeaderStartRoom.gameObject.SetActive(false);
         
        }

        private void Start() {
            transport = NetworkManager.singleton.GetComponent<TelepathyTransport>();
        }

        

        private void OnBackToMenuClicked() {
           QuitRoom();
           Invoke(nameof(QuitRoom), 0.3f);
            //SceneManager.LoadScene(0);
        }

        private void QuitRoom() {
            if (NetworkServer.active)
            {
                NetworkServer.DisconnectAll();
                NetworkServer.Shutdown();
                NetworkServer.DisconnectAll();
                NetworkServer.Shutdown();
            }
            else if (NetworkClient.active)
            {
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
        private void OnTestModeButtonClicked() {
            if (NetworkServer.active) {
                this.SendCommand<ServerStartGameCommand>();
            }
        }

        private void OnHostStartGameButtonClicked() {
            if (NetworkServer.active) {
                this.SendCommand<ServerStartGameCommand>();
            }
        }

        private void Update() {
            if (((NetworkedRoomManager) NetworkManager.singleton).NetworkingMode == NetworkingMode.Normal) {
                TextPort.text = $"Port: {transport.port}";
            }
        
        }

        //only called on the host
        private void OnAllPlayerReadyStatusChange(OnAllPlayersReadyStatusChanged e) {
            Debug.Log($"All players ready: {e.IsAllPlayerReady}");
            if (NetworkServer.active) {
                if (!e.IsAllPlayerReady) {
                    BtnRoomLeaderStartRoom.gameObject.SetActive(false);
                }
                else {
                    bool isTwoSidesEqual = this.GetSystem<IRoomMatchSystem>().ServerGetIsTeamSizeEqual();
                    BtnRoomLeaderStartRoom.gameObject.SetActive(isTwoSidesEqual);
                }
            }
        }

        private void OnSwitchSideClicked() {
            this.SendCommand<ClientSwitchSideCommand>();
        }

        private void OnClientPrepareRoomPlayerListChange(OnClientPrepareRoomPlayerListChange e) {
            int leftIndex = 0, rightIndex = 0;
            if (this && e.MatchInfos != null) {
                foreach (PlayerMatchInfo playerMatchInfo in e.MatchInfos)
                {
                    GameObject infoObj;
                    if (playerMatchInfo.Team == 1)
                    {
                        infoObj = ObjTeam1Layout.transform.GetChild(leftIndex).gameObject;
                        leftIndex++;
                    }
                    else
                    {
                        infoObj = ObjTeam2Layout.transform.GetChild(rightIndex).gameObject;
                        rightIndex++;
                    }
                    infoObj.SetActive(true);
                    bool isSelf = playerMatchInfo.ID == e.SelfInfo.ID;
                    infoObj.GetComponent<PlayerInfo>().SetInfo(playerMatchInfo.ID, playerMatchInfo.Name,
                        playerMatchInfo.Prepared, isSelf, e.IsHost);
                    if (isSelf)
                    {
                        infoObj.transform.SetAsFirstSibling();
                        BtnChangeSide.gameObject.SetActive(!playerMatchInfo.Prepared);
                    }
                }

                for (int i = leftIndex; i < ObjTeam1Layout.transform.childCount; i++)
                {
                    ObjTeam1Layout.transform.GetChild(i).gameObject.SetActive(false);
                }
                for (int i = rightIndex; i < ObjTeam2Layout.transform.childCount; i++)
                {
                    ObjTeam2Layout.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
           
        }
    }
}
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
using Polyglot;
using UnityEngine.SceneManagement;



namespace Mikrocosmos {

	public partial class PrepareRoomUI : AbstractMikroController<Mikrocosmos> {
        private TelepathyTransport transport;
        
        private List<string> gamemodeLocalizedKeys = new List<string>() {
            "MENU_GAME_MODE_STANDARD",
            "MENU_GAME_MODE_TUTORIAL"
        };
        
        private void Awake() {
            this.RegisterEvent<OnClientPrepareRoomPlayerListChange>(OnClientPrepareRoomPlayerListChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnAllPlayersReadyStatusChanged>(OnAllPlayerReadyStatusChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientGameModeChanged>(OnClientGameModeChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            BtnChangeSide.onClick.AddListener(OnSwitchSideClicked);
            BtnTestMode.onClick.AddListener(OnTestModeButtonClicked);
            BtnRoomLeaderStartRoom.onClick.AddListener(OnHostStartGameButtonClicked);
            BtnBack.onClick.AddListener(OnBackToMenuClicked);
            BtnRoomLeaderStartRoom.gameObject.SetActive(false);
            DropdownGameMode.interactable = NetworkServer.active;
            DropdownGameMode.onValueChanged.AddListener(OnDropDownValueChanged);
        }

        private void OnClientGameModeChanged(OnClientGameModeChanged e) {
            int option = (int) e.NewGameMode;
            DropdownGameMode.value = option;
        }

        private void OnDropDownValueChanged(int choice) {
            if (NetworkServer.active) {
                this.GetSystem<IRoomMatchSystem>().ServerChangeGameMode((GameMode)choice);
            }
        }

        private void Start() {
            transport = NetworkManager.singleton.GetComponent<TelepathyTransport>();
            foreach (string gamemodeLocalizedKey in gamemodeLocalizedKeys) {
                DropdownGameMode.options.Add(new TMP_Dropdown.OptionData(Localization.Get(gamemodeLocalizedKey)));
            }
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
                this.SendCommand<ServerStartGameCommand>(new ServerStartGameCommand(this.GetSystem<IRoomMatchSystem>().GameMode));
            }
        }

        private void OnHostStartGameButtonClicked() {
            if (NetworkServer.active) {
                this.SendCommand<ServerStartGameCommand>(new ServerStartGameCommand(this.GetSystem<IRoomMatchSystem>().GameMode));
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
                    bool canStartGame = this.GetSystem<IRoomMatchSystem>().ServerGetIsStartGameConditionSatisfied();
                    BtnRoomLeaderStartRoom.gameObject.SetActive(canStartGame);
                }
            }
        }

        private void OnSwitchSideClicked() {
            this.SendCommand<ClientSwitchSideCommand>();
        }

        private void OnClientPrepareRoomPlayerListChange(OnClientPrepareRoomPlayerListChange e) {
            int team1Index = 0, team2Index = 0;
            if (this && e.MatchInfos != null) {
                foreach (PlayerMatchInfo playerMatchInfo in e.MatchInfos)
                {
                    GameObject infoObj;
                    if (playerMatchInfo.Team == 1) {
                        infoObj = ObjTeam1Layout.transform.GetChild(team1Index).gameObject;
                        ObjTeam1SlotsBG.transform.GetChild(team1Index).gameObject.SetActive(false);
                        team1Index++;
                    }
                    else {
                        infoObj = ObjTeam2Layout.transform.GetChild(team2Index).gameObject;
                        ObjTeam2SlotsBG.transform.GetChild(team1Index).gameObject.SetActive(false);
                        team2Index++;
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

                for (int i = team1Index; i < ObjTeam1Layout.transform.childCount; i++) {
                    ObjTeam1Layout.transform.GetChild(i).gameObject.SetActive(false);
                    ObjTeam1SlotsBG.transform.GetChild(i).gameObject.SetActive(true);
                }
                
                for (int i = team2Index; i < ObjTeam2Layout.transform.childCount; i++) {
                    ObjTeam2Layout.transform.GetChild(i).gameObject.SetActive(false);
                    ObjTeam2SlotsBG.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
           
        }
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;

namespace Mikrocosmos {
	public partial class PrepareRoomUI : AbstractMikroController<Mikrocosmos> {
        

        private void Awake() {
            this.RegisterEvent<OnClientPrepareRoomPlayerListChange>(OnClientPrepareRoomPlayerListChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnAllPlayersReadyStatusChanged>(OnAllPlayerReadyStatusChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            BtnChangeSide.onClick.AddListener(OnSwitchSideClicked);
            BtnRoomLeaderStartRoom.onClick.AddListener(OnHostStartGameButtonClicked);
            BtnRoomLeaderStartRoom.gameObject.SetActive(false);
        }

        private void OnHostStartGameButtonClicked() {
            if (NetworkServer.active) {
                NetworkRoomManager.singleton.ServerChangeScene(((NetworkRoomManager) NetworkRoomManager.singleton)
                    .GameplayScene);
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
            
            foreach (PlayerMatchInfo playerMatchInfo in e.MatchInfos) {
                GameObject infoObj;
                if (playerMatchInfo.Team == 1) {
                    infoObj = ObjTeam1Layout.transform.GetChild(leftIndex).gameObject;
                    leftIndex++;
                }
                else {
                    infoObj = ObjTeam2Layout.transform.GetChild(rightIndex).gameObject;
                    rightIndex++;
                }
                infoObj.SetActive(true);
                bool isSelf = playerMatchInfo.ID == e.SelfInfo.ID;
                infoObj.GetComponent<PlayerInfo>().SetInfo(playerMatchInfo.ID, playerMatchInfo.Name,
                    playerMatchInfo.Prepared, isSelf, e.IsHost);
                if (isSelf) {
                    infoObj.transform.SetAsFirstSibling();
                    BtnChangeSide.gameObject.SetActive(!playerMatchInfo.Prepared);
                }
            }

            for (int i = leftIndex; i < ObjTeam1Layout.transform.childCount; i++) {
                ObjTeam1Layout.transform.GetChild(i).gameObject.SetActive(false);
            }
            for (int i = rightIndex; i < ObjTeam2Layout.transform.childCount; i++)
            {
                ObjTeam2Layout.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
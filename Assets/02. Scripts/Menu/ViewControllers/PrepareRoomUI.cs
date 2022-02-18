using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;

namespace Mikrocosmos {
	public partial class PrepareRoomUI : AbstractMikroController<Mikrocosmos> {
        

        private void Awake() {
            this.RegisterEvent<OnClientPrepareRoomPlayerListChange>(OnClientPrepareRoomPlayerListChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            BtnChangeSide.onClick.AddListener(OnSwitchSideClicked);
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
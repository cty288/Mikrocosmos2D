using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using Mirror;

namespace Mikrocosmos {
	public partial class RoomInfo : AbstractMikroController<Mikrocosmos> {
        private DiscoveryResponse response;
        public void SetRoomInfo(DiscoveryResponse response) {
            this.response = response;
            TextRoomName.text = response.HostName + " 的房间";
            TextRoomNumber.text = $"{response.ServerPlayerNum}/{response.ServerMaxPlayerNum}";
            if (response.ServerPlayerNum >= response.ServerMaxPlayerNum || response.IsGaming) {
                BtnJoinButton.gameObject.SetActive(false);
                TextRoomStatus.gameObject.SetActive(true);
                if (response.IsGaming) {
                    TextRoomStatus.text = "游戏中";
                }
                else if (response.ServerPlayerNum >= response.ServerMaxPlayerNum) {
                    TextRoomStatus.text = "房间已满";
                }
            }
            else {
                BtnJoinButton.gameObject.SetActive(true);
                TextRoomStatus.gameObject.SetActive(false);
            }
        }

        private void Awake() {
            BtnJoinButton.onClick.AddListener(OnJoinButtonClicked);
        }

        private void OnJoinButtonClicked() {
            if (response != null) {
                if (response.IsGaming || response.ServerPlayerNum >= response.ServerMaxPlayerNum) {
                    return;
                }

                NetworkManager.singleton.StartClient(response.Uri);
            }
        }
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using Mirror;
using Steamworks;

namespace Mikrocosmos {
	public partial class RoomInfo : AbstractMikroController<Mikrocosmos> {
        private DiscoveryResponse response;
        
        public void SetRoomInfo(DiscoveryResponse response) {
            this.response = response;
            TextRoomName.text = response.HostName + " �ķ���";
            TextRoomNumber.text = $"{response.ServerPlayerNum}/{response.ServerMaxPlayerNum}";
            if (response.ServerPlayerNum >= response.ServerMaxPlayerNum || response.IsGaming) {
                BtnJoinButton.gameObject.SetActive(false);
                TextRoomStatus.gameObject.SetActive(true);
                if (response.IsGaming) {
                    TextRoomStatus.text = "��Ϸ��";
                }
                else if (response.ServerPlayerNum >= response.ServerMaxPlayerNum) {
                    TextRoomStatus.text = "��������";
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
          
                if (response.IsGaming || response.ServerPlayerNum >= response.ServerMaxPlayerNum) {
                    return;
                }

                if (response.IsLAN) {
                    NetworkManager.singleton.GetComponent<TelepathyTransport>().port = (ushort)response.Uri.Port;
                    ((NetworkedRoomManager)NetworkManager.singleton).StartJoiningClient(NetworkingMode.Normal);
                }
                else {
                    SteamMatchmaking.JoinLobby(response.HostSteamLobbyID);
                }
           
            
        }
    }
}
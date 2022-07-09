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
    public struct OnJoinRoomButtonClicked {

    }
	public partial class RoomInfo : AbstractMikroController<Mikrocosmos>, ICanSendEvent {
        [SerializeField]
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
            if (response.IsGaming || response.ServerPlayerNum >= response.ServerMaxPlayerNum) {
                return;
            }

            if (response.IsLAN) {
                NetworkManager.singleton.GetComponent<TelepathyTransport>().port = (ushort)response.Uri.Port;
                ((NetworkedRoomManager)NetworkManager.singleton).StartJoiningClient(response.Uri);
            }
            else {
                SteamMatchmaking.JoinLobby(response.HostSteamLobbyID);
            }
            this.SendEvent(new OnJoinRoomButtonClicked());

        }
    }
}
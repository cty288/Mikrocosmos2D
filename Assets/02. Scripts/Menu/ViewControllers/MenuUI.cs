using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using Random = UnityEngine.Random;

namespace Mikrocosmos {
	public partial class MenuUI : AbstractMikroController<Mikrocosmos> {
        private void Awake() {
            BtnNameConfirmButton.onClick.AddListener(OnNameButtonConfirmClicked);
           // BtnHostGame.onClick.AddListener(OnHostGameButtonClicked);
            BtnFindGame.onClick.AddListener(OnFindGameButtonClicked);
            BtnHostLocalNetwork.onClick.AddListener(OnHostLocalNetwork);
            BtnRoomSearchPanelBackToMenu.onClick.AddListener(OnRoomSearchBackToMenu);
            this.RegisterEvent<ChangeNameSuccess>(OnNameChangeSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
           BtnHostSteam.onClick.AddListener(OnHostSteam);
        }

        private void OnHostSteam() {
            ((NetworkedRoomManager)(NetworkManager.singleton)).StartHosting(NetworkingMode.Steam);
        }

        private void OnHostLocalNetwork() {
            ((NetworkedRoomManager)(NetworkManager.singleton)).StartHosting(NetworkingMode.Normal);
        }

        private void OnRoomSearchBackToMenu() {
            ObjFindServerPanel.SetActive(false);
            ObjMenuPanel.SetActive(true);
            this.SendCommand<CmdRequestStopNetworkDiscoveryCommand>();
        }

        private void Start()
        {
            Debug.Log(NetworkUtility.GetLocalIPAddress());
            ShowInitialPanel();
        }
        private void OnFindGameButtonClicked() {
            //NetworkRoomManager.singleton.StartClient();
            ObjFindServerPanel.SetActive(true);
            ObjMenuPanel.SetActive(false);
            this.SendCommand<ClientRequestNetworkDiscoveryCommand>();
        }



        private void OnNameChangeSuccess(ChangeNameSuccess obj) {
            ObjNewPlayerPanelParent.SetActive(false);
            ObjMenuPanel.SetActive(true);
        }
       
        private void OnNameButtonConfirmClicked() {
            Debug.Log("Name Button Confirmed");
            this.SendCommand<ChangeDisplayNameCommand>(new ChangeDisplayNameCommand(InputNameInput.text));
            
        }

      

        private void ShowInitialPanel() {
            ILocalPlayerInfoModel playerInfoModel = this.GetModel<ILocalPlayerInfoModel>();
            if (string.IsNullOrEmpty(playerInfoModel.NameInfo.Value)) {
                ObjNewPlayerPanelParent.SetActive(true);
            }
            else {
                ObjMenuPanel.SetActive(true);
            }
        }
    }
}
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using Random = UnityEngine.Random;

namespace Mikrocosmos {
	public partial class MenuUI : AbstractMikroController<Mikrocosmos> {
        [SerializeField] private GameObject CreateAvatarPanel;
        [SerializeField] private Image avatarPanelOpenBG;
        [SerializeField] private AvatarElementViewController avatarElement;
        private IClientAvatarModel avatarModel;
        private void Awake() {
            //BtnNameConfirmButton.onClick.AddListener(OnNameButtonConfirmClicked);
           // BtnHostGame.onClick.AddListener(OnHostGameButtonClicked);
            BtnFindGame.onClick.AddListener(OnFindGameButtonClicked);
            BtnHostLocalNetwork.onClick.AddListener(OnHostLocalNetwork);
            BtnRoomSearchPanelBackToMenu.onClick.AddListener(OnRoomSearchBackToMenu);
            //this.RegisterEvent<ChangeNameSuccess>(OnNameChangeSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
           BtnHostSteam.onClick.AddListener(OnHostSteam);
           avatarElement.GetComponentInChildren<AvatarSelectionElementButton>().OnSelected.AddListener(OnAvatarClicked);
           avatarModel = this.GetModel<IClientAvatarModel>();
           this.RegisterEvent<OnClientAvatarSet>(OnAvatarSet).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnAvatarSet(OnClientAvatarSet e) {
            Avatar avatar = e.avatar;
            if (avatar.Elements.Count > 0)
            {
                avatarElement.SetAvatar(avatar);
            }
        }

        private void OnAvatarClicked() {
            OpenAvatarPanel();
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
            Avatar avatar = avatarModel.Avatar;
            if (avatar.Elements.Count > 0) {
                avatarElement.SetAvatar(avatar);
            }
        }
        private void OnFindGameButtonClicked() {
            //NetworkRoomManager.singleton.StartClient();
            ObjFindServerPanel.SetActive(true);
            ObjMenuPanel.SetActive(false);
            this.SendCommand<ClientRequestNetworkDiscoveryCommand>();
        }


        /*
        private void OnNameChangeSuccess(ChangeNameSuccess obj) {
            ObjNewPlayerPanelParent.SetActive(false);
            ObjMenuPanel.SetActive(true);
        }*/
       
        private void OnNameButtonConfirmClicked() {
            Debug.Log("Name Button Confirmed");
            this.SendCommand<ChangeDisplayNameCommand>(new ChangeDisplayNameCommand(InputNameInput.text));
            
        }

        public void CloseAvatarPanel() {
            avatarPanelOpenBG.GetComponent<Animator>().SetTrigger("Play");
            this.GetSystem<ITimeSystem>().AddDelayTask(0.5f, () => {
                CreateAvatarPanel.SetActive(false);
                ObjMenuPanel.SetActive(true);
            });
        }

        private void ShowInitialPanel() {
            ILocalPlayerInfoModel playerInfoModel = this.GetModel<ILocalPlayerInfoModel>();
            if (string.IsNullOrEmpty(playerInfoModel.NameInfo.Value)) {
                OpenAvatarPanel();
            }
            else {
                ObjMenuPanel.SetActive(true);
            }
        }

        private void OpenAvatarPanel() {
            avatarPanelOpenBG.GetComponent<Animator>().SetTrigger("Play");
            this.GetSystem<ITimeSystem>().AddDelayTask(0.5f, () => {
                CreateAvatarPanel.SetActive(true);
            });
        }
    }
}
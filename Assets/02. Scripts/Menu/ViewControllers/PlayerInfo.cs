using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using Mirror;

namespace Mikrocosmos {
	public partial class PlayerInfo : AbstractMikroController<Mikrocosmos> {
        private int id = -1;
        private bool prepared = false;
        private AvatarElementViewController avatarElement;
        private void Awake() {
            BtnPrepare.onClick.AddListener(OnPrepareClicked);
            BtnUnPrepare.onClick.AddListener(OnPrepareClicked);
            BtnKick.onClick.AddListener(OnKickButtonClicked);
            avatarElement = GetComponentInChildren<AvatarElementViewController>();
        }

        private void OnKickButtonClicked() {
            this.SendCommand<ClientKickPlayerCommand>(new ClientKickPlayerCommand(id));
        }

        private void OnPrepareClicked() {
            this.SendCommand<ClientPrepareCommand>();
        }

        public void SetInfo(int id, string name, Avatar avatar, bool isPrepared, bool isSelf, bool isHost) {
            this.id = id;
            TextName.text = name;
            prepared = isPrepared;
            avatarElement.SetAvatar(avatar);
            if (isSelf) {
                
                BtnKick.gameObject.SetActive(false);
                //TextReadyStatus.gameObject.SetActive(false);
                BtnUnPrepare.interactable = true;
                if (prepared) {
                    BtnUnPrepare.gameObject.SetActive(true);
                    BtnPrepare.gameObject.SetActive(false);
                }
                else {
                    BtnPrepare.gameObject.SetActive(true);
                    BtnUnPrepare.gameObject.SetActive(false);
                }
            }
            else {
              //  TextReadyStatus.gameObject.SetActive(true);
                BtnPrepare.gameObject.SetActive(false);
                BtnUnPrepare.interactable = false;
                BtnUnPrepare.gameObject.SetActive(isPrepared);
                
              //  TextReadyStatus.text = (isPrepared ? "Prepared" : "UnPrepared");
                if (isHost) { //host
                    BtnKick.gameObject.SetActive(true);
                }
                else {
                    BtnKick.gameObject.SetActive(false);
                }
            }
        }
	}
}
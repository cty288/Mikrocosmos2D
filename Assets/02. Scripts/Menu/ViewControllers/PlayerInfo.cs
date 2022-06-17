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

        private void Awake() {
            BtnPrepare.onClick.AddListener(OnPrepareClicked);
            BtnKick.onClick.AddListener(OnKickButtonClicked);
        }

        private void OnKickButtonClicked() {
            this.SendCommand<ClientKickPlayerCommand>(new ClientKickPlayerCommand(id));
        }

        private void OnPrepareClicked() {
            this.SendCommand<ClientPrepareCommand>();
        }

        public void SetInfo(int id, string name, bool isPrepared, bool isSelf, bool isHost) {
            this.id = id;
            TextName.text = name;
            prepared = isPrepared;
            if (isSelf) {
                BtnPrepare.gameObject.SetActive(true);
                BtnKick.gameObject.SetActive(false);
                TextReadyStatus.gameObject.SetActive(false);

                if (prepared) {
                    BtnPrepare.GetComponentInChildren<TMP_Text>().text = "Cancel";
                }
                else {
                    BtnPrepare.GetComponentInChildren<TMP_Text>().text = "Prepare";
                }
            }
            else {
                TextReadyStatus.gameObject.SetActive(true);
                BtnPrepare.gameObject.SetActive(false);
                TextReadyStatus.text = (isPrepared ? "Prepared" : "UnPrepared");
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
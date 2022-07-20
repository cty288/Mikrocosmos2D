using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class MiniChatMessagePanel : AbstractMikroController<Mikrocosmos> {
        private TMP_Text messageText;

        [SerializeField] private float fadeTime = 8;

        private float fadeTimer;
        private bool faded = false;

        private PlayerMatchInfo matchInfo;
        private void Awake() {
            messageText = GetComponentInChildren<TMP_Text>(true);
            fadeTimer = fadeTime;
        }

        
        private void Update() {
            fadeTimer -= Time.deltaTime;
            if (fadeTimer <= 0) {
                if (!faded) {
                    faded = true;
                    messageText.DOFade(0, 1f);
                }
            }
        }

        private void Start() {
            this.RegisterEvent<OnClientReceiveMessage>(OnReceiveMessage).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnLogMessage>(OnLogMessage).UnRegisterWhenGameObjectDestroyed(gameObject);
            matchInfo = this.GetSystem<IRoomMatchSystem>().ClientGetMatchInfoCopy();
        }
        
        private void OnLogMessage(OnLogMessage e) {
            fadeTimer = fadeTime;
            faded = false;
            messageText.DOFade(1, 0);
            
            messageText.text += e.message;
            messageText.text += "\n";
        }

        private void OnReceiveMessage(OnClientReceiveMessage e) {
            fadeTimer = fadeTime;
            faded = false;
            messageText.DOFade(1, 0);
            bool isSameTeam = e.Team == matchInfo.Team;
            string output = "";
            if (isSameTeam) {
                output += $"<color=green></b>{e.Name}: </b></color>";
            }
            else {
                output += $"<color=red></b>{e.Name}: </b></color>";
            }

            output += e.Message;
            output += "\n";
            messageText.text += output;
        }
    }
}

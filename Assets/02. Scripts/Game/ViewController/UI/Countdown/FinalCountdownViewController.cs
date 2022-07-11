using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Polyglot;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class FinalCountdownViewController : AbstractMikroController<Mikrocosmos> {

        private Animator animator;
        private TMP_Text finalCountdownText;
        private TMP_Text playerWinText;

        private void Awake() {
            animator = GetComponent<Animator>();
            finalCountdownText = transform.Find("FinalCountdownTimer").GetComponent<TMP_Text>();
            playerWinText = transform.Find("PlayerWinText").GetComponent<TMP_Text>();

            this.RegisterEvent<OnClientFinalCountdownTimerStart>(OnFinalCountdownStart)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientFinalCountDownTimerEnds>(OnFinalCountdownEnd)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientFinalCountdownTimerChange>(OnFinalCountdownChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

         

            this.RegisterEvent<OnTieTimerStart>(OnTieTimerStart).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientGameEnd>(OnClientGameEnd).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientGameEnd(OnClientGameEnd obj) {
            finalCountdownText.gameObject.SetActive(false);
        }

        private void OnTieTimerStart(OnTieTimerStart e) {
            playerWinText.gameObject.SetActive(true);
            playerWinText.text = "";
            playerWinText.text += $"{Localization.Get("GAME_COUNTDOWN_TIE")}";
            this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => playerWinText.gameObject.SetActive(false));
        }

        private void OnFinalCountdownChange(OnClientFinalCountdownTimerChange e) {
            finalCountdownText.text = e.Time.ToString();
        }

        private void OnFinalCountdownStart(OnClientFinalCountdownTimerStart e) {
            finalCountdownText.gameObject.SetActive(true);
            animator.SetTrigger("Start");
        }

        private void OnFinalCountdownEnd(OnClientFinalCountDownTimerEnds e) {
            finalCountdownText.gameObject.SetActive(false);
            playerWinText.gameObject.SetActive(true);
            playerWinText.text = "";
            for (int i = 0; i < e.WinNames.Count; i++) {
                if (i != e.WinNames.Count - 1) {
                    playerWinText.text += e.WinNames[i] + ", ";
                }
                else {
                    playerWinText.text += e.WinNames[i];
                }
              
            }

            playerWinText.text += $" {Localization.Get("GAME_COUNTDOWN_WIN")}";
        }

        
    }
}

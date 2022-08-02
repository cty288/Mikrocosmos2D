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
        private Animator countdownAnimator;
        private TMP_Text finalCountdownText;
        
        private float remainingTime = 0;
        private bool isTieTimer = false;
        private void Awake() {
            animator = GetComponent<Animator>();
            finalCountdownText = transform.Find("CountdownFrame/FinalCountdownTimer").GetComponent<TMP_Text>();
            countdownAnimator = transform.Find("CountdownFrame").GetComponent<Animator>();

            
            this.RegisterEvent<OnClientFinalCountdownTimerStart>(OnFinalCountdownStart)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnClientFinalCountDownTimerEnds>(OnFinalCountdownEnd)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

      
            this.RegisterEvent<OnTieTimerStart>(OnTieTimerStart).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientGameEnd>(OnClientGameEnd).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientGameEnd(OnClientGameEnd obj) {
            countdownAnimator.SetTrigger("End");
         //finalCountdownText.gameObject.SetActive(false);
        }

        private void OnTieTimerStart(OnTieTimerStart e) {
            finalCountdownText.text += $"{Localization.Get("GAME_COUNTDOWN_TIE")}";
            isTieTimer = true;
            this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => isTieTimer = false);
            remainingTime = e.Time;
        }

        private void Update() {
            if (remainingTime > 0) {
                remainingTime -= Time.deltaTime;
                remainingTime = Mathf.Max(0, remainingTime);
                if (!isTieTimer) {
                    finalCountdownText.text = $"{Mathf.CeilToInt(remainingTime)}";
                }
             
            }
        }

        

        private void OnFinalCountdownStart(OnClientFinalCountdownTimerStart e) {
            // finalCountdownText.gameObject.SetActive(true);
            countdownAnimator.SetTrigger("Start");
            animator.SetTrigger("Start");
            remainingTime = e.Time;
        }

        private void OnFinalCountdownEnd(OnClientFinalCountDownTimerEnds e) {
            /*
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
            */
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;

namespace Mikrocosmos {
	public partial class GameStartEndCanvas : AbstractMikroController<Mikrocosmos> {
        private float countdown = -1;
        private bool countdownStarted = false;
        
        private void Awake() {
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnNetworkedMainGamePlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            ImgStartBG.gameObject.SetActive(true);
            this.RegisterEvent<OnClientBeginGameCountdownStart>(OnBeginGameCountdownStart)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }


        private void Update() {
            if (countdownStarted) {
                countdown -= Time.deltaTime;
                TextCountdownTime.text = Mathf.CeilToInt(countdown).ToString();
                if (countdown <= 0) {
                    countdownStarted = false;
                    TextCountdownTime.text = "GO!";
                    ObjCountdown.GetComponent<Animator>().SetTrigger("Stop");
                    this.GetSystem<ITimeSystem>().AddDelayTask(1.5f, () => {
                        ObjCountdown.SetActive(false);
                    });
                }
            }
        }

        private void OnBeginGameCountdownStart(OnClientBeginGameCountdownStart e) {
            ObjCountdown.SetActive(true);
            countdown = e.Time;
            countdownStarted = true;
        }


        private void OnNetworkedMainGamePlayerConnected(OnClientMainGamePlayerConnected e) {
            if (e.playerSpaceship.GetComponent<NetworkIdentity>().hasAuthority) {
                ImgStartBG.GetComponent<Animator>().SetTrigger("Ready");
            }
        }
    }
}
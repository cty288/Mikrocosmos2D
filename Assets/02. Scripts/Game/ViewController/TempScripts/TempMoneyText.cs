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
    public class TempMoneyText : AbstractMikroController<Mikrocosmos> {
        private TMP_Text moneyText;
        private int moneyViewNumber;
        private void Awake() {
            moneyText = this.GetComponent<TMP_Text>();
        }

        private void Start() {
            this.RegisterEvent<OnClientMainGamePlayerConnected>(OnLocalPlayerConnected)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientMoneyChange>(OnClientMoneyChange).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Update() {
            moneyText.text = $"Money: {moneyViewNumber}";
        }

        private void OnClientMoneyChange(OnClientMoneyChange e) {
            DOTween.To(() => moneyViewNumber, x => moneyViewNumber = x,
                e.NewMoney, 1.5f);
        }

        private void OnLocalPlayerConnected(OnClientMainGamePlayerConnected e) {
            moneyViewNumber = e.playerSpaceship.GetComponent<IPlayerTradingSystem>().Money;
            moneyText.text = $"Money: {moneyViewNumber}";
            
        }
    }
}

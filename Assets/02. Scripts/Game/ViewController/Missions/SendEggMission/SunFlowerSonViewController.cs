using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class SunFlowerSonViewController : BasicGoodsViewController {
        private GameObject mapUI;

        protected override void Awake() {
            base.Awake();
            mapUI = transform.Find("MapUI").gameObject;
        }

        public override void OnStartClient() {
            base.OnStartClient();
            this.RegisterEvent<OnClientGoodsTransactionStatusChanged>(OnTransactionStatusChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            mapUI.SetActive(false);
            this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => {
                TurnMapUI(GoodsModel.TransactionFinished);
            });

        }

        private void OnTransactionStatusChanged(OnClientGoodsTransactionStatusChanged e) {
            if (e.Goods == GoodsModel) {
                TurnMapUI(e.IsFinished);
            }
            
        }

        [ClientCallback]
        private void TurnMapUI(bool isOn) {
            mapUI.SetActive(isOn);
        }
    }
}

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
        private Animator mapUIAnimator;

        protected override void Awake() {
            base.Awake();
            mapUI = transform.Find("MapUI").gameObject;
            mapUIAnimator = mapUI.GetComponent<Animator>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnMissionStop>(OnMissionStop).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnMissionStop(OnMissionStop e) {
            if (e.MissionName == "SendEggMission") {
                if (GoodsModel.TransactionFinished) {
                    RpcStopMapUI();
                }
            }
        }

        

        public override void OnStartClient() {
            base.OnStartClient();
            this.RegisterEvent<OnClientGoodsTransactionStatusChanged>(OnTransactionStatusChanged).UnRegisterWhenGameObjectDestroyed(gameObject, true);
            mapUI.SetActive(false);
            this.GetSystem<ITimeSystem>().AddDelayTask(1f, () => {
                TurnMapUI(GoodsModel.TransactionFinished);
            });

        }

        protected override string GetHintAssetName() {
            return "";
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
        
        [ClientRpc]
        private void RpcStopMapUI() {
           mapUIAnimator.SetTrigger("Stop");
        }

    }
}

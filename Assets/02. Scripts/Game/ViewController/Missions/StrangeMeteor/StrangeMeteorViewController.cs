using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class StrangeMeteorViewController : BasicEntityViewController {
        private StrangeMeteorModel model;
        private StrangeMeteorTrigger trigger;
        private SpriteRenderer rangeRenderer;
        private SpriteRenderer mapUI;
        private Animator rangeRingAnimator;


        [SerializeField] private Image team1Ring;
        [SerializeField] private List<int> teamPlayersInRange = new List<int>() {0, 0};

        [SerializeField] private Color[] teamColors;

        private BindableProperty<bool> belongToTeam1 = new BindableProperty<bool>(false);
        private bool belongToTeam1Set = false;

        private float actualFill = 0.5f;
        protected override void Awake() {
            base.Awake();
            model = GetComponent<StrangeMeteorModel>();
            trigger = GetComponentInChildren<StrangeMeteorTrigger>();
            rangeRenderer = trigger.GetComponent<SpriteRenderer>();
            belongToTeam1.RegisterOnValueChaned(OnBelongToTeam1Changed).UnRegisterWhenGameObjectDestroyed(gameObject);
            mapUI = transform.Find("MapUI").GetComponent<SpriteRenderer>();
            rangeRingAnimator = transform.Find("StrangeMeteorRange/Canvas").GetComponent<Animator>();
        }

        private void OnBelongToTeam1Changed(bool arg1, bool belong) {
            Color targetColor = Color.white;
            if (actualFill != 0.5f) {
                 targetColor = belong ? teamColors[0] : teamColors[1];
            }
          
            rangeRenderer.DOColor(targetColor, 1f);
            mapUI.DOColor(targetColor, 1f);

        }

        public override void OnStartServer() {
            base.OnStartServer();
            trigger.OnPlayerEnterTrigger += OnPlayerEnterTrigger;
            trigger.OnPlayerExitTrigger += OnPlayerExitTrigger;
        }

        public override void OnStopServer() {
            base.OnStopServer();
            trigger.OnPlayerEnterTrigger -= OnPlayerEnterTrigger;
            trigger.OnPlayerExitTrigger -= OnPlayerExitTrigger;
        }

        private void OnPlayerExitTrigger(PlayerSpaceship obj) {
            teamPlayersInRange[obj.ThisSpaceshipTeam - 1]--;
            CalculatePlayerDifferenceAndNotifyClient();
        }

        private void OnPlayerEnterTrigger(PlayerSpaceship obj) {
            teamPlayersInRange[obj.ThisSpaceshipTeam - 1]++;
            CalculatePlayerDifferenceAndNotifyClient();
        }
        //64   //5.625 -> 1  //0.015625
        [ServerCallback]
        private void CalculatePlayerDifferenceAndNotifyClient() {

             model.Team1MinusTeam2PlayerDifference = teamPlayersInRange[0] - teamPlayersInRange[1];
            RpcUpdateProgress(model.Team1MinusTeam2PlayerDifference , model.Team1Progress);
        }


        protected override void Update() {
            base.Update();
            if (model.Team1MinusTeam2PlayerDifference != 0) {
                actualFill += (model.Team1MinusTeam2PlayerDifference * model.PerPlayerProgressPerSecond) *
                              Time.deltaTime;

                actualFill = Mathf.Clamp(actualFill, 0f, 1f);

                team1Ring.fillAmount = Mathf.FloorToInt(actualFill / 0.015625f) * 0.015625f;


                //team1Ring.fillAmount = actualFill / 0;

                belongToTeam1.Value = team1Ring.fillAmount > 0.5f;
                if (!belongToTeam1Set) {
                    belongToTeam1Set = true;
                    OnBelongToTeam1Changed(false, belongToTeam1.Value);
                }

                rangeRingAnimator.SetBool("Changing", true);
            }
            else {
                rangeRingAnimator.SetBool("Changing", false);
            }
        }

        [ClientRpc]
        private void RpcUpdateProgress(int team1MinusTeam2, float fixedProgress) {
            if (isClientOnly) {
                model.Team1MinusTeam2PlayerDifference = team1MinusTeam2;
                model.Team1Progress = fixedProgress;
            }
        }
    }
}

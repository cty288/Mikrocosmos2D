using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class MissionWinnerInfoUIViewController : AbstractMikroController<Mikrocosmos> {
        private Animator animator;
        private TMP_Text missionNameText;
        private TMP_Text winnerNameText;
        private TMP_Text rewardsText;
        private Transform difficultyLayout;
        private void Awake() {
            animator = GetComponentInChildren<Animator>();
            Transform panel = transform.Find("Panel");

            missionNameText = panel.Find("TopMask/Parent/TitleBG/TitleText").GetComponent<TMP_Text>();
            winnerNameText = panel.Find("PanelMask/BG/MaskParent/WinnerNamesText").GetComponent<TMP_Text>();
            rewardsText = panel.Find("PanelMask/BG/MaskParent/RewardsText").GetComponent<TMP_Text>();
            difficultyLayout = panel.Find("TopMask/Parent/DifficultyLayout");
        }

        private void Start() {
            this.RegisterEvent<OnClientRewardsGeneratedForMission>(OnRewardsGenerated)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnRewardsGenerated(OnClientRewardsGeneratedForMission e) {
            missionNameText.text = e.MissionNameLocalized + Localization.Get("GAME_MISSION_WINNER");
            rewardsText.text = "";
            for (int i = 0; i < e.RewardNames.Count; i++) {
                rewardsText.text += $"-{e.RewardNames[i]}";
                if (i != e.RewardNames.Count - 1) {
                    rewardsText.text += "\n";
                }
            }

            winnerNameText.text = "";
            for (int i = 0; i < e.WinnerNames.Count; i++)
            {
                winnerNameText.text += $"{e.WinnerNames[i]}";
                if (i != e.WinnerNames.Count - 1) {
                    winnerNameText.text += Localization.Get("COMMA");
                }
            }

            
            for (int i = 0; i < difficultyLayout.childCount; i++) {
                if (i < e.DifficultyLevel) {
                    difficultyLayout.GetChild(i).gameObject.SetActive(true);
                }
                else {
                    difficultyLayout.GetChild(i).gameObject.SetActive(false);
                }
            }

            animator.SetTrigger("Start");
            this.GetSystem<ITimeSystem>().AddDelayTask(7f, () => { animator.SetTrigger("End"); });
        }
    }
}

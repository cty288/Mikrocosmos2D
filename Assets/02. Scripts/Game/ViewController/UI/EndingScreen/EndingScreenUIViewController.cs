using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class EndingScreenUIViewController : AbstractMikroController<Mikrocosmos> {
        private Animator animator;

        [SerializeField] private List<GameObject> particleEffects;
        [SerializeField] private List<GameObject> teamWinTexts;

        [SerializeField]
        private List<Image> affinityBars;
        [SerializeField]
        private List<GameObject> affinityTexts;
        [SerializeField]
        private List<ScoreElement> scoreElements;

        [SerializeField] private List<CategoryWinnerElement> categoryWinElements;

        private GameEndInfo gameEndInfo;

        private GameObject smallAnimalsObject;
        private GameObject statisticsPanel;
        private Button exitGameButton;
        private void Awake() {
            animator = GetComponent<Animator>();
            this.RegisterEvent<OnClientGameEnd>(OnClientGameEnd).UnRegisterWhenGameObjectDestroyed(gameObject);
            statisticsPanel = transform.Find("EndingScreen/Statistics").gameObject;
            smallAnimalsObject = transform.Find("EndingScreen/BG2").gameObject;
            exitGameButton = transform.Find("EndingScreen/ExitButton").GetComponent<Button>();
            exitGameButton.onClick.AddListener(OnExitGame);
        }

        private void OnExitGame() {
            if (NetworkClient.active) {
                this.GetSystem<IRoomMatchSystem>().CmdQuitRoom(NetworkClient.localPlayer);
            }

            if (NetworkServer.active) {
                NetworkRoomManager.singleton.StopHost();
                NetworkServer.DisconnectAll();
                NetworkServer.Shutdown();
            }

            SceneManager.LoadScene("Menu");
        }

        private void OnClientGameEnd(OnClientGameEnd e) {
            animator.SetTrigger("Start");
            SetUpAllInfo(e.GameEndInfo);
            NetworkManager.singleton.offlineScene = "";

        }


        private void Update() {
            if (gameEndInfo != null && affinityBars[0].fillAmount > 0) {
                
                float team1Affinity = ((float) Math.Round(affinityBars[0].fillAmount * 100, 1));
                float team2Affinity = ((float)Math.Round(affinityBars[1].fillAmount * 100, 1));
                if (gameEndInfo.Team1Affinity > 0.2f)
                {
                    affinityTexts[0].SetActive(true);
                    affinityTexts[0].GetComponent<TMP_Text>().text =
                        team1Affinity.ToString() + "%";
                }

                if ( (1-gameEndInfo.Team1Affinity) > 0.2f)
                {
                    affinityTexts[1].SetActive(true);
                    affinityTexts[1].GetComponent<TMP_Text>().text =
                        team2Affinity.ToString()+"%";
                }
            }
           
        }

        private void SetUpAllInfo(GameEndInfo info) {
            //category winner
            for (int i = 0; i < info.CategoryWinners.Count; i++) {
                CategoryWinner winnerInfo = info.CategoryWinners[i];
                categoryWinElements[i].SetInfo(winnerInfo.PlayerInfo.Name,
                    GetCategoryDescriptionLocalized(winnerInfo.CategoryWinningType));
            }
            
            //affinity text and winning text
            int team1Affinity = Mathf.Clamp(Mathf.RoundToInt(info.Team1Affinity * 100), 0, 100);
            int team2Affinity = 100 - team1Affinity;
            affinityTexts[0].GetComponent<TMP_Text>().text = team1Affinity.ToString();
            affinityTexts[1].GetComponent<TMP_Text>().text = team2Affinity.ToString();
            
            
            //score elements
            for (int i = 0; i < info.PlayerWinInfos.Count; i++) {
                PlayerWinInfo winInfo = info.PlayerWinInfos[i];
                scoreElements[i].SetInfo(winInfo.PlayerInfo.Name, winInfo.Score);
                scoreElements[i].gameObject.SetActive(true);
            }

            for (int i = info.PlayerWinInfos.Count; i < scoreElements.Count; i++) {
                if (i <= 2) {
                    scoreElements[i].SetInfo("",-1);
                    scoreElements[i].gameObject.SetActive(true);
                }
                else {
                    scoreElements[i].gameObject.SetActive(false);
                }

            }

            this.gameEndInfo = info;

            /*
            if(info.WinTeam == 1){ 
                teamWinTexts[0].SetActive(true);
                teamWinTexts[1].SetActive(false);
            }else {
                teamWinTexts[0].SetActive(false);
                teamWinTexts[1].SetActive(true);
            }*/
        }

        public void OnStartAffinityBars() {
            float team1Affinity = gameEndInfo.Team1Affinity;
            float team2Affinity = 1 - team1Affinity;

            affinityBars[0].DOFillAmount(Mathf.Min(team1Affinity, 0.25f), 1f);
            affinityBars[1].DOFillAmount(Mathf.Min(0.25f, team2Affinity), 1f).SetEase(Ease.OutCubic).OnComplete(() => {
                
                this.GetSystem<ITimeSystem>().AddDelayTask(0.3f, () => {
                    affinityBars[0].DOFillAmount(team1Affinity, 0.3f);
                    affinityBars[1].DOFillAmount(team2Affinity, 0.3f).OnComplete(() => {
                        transform.Find("EndingScreen").DOShakePosition(0.4f, 100f, 30);
                        if (gameEndInfo.WinTeam == 1)
                        {
                            teamWinTexts[0].SetActive(true);
                            teamWinTexts[1].SetActive(false);
                        }
                        else
                        {
                            teamWinTexts[0].SetActive(false);
                            teamWinTexts[1].SetActive(true);
                        }
                        /*
                        if (team1Affinity >= 0.2f)
                        {
                            affinityTexts[0].SetActive(true);
                        }

                        if (team2Affinity >= 0.2f)
                        {
                            affinityTexts[1].SetActive(true);
                        }*/

                        foreach (GameObject effect in particleEffects)
                        {
                            effect.SetActive(true);
                        }
                        statisticsPanel.SetActive(true);
                        smallAnimalsObject.SetActive(true);
                    });
                });
               
            });
        }

        private string GetCategoryDescriptionLocalized(CategoryWinningType winningType) {
            switch (winningType) {
                case CategoryWinningType.DieLeast:
                    return Localization.Get("GAME_END_LEAST_DIE");
                case CategoryWinningType.EarnMostMoney:
                    return Localization.Get("GAME_END_EARN_MOST");
                case CategoryWinningType.MostEffectiveKills:
                    return Localization.Get("GAME_END_KILL_MOST");
                case CategoryWinningType.MostTrade:
                    return Localization.Get("GAME_END_TRADE_MOST");
            }

            return "";
        }
    }
}

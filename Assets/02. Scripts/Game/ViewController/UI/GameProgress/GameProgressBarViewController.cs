using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Mikrocosmos
{
    public class GameProgressBarViewController : AbstractMikroController<Mikrocosmos> {
        private GameProgressElement ongoingElement = null;
        private HorizontalLayoutGroup layoutGroup;
        private ContentSizeFitter contentSizeFitter;

        [FormerlySerializedAs("gameProgressElement")] [SerializeField] private GameObject gameProgressElementPrefab = null;
        [SerializeField] private GameObject gameMissionElementPrefab = null;

        private List<float> gameMissionCutoffPoint = new List<float>();
        
        private List<GameProgressElement> gameProgressProgressBars = new List<GameProgressElement>();
        private List<MissionProgressElement> gameMissionElements = new List<MissionProgressElement>();

        

        private void Awake() {
            this.RegisterEvent<OnClientMissionTimeCutoffGenerated>(OnClientMissionTimeCutoffGenerated)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientBeginGameCountdownStart>(OnClientBeginGameCountdownStart)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnStandardGameProgressChanged>(OnStandardGameProgressChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnProgressMissionFinished>(OnProgressMissionFinished)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            layoutGroup = GetComponentInChildren<HorizontalLayoutGroup>(true);
            contentSizeFitter = layoutGroup.GetComponent<ContentSizeFitter>();

        }
        
        private void OnProgressMissionFinished(OnProgressMissionFinished e) {
            if (e.MissionIndex >= gameMissionElements.Count) {
                return;
            }
            MissionProgressElement missionElement = gameMissionElements[e.MissionIndex];
            missionElement.OnMissionProgressStop(e.WinTeam);
            missionElement.GetComponent<Animator>().SetTrigger("Stop");
        }

        private void OnStandardGameProgressChanged(OnStandardGameProgressChanged e) {
            if (!e.Info.IsReachMissionPoint && !e.Info.IsReachGameEndPoint) {
                GameProgressElement progressBar = gameProgressProgressBars[e.Info.NextMissionIndex];
                progressBar.UpdateProgress(e.Progress, e.Info.Affinity);
                return;
            }else {
                //stop ongoing bar first
                GameProgressElement ongoingElement = gameProgressProgressBars[e.Info.NextMissionIndex];
                ongoingElement.GetComponent<Animator>().SetTrigger("End");
                ongoingElement.UpdateProgress(e.Progress, e.Info.Affinity);
                if (!e.Info.IsReachGameEndPoint && e.Info.NextMissionIndex < gameMissionElements.Count) { //start next mission
                    MissionProgressElement missionElement = gameMissionElements[e.Info.NextMissionIndex];
                    missionElement.GetComponent<Animator>().SetTrigger("Start");
                }
            }
        }

        private void OnClientBeginGameCountdownStart(OnClientBeginGameCountdownStart e) {
            StartCoroutine(StartFirstProgressBar(e.Time));
        }

        private IEnumerator StartFirstProgressBar(float time) {
            yield return new WaitForSeconds(time);
            gameProgressProgressBars[0].GetComponent<Animator>().SetTrigger("Start");
        }

        private void OnClientMissionTimeCutoffGenerated(OnClientMissionTimeCutoffGenerated e) {
            gameMissionCutoffPoint = e.cutoffs;
            StartCoroutine(SetupMissionCutoffs(gameMissionCutoffPoint));
            
        }

        private IEnumerator SetupMissionCutoffs(List<float> cutoffs) {
            for (int i = 0; i < cutoffs.Count; i++) {
                float progressStartAt = i == 0 ? 0 : cutoffs[i - 1];
                float progressEndAt = cutoffs[i];
                var progressBarElement = Instantiate(gameProgressElementPrefab, layoutGroup.transform).GetComponent<GameProgressElement>();
                progressBarElement.transform.SetAsLastSibling();
                gameProgressProgressBars.Add(progressBarElement);
                progressBarElement.SetUpInfo(progressStartAt, progressEndAt);
                yield return null;
                CorrectLayoutSize();

                var missionElement = Instantiate(gameMissionElementPrefab, layoutGroup.transform).GetComponent<MissionProgressElement>();
                missionElement.transform.SetAsLastSibling();
                gameMissionElements.Add(missionElement);
                yield return null;
                CorrectLayoutSize();
            }
          

            //spawn an additional progress bar for the last mission
            var progressBarElementLast = Instantiate(gameProgressElementPrefab, layoutGroup.transform).GetComponent<GameProgressElement>();
            progressBarElementLast.transform.SetAsLastSibling();
            gameProgressProgressBars.Add(progressBarElementLast);
            progressBarElementLast.SetUpInfo(cutoffs[^1], 1);
            yield return null;
            CorrectLayoutSize();
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }


        private void CorrectLayoutSize() {
            if (contentSizeFitter.horizontalFit == ContentSizeFitter.FitMode.Unconstrained) {
                return;
            }
            //get left of the layout group
            var rect = layoutGroup.GetComponent<RectTransform>();
            var left = rect.offsetMin.x;
            if (left <= 0) {
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                //set left and right to 0
                rect.SetLeft(0);
                rect.SetRight(0);
            }

        }
    }
    
}

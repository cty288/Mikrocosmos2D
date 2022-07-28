using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class GameProgressBarViewController : AbstractMikroController<Mikrocosmos> {
        private GameProgressElement ongoingElement = null;
        private HorizontalLayoutGroup layoutGroup;
        [SerializeField] private Color[] progressBarColors = new Color[3];
        [SerializeField] private GameObject gameProgressElement = null;

        private bool canFadeOut = false;
        private bool faded = false;
        private float fadeTimer = 8f;
        private bool fullMapOpening = false;
        private void Awake() {
            this.RegisterEvent<OnClientNextCountdown>(OnNewCountdownGenerated)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            layoutGroup = GetComponentInChildren<HorizontalLayoutGroup>(true);
            this.RegisterEvent<OnFullMapCanvasOpen>(OnFullMapCanvasOpen).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnFullMapCanvasClose>(OnFullMapCanvasClosed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnFullMapCanvasClosed(OnFullMapCanvasClose obj) {
            fullMapOpening = false;
        }

        private void OnFullMapCanvasOpen(OnFullMapCanvasOpen e) {
            FadeIn();
            fullMapOpening = true;
            
        }

        private void Update() {
            if (canFadeOut && !fullMapOpening) {
                fadeTimer -= Time.deltaTime;
                if (fadeTimer <= 0 && !faded) {
                    faded = true;
                    fadeTimer = 8f;
                    FadeOut();
                }
            }
        }

        private void OnNewCountdownGenerated(OnClientNextCountdown e) {
            canFadeOut = false;
            if (ongoingElement) {
                Color color = e.ShowAffinityForLastTime
                    ? (e.Team1Affinity >= 0.5f ? progressBarColors[1] : progressBarColors[2])
                    : progressBarColors[0];

                ongoingElement.EndProgress(color, () => {
                    ongoingElement = Instantiate(gameProgressElement, layoutGroup.transform).GetComponent<GameProgressElement>();
                    ongoingElement.transform.SetAsLastSibling();
                    ongoingElement.StartProgress(e.remainingTime);
                    StartCoroutine(CheckNeedExpandPanel());
                });
            }
            else {
                ongoingElement = Instantiate(gameProgressElement, layoutGroup.transform).GetComponent<GameProgressElement>();
                ongoingElement.transform.SetAsLastSibling();
                ongoingElement.StartProgress(e.remainingTime);
                StartCoroutine(CheckNeedExpandPanel());
            }

            if (e.ShowAffinityForLastTime) {
                StartCoroutine(ChangeCanFadeOut(10f, true));
            }
           
            StartCoroutine(ChangeCanFadeOut(Mathf.Max(e.remainingTime - 20, 0), false));
        }

        private IEnumerator CheckNeedExpandPanel() {
            yield return null;
            RectTransform ongoingTransform = ongoingElement.GetComponent<RectTransform>();
            if (ongoingTransform.anchoredPosition.x + ongoingTransform.sizeDelta.x/2  >= 1070f) {
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childControlWidth = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            }
        }

        private IEnumerator ChangeCanFadeOut(float time, bool canFade) {
            yield return new WaitForSeconds(time);
            canFadeOut = canFade;
            if (!canFadeOut) {
                FadeIn();
            }
        }

        private void FadeIn() {
            faded = false;
            fadeTimer = 8f;
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images) {
                image.DOFade(1, 0.7f);
            }
        }

        private void FadeOut() {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                image.DOFade(0, 0.7f);
            }
        }
    }
    
}

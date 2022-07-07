using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class BuffElementViewController : AbstractMikroController<Mikrocosmos> {
        protected TMP_Text titleText;
        protected TMP_Text descriptionText;
        protected BuffIconViewController iconViewController;
        public BuffInfoPanelViewController BuffInfoPanelViewController;
        protected Animator animator;
        protected string buffName;

        protected static readonly int OnCloseBigMap = Animator.StringToHash("OnCloseBigMap");
        protected static readonly int OnOpenBigMap = Animator.StringToHash("OnOpenBigMap");

        private void Awake() {
            titleText = transform.Find("TitleText").GetComponent<TMP_Text>();
            descriptionText = transform.Find("DescriptionText").GetComponent<TMP_Text>();
            animator = GetComponent<Animator>();
            this.RegisterEvent<OnFullMapCanvasOpen>(OnFullMapCanvasOpen).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnFullMapCanvasClose>(OnFullMapCanvasClose).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnFullMapCanvasClose(OnFullMapCanvasClose e) {
            animator.SetTrigger(OnCloseBigMap);
            animator.ResetTrigger(OnOpenBigMap);
        }

        private void OnFullMapCanvasOpen(OnFullMapCanvasOpen e) {
            animator.SetTrigger(OnOpenBigMap);
            animator.ResetTrigger(OnCloseBigMap);
        }

        public virtual void SetBuffInfo(BuffInfo info) {
            
            if (!iconViewController) {
                iconViewController = GetComponentInChildren<BuffIconViewController>();
            }

            
            titleText.text = info.LocalizedName;
            descriptionText.text = info.LocalizedDescription;
            iconViewController.SetIconInfo(info);
            buffName = info.Name;
        }

    }
}

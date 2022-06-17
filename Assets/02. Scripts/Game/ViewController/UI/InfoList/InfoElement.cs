using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class InfoElement : MonoBehaviour {
        private InfoElementText titleWithDescription;
        private InfoElementText onlyTitle;

        private float tentativeMaxTime = -1;

        private float MaxTime = -1;
        private float timer;

        private Image slider;
        private Animator animator;

        private ClientInfoMessage info;
        private void Awake() {
            titleWithDescription = transform.Find("InfoContainer/TitleWithDescription").GetComponent<InfoElementText>();
            onlyTitle = transform.Find("InfoContainer/OnlyTitle").GetComponent<InfoElementText>();
            slider = transform.Find("InfoContainer/SliderBG/Slider").GetComponent<Image>();
            animator = GetComponent<Animator>();
        }


        private void Update() {
            if (MaxTime < 0) {
                slider.fillAmount = 1;
            }
            else {
                timer -= Time.deltaTime;
                timer = Mathf.Clamp(timer, 0, MaxTime);
                slider.fillAmount = timer / MaxTime;
            }
        }

        public void SetInfo(ClientInfoMessage info, bool isUpdate) {
            this.info = info;
            if (isUpdate) {
                animator.SetTrigger("Update");
                tentativeMaxTime = info.RemainingTime - 1;
            }
            else {
                tentativeMaxTime = info.RemainingTime;
                AnimationSetInfo();
            }
        }

        public void StopInfo() {
            animator.SetTrigger("Stop");
        }

        public void DestroySelf() {
            Destroy(gameObject);
        }
        public void AnimationSetInfo() {
            if (String.IsNullOrEmpty(info.Description)) {
                //no description
                titleWithDescription.gameObject.SetActive(false);
                onlyTitle.gameObject.SetActive(true);
                onlyTitle.SetInfo(info.Title, "");
            }
            else {
                titleWithDescription.gameObject.SetActive(true);
                onlyTitle.gameObject.SetActive(false);
                titleWithDescription.SetInfo(info.Title, info.Description);
            }

            MaxTime = tentativeMaxTime;
            timer = MaxTime;
        }
    }
}

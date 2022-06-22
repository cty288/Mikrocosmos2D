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


        private bool autoDestroy = false;

        private bool showRemainingTime = false;


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
                if (showRemainingTime) {
                    slider.fillAmount = timer / MaxTime;
                }

                
                if (autoDestroy && timer <= 0) {
                    autoDestroy = false;
                    this.GetComponentInParent<GameInfoPanel>().InfoElementSelfDestroy(info.Name);
                }
            }
        }

        public void SetInfo(ClientInfoMessage info, bool isUpdate) {
            this.info = info;
            this.autoDestroy = info.AutoDestroyWhenTimeUp;
            this.showRemainingTime = info.ShowRemainingTime;
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

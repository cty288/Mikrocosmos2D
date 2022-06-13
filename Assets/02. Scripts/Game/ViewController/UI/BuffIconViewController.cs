using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class BuffIconViewController : MonoBehaviour {
        private Image buffIconProgress;
        private float maxTime = -1;
        
        private float progressTimer = -1;
        private float targetProgressTimer = -1;
        
        private void Awake() {
            buffIconProgress = transform.Find("BuffIconProgress").GetComponent<Image>();
        }

        private void Update() {
            if (maxTime > 0) {
                targetProgressTimer -= Time.deltaTime;
                if (targetProgressTimer < 0) {
                    targetProgressTimer = 0;
                }

                progressTimer = Mathf.Lerp(progressTimer, targetProgressTimer, Time.deltaTime * 4);
                buffIconProgress.fillAmount = progressTimer / maxTime;
            }
        }

        public void SetIconInfo(BuffInfo info) {
            if(info.TimeBuffMaxTime==0){  // no time buff
                buffIconProgress.fillAmount = 1;
            }else {
                maxTime = info.TimeBuffMaxTime;
                targetProgressTimer = info.TimeBuffTime;
                if (progressTimer < 0) {
                    progressTimer = targetProgressTimer;
                }
            }
        }
    }
}
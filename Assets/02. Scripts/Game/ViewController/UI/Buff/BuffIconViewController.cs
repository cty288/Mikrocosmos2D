using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class BuffIconViewController : AbstractMikroController<Mikrocosmos> {
        protected Image buffIconProgress;
        private float maxTime = -1;
        
        private float progressTimer = -1;
        private float targetProgressTimer = -1;
        
        private void Awake() {
            buffIconProgress = transform.Find("BuffIconProgress").GetComponent<Image>();
        }

        protected virtual void Update() {
            if (maxTime > 0) {
                targetProgressTimer -= Time.deltaTime;
                if (targetProgressTimer < 0) {
                    targetProgressTimer = 0;
                }

                progressTimer = Mathf.Lerp(progressTimer, targetProgressTimer, Time.deltaTime * 4);
                buffIconProgress.fillAmount = progressTimer / maxTime;
            }
        }

        public virtual void SetIconInfo(BuffInfo info) {
            if(info.TimeBuffInfo.TimeBuffMaxTime==0){  // no time buff
                buffIconProgress.fillAmount = 1;
            }else {
                maxTime = info.TimeBuffInfo. TimeBuffMaxTime;
                targetProgressTimer = info.TimeBuffInfo.TimeBuffTime;
                if (progressTimer < 0) {
                    progressTimer = targetProgressTimer;
                }
            }
        }
    }
}

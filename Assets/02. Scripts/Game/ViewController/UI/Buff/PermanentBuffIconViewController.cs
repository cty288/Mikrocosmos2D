using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public class PermanentBuffIconViewController : BuffIconViewController {
        private bool initialized = false;
        public BindableProperty<int> IconLevel = new BindableProperty<int>(0);
        public Action OnFillImageFinished;
        protected override void Update() {
          
        }

        public override void SetIconInfo(BuffInfo info) {
            if (!initialized) {
                initialized = true;
                IconLevel.Value = info.PermanentRawMaterialBuffInfo.CurrentLevel;
                this.GetSystem<IAudioSystem>().PlaySound("BuffUpgrade", SoundType.Sound2D);
                if (info.PermanentRawMaterialBuffInfo.CurrentLevel != info.PermanentRawMaterialBuffInfo.MaxLevel) {
                    buffIconProgress.fillAmount = info.PermanentRawMaterialBuffInfo.CurrentProgressInLevel / (float)info.PermanentRawMaterialBuffInfo.MaxProgressForCurrentLevel;
                }
                else {
                    buffIconProgress.fillAmount = 1;
                }
            }
            else {
                buffIconProgress.DOKill();
                int levelChangeNumber = info.PermanentRawMaterialBuffInfo.CurrentLevel - IconLevel.Value;
                if (levelChangeNumber > 0) {
                    buffIconProgress.DOFillAmount(1, 0.2f).OnComplete((() => {
                        buffIconProgress.fillAmount = 0;
                        IconLevel.Value++;
                        levelChangeNumber--;
                        this.GetSystem<IAudioSystem>().PlaySound("BuffUpgrade", SoundType.Sound2D);
                        
                        if (levelChangeNumber > 0) {
                            buffIconProgress.DOFillAmount(1, 0.2f).SetLoops(levelChangeNumber, LoopType.Restart).OnStepComplete((
                                () => {
                                    this.GetSystem<IAudioSystem>().PlaySound("BuffUpgrade", SoundType.Sound2D);
                                    IconLevel.Value++;
                                })).OnComplete(() => {
                                buffIconProgress.fillAmount = 0;
                                FillRemainingProgress(info.PermanentRawMaterialBuffInfo.CurrentProgressInLevel, info.PermanentRawMaterialBuffInfo.MaxProgressForCurrentLevel,
                                    info.PermanentRawMaterialBuffInfo.CurrentLevel == info.PermanentRawMaterialBuffInfo.MaxLevel);
                                });
                        }
                        else {
                            FillRemainingProgress(info.PermanentRawMaterialBuffInfo.CurrentProgressInLevel, info.PermanentRawMaterialBuffInfo.MaxProgressForCurrentLevel,
                                info.PermanentRawMaterialBuffInfo.CurrentLevel == info.PermanentRawMaterialBuffInfo.MaxLevel);
                        }
                    }));
                    
                }else if (levelChangeNumber < 0) {
                    int levelDownNumber = Mathf.Abs(levelChangeNumber);
                    
                    buffIconProgress.DOFillAmount(0, 0.2f).OnComplete((() => {
                        buffIconProgress.fillAmount = 1;
                        IconLevel.Value--;
                        levelDownNumber--;
                        
                        
                        if (levelDownNumber > 0)
                        {
                            buffIconProgress.DOFillAmount(0, 0.2f).SetLoops(levelDownNumber, LoopType.Restart).OnStepComplete((
                                () => {
                                    IconLevel.Value--;
                                })).OnComplete(() => {
                                buffIconProgress.fillAmount = 1;
                                FillRemainingProgress(info.PermanentRawMaterialBuffInfo.CurrentProgressInLevel, info.PermanentRawMaterialBuffInfo.MaxProgressForCurrentLevel,
                                    info.PermanentRawMaterialBuffInfo.CurrentLevel == info.PermanentRawMaterialBuffInfo.MaxLevel);
                            });
                        }
                        else
                        {
                            FillRemainingProgress(info.PermanentRawMaterialBuffInfo.CurrentProgressInLevel, info.PermanentRawMaterialBuffInfo.MaxProgressForCurrentLevel,
                                info.PermanentRawMaterialBuffInfo.CurrentLevel == info.PermanentRawMaterialBuffInfo.MaxLevel);
                        }
                    }));
                }
                else {
                    FillRemainingProgress(info.PermanentRawMaterialBuffInfo.CurrentProgressInLevel, info.PermanentRawMaterialBuffInfo.MaxProgressForCurrentLevel,
                        info.PermanentRawMaterialBuffInfo.CurrentLevel == info.PermanentRawMaterialBuffInfo.MaxLevel);
                }

             

            }
        }


        private void FillRemainingProgress(int currentProgress, int maxProgress, bool isMaxLevel) {
            if (isMaxLevel) {
                buffIconProgress.DOFillAmount(
                    1,
                    0.3f).OnComplete(() => {
                        OnFillImageFinished?.Invoke();
                });
            }
            else {
                buffIconProgress.DOFillAmount(
                    currentProgress /
                    (float)maxProgress,
                    0.3f).OnComplete(() => {
                    OnFillImageFinished?.Invoke();
                    });
            }
           
        }
    }
}

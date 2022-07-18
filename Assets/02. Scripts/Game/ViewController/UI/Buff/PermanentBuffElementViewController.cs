using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Polyglot;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class PermanentBuffElementViewController : BuffElementViewController {
        private bool initialized = false;
        private string localizedName;
        private string localizedDescription;
        private int previousLevel;
       
        public override void SetBuffInfo(BuffInfo info, GameObject buffIconCreated) {
            buffName = info.Name;
            if (!iconViewController && buffIconCreated)
            {
                iconViewController = buffIconCreated.AddComponent<PermanentBuffIconViewController>();
                PermanentBuffIconViewController icon = (PermanentBuffIconViewController)iconViewController;
                icon.IconLevel.RegisterOnValueChaned(OnIconLevelNumberChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
                icon.OnFillImageFinished += OnFillImageFinished;
            }

            if (this && animator)
            {
                animator.SetTrigger("OnInfoUpdate");
            }
          
            float waitTime = 0f;
           
            if (titleText.color.a < 0.5f) {
                waitTime = 0.6f;
            }
            
          
            
            this.GetSystem<ITimeSystem>().AddDelayTask(waitTime, () => {

               

                localizedName = info.LocalizedName;
                if (!initialized) {
                 
                    previousLevel = info.PermanentRawMaterialBuffInfo.CurrentLevel;
                    titleText.text = localizedName + Localization.GetFormat("GAME_PERM_BUFF_LEVEL",
                        info.PermanentRawMaterialBuffInfo.CurrentLevel);
                    
                    descriptionText.text = info.LocalizedDescription;
                    iconViewController.SetIconInfo(info);
                    initialized = true;
                }
                else {
                    localizedDescription = info.LocalizedDescription;
                    iconViewController.SetIconInfo(info);

                    if (previousLevel != info.PermanentRawMaterialBuffInfo.CurrentLevel) {
                        //hide text for a while
                        previousLevel = info.PermanentRawMaterialBuffInfo.CurrentLevel;
                        if (this && animator) {
                            animator.SetTrigger("OnBuffLevelUp");
                        }
                        

                        this.GetSystem<ITimeSystem>().AddDelayTask(0.18f, () => {
                            descriptionText.text = localizedDescription;
                        });
                    }
                    else
                    {
                        descriptionText.text = info.LocalizedDescription;
                    }
                }
            });
           
            
           
          

        }

        private void OnDestroy() {
            ((PermanentBuffIconViewController)iconViewController).OnFillImageFinished -= OnFillImageFinished;
        }

        private void OnFillImageFinished() {
            animator.SetTrigger("OnFillBuffFinished");
        }

        private void OnIconLevelNumberChanged(int oldValue, int newValue) {
            if (newValue <= 0) {
                if (BuffInfoPanelViewController) {
                    BuffInfoPanelViewController.RemoveBuffFromPanel(buffName);
                }
                else {
                    Destroy(gameObject);
                }
            }
            else {
                titleText.text = localizedName + Localization.GetFormat("GAME_PERM_BUFF_LEVEL", newValue);
            }
           
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.ActionKit;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class DieBuff :  ITimedBuff
    {
        public DieBuff(float maxDuration, Action onFinished = null) {

            MaxDuration = maxDuration;
            RemainingTime = maxDuration;

            OnTimedActionEnd = CallbackAction.Allocate(() => {
                onFinished?.Invoke();
            });
        }

        public  string Name { get; } = "DieBuff";

        public  string GetLocalizedDescriptionText() {
            return Localization.Get("GAME_BUFF_DIE_DESCRIPTION");
        }

        public  string GetLocalizedName() {
            return Localization.Get("GAME_BUFF_DIE");
        }

        public float MaxDuration { get; protected set; }
        public float RemainingTime { get; set; }
        public MikroAction OnTimedActionEnd { get; set; }
    }

    
    public class InvincibleBuff : ITimedBuff
    {
        public InvincibleBuff(float maxDuration,
            Action onFinished = null) {

            MaxDuration = maxDuration;
            RemainingTime = maxDuration;
            
            OnTimedActionEnd = CallbackAction.Allocate(() => {
                onFinished?.Invoke();
            });

        }


        public  string Name { get; } = "InvincibleBuff";

        public  string GetLocalizedDescriptionText()
        {
            return Localization.Get("GAME_BUFF_INVINCIBLE_DESCRIPTION");
        }

        public  string GetLocalizedName()
        {
            return Localization.Get("GAME_BUFF_INVINCIBLE");
        }

        public float MaxDuration { get; }
        public float RemainingTime { get; set; }
        public MikroAction OnTimedActionEnd { get; set; }
    }
}

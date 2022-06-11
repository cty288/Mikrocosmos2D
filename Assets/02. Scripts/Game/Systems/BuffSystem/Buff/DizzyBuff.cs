using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.ActionKit;
using UnityEngine;

namespace Mikrocosmos
{
    public class DizzyTimedBuff :  TimedBuff
    {
        public DizzyTimedBuff(float maxDuration, float frequency, IBuffSystem owner,
            Action onFinished = null): base(maxDuration, frequency, owner) {
            
            CallbackAction action = CallbackAction.Allocate(() => {
                
            });
            action.OnEndedCallback += onFinished;

            OnAction = action;
        }
    }
    public class InvincibleTimedBuff : TimedBuff
    {
        public InvincibleTimedBuff(float maxDuration, float frequency, IBuffSystem owner,
            Action onFinished = null) : base(maxDuration, frequency, owner)
        {

            CallbackAction action = CallbackAction.Allocate(() => {

            });
            action.OnEndedCallback += onFinished;

            OnAction = action;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.ActionKit;
using UnityEngine;

namespace Mikrocosmos
{
    public class DizzyBuff :  Buff
    {
        public DizzyBuff(float maxDuration, float frequency, IBuffSystem owner,
            Action onFinished = null): base(maxDuration, frequency, owner) {
            
            CallbackAction action = CallbackAction.Allocate(() => {
                
            });
            action.OnEndedCallback += onFinished;

            OnAction = action;
        }
    }
    public class InvincibleBuff : Buff
    {
        public InvincibleBuff(float maxDuration, float frequency, IBuffSystem owner,
            Action onFinished = null) : base(maxDuration, frequency, owner)
        {

            CallbackAction action = CallbackAction.Allocate(() => {

            });
            action.OnEndedCallback += onFinished;

            OnAction = action;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.ActionKit;
using UnityEngine;

namespace Mikrocosmos
{
    public class PoisonBuff : Buff, IRepeatableBuff<PoisonBuff>
    {
        public PoisonBuff(float maxDuration, float frequency, int damagePerFrq, IBuffSystem owner) : base(maxDuration, frequency, owner)
        {
            CallbackAction action = CallbackAction.Allocate(() =>
            {
                if (owner.GetOwnerObject().TryGetComponent<IDamagable>(out IDamagable damagable))
                {
                    damagable.TakeRawDamage(damagePerFrq);
                }
            });

            OnAction = action;
        }

        public void OnBuffRepeated(PoisonBuff addedBuff) {
            RemainingTime += addedBuff.RemainingTime;
            RemainingTime = Mathf.Min(RemainingTime, MaxDuration);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.ActionKit;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class PoisonFrequentBuff : TimedFrequentBuff, IStackableBuff<PoisonFrequentBuff>
    {
        public int DamagePerFrq { get; set; }
        public PoisonFrequentBuff(float maxDuration, float frequency, int damagePerFrq, IBuffSystem owner) : base(maxDuration, frequency,
            CallbackAction.Allocate(() =>
            {
                if (owner.GetOwnerObject().TryGetComponent<IDamagable>(out IDamagable damagable))
                {
                    damagable.TakeRawDamage(damagePerFrq);
                }
            }) ) {
            DamagePerFrq = damagePerFrq;
        }

        public void OnBuffStacked(PoisonFrequentBuff addedFrequentBuff) {
            RemainingTime += addedFrequentBuff.RemainingTime;
            RemainingTime = Mathf.Min(RemainingTime, MaxDuration);
        }

        public override string Name { get; } = "PoisonBuff";

        public override string GetLocalizedDescriptionText() {
            return Localization.GetFormat("GAME_BUFF_POISION_DESCRIPTION", Frequency, DamagePerFrq);
        }

        public override string GetLocalizedName()
        {
            return Localization.Get("GAME_BUFF_POISION");
        }
    }
}

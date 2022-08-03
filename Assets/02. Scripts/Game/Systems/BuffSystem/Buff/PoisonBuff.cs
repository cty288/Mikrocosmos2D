using System.Collections;
using System.Collections.Generic;
using MikroFramework.ActionKit;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class PoisonFrequentBuff : TimedFrequentBuff, IStackableBuff<PoisonFrequentBuff>
    {
        public int DamagePerFrq { get; set; }
        public PoisonFrequentBuff(float maxDuration, float frequency, int damagePerFrq, NetworkIdentity damageDealer, IBuffSystem owner) : base(maxDuration, frequency,
            CallbackAction.Allocate(() =>
            {
                if (owner.GetOwnerObject().TryGetComponent<IDamagable>(out IDamagable damagable))
                {
                    damagable.TakeRawDamage(damagePerFrq, damageDealer);
                }
            }) ) {
            DamagePerFrq = damagePerFrq;
        }

        public void OnBuffStacked(PoisonFrequentBuff addedFrequentBuff) {
            RemainingTime += addedFrequentBuff.RemainingTime;
            RemainingTime = Mathf.Min(RemainingTime, MaxDuration);
        }

        public override string Name { get; } = "PoisonBuff";

        public override string GetLocalizedDescriptionText(Language language) {
            return Localization.GetFormat("GAME_BUFF_POISION_DESCRIPTION", language, Frequency, DamagePerFrq);
        }

        public override string GetLocalizedName(Language language)
        {
            return Localization.Get("GAME_BUFF_POISION", language);
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
    }
}

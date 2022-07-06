using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class AimingSpeedDownDeBuff : UntilBuff {
        public float DecreasePercentage { get; private set; }
        public AimingSpeedDownDeBuff(UntilAction untilAction, float decreasePercentage = 0.5f, int canBeTriggeredTime = 1) : base(canBeTriggeredTime, untilAction) {
            DecreasePercentage = decreasePercentage;
        }

        public override string Name { get; } = "AimingSpeedDownDeBuff";

        public override string GetLocalizedDescriptionText(Language languege) {
            return Localization.Get("GAME_BUFF_AIMING_SPEED_DOWN", languege);
        }

        public override string GetLocalizedName(Language languege) {
            return Localization.Get("GAME_BUFF_AIMING_SPEED_DOWN_DESCRIPTION", languege);
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
    }
}

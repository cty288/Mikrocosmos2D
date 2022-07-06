using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class KrowEyeSpeedDownDeBuff : UntilBuff {
        public float DecreasePercentage { get; private set; }
        public KrowEyeSpeedDownDeBuff(UntilAction untilAction, float decreasePercentage = 0.5f, int canBeTriggeredTime=1 ) : base(canBeTriggeredTime, untilAction) {
            DecreasePercentage = decreasePercentage;
        }

        public override string Name { get; } = "KrowEyeSpeedDownDeBuff";

        public override string GetLocalizedDescriptionText(Language languege)
        {
            return Localization.Get("GAME_BUFF_EYE_SPEED_DOWN", languege);
        }

        public override string GetLocalizedName(Language languege)
        {
            return Localization.Get("GAME_BUFF_EYE_SPEED_DOWN_DESCRIPTION", languege);
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
    }
}

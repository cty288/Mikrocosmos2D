using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class OverweightDeBuff : UntilBuff {
        public OverweightDeBuff(int canBeTriggeredTime, UntilAction untilAction) : base(canBeTriggeredTime, untilAction) {
        }

        public override string Name { get; } = "OverweightDeBuff";

        public override string GetLocalizedDescriptionText() {
            return Localization.Get("GAME_BUFF_OVERWEIGHT_DESCRIPTION");
        }

        public override string GetLocalizedName() {
            return Localization.Get("GAME_BUFF_OVERWEIGHT");
          
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
    }
}

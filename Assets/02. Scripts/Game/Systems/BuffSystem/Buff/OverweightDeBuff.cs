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

        public override string GetLocalizedDescriptionText(Language language) {
            return Localization.Get("GAME_BUFF_OVERWEIGHT_DESCRIPTION", language);
        }

        public override string GetLocalizedName(Language language) {
            return Localization.Get("GAME_BUFF_OVERWEIGHT", language);
          
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
    }
}

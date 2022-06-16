using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class PermanentPowerUpBuff : PermanentRawMaterialBuff
    {
        private float additionalDamageAdditionPercentage;

        public float AdditionalDamageAdditionPercentage => additionalDamageAdditionPercentage;

        public PermanentPowerUpBuff(float additionalDamageAdditionPercentage = 0.1f, int currentLevel = 0, int currentProgressInLevel = 1) : base(currentLevel, currentProgressInLevel) {
            this.additionalDamageAdditionPercentage = additionalDamageAdditionPercentage;
        }

        public override void OnLevelUp(int previousLevel, int currentLevel)
        {

        }

        public override string Name { get; } = "PermanentPowerUpBuff";
        public override string GetLocalizedDescriptionText()
        {
            return Localization.GetFormat("GAME_PERM_BUFF_POWER_UP_DESCRIPTION", ((int)(additionalDamageAdditionPercentage * 100)) * CurrentLevel);
        }

        public override string GetLocalizedName()
        {
            return Localization.Get("GAME_PERM_BUFF_POWER_UP");
        }

        

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();


        public override int MaxLevel { get; set; } = 5;
        public override List<int> ProgressPerLevel { get; set; } = new List<int>() { 1, 2, 2, 3, 4 };
    }
}

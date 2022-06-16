using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class PermanentHealthBuff : PermanentRawMaterialBuff
    {
        private float healthMultiplier;
        private ISpaceshipConfigurationModel spaceshipModel;
        public float HealthMultiplier => healthMultiplier;

        public PermanentHealthBuff(float healthMultiplier, ISpaceshipConfigurationModel spaceshipModel, int currentLevel = 0, int currentProgress = 1) : base()
        {
            this.healthMultiplier = healthMultiplier;
            this.spaceshipModel = spaceshipModel;
        }

        public override void OnLevelUp(int previousLevel, int currentLevel)
        {
            if (spaceshipModel != null) {
                spaceshipModel.AddMaximumHealth((currentLevel - previousLevel) * healthMultiplier);
            }

        }

        public override string Name { get; } = "PermanentHealthBuff";
        public override string GetLocalizedDescriptionText()
        {
            return Localization.GetFormat("GAME_PERM_BUFF_HEALTH_DESCRIPTION", ((int)(healthMultiplier * 100)) * CurrentLevel);
        }

        public override string GetLocalizedName()
        {
            return Localization.Get("GAME_PERM_BUFF_HEALTH");
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
        public override int MaxLevel { get; set; } = 5;
        public override List<int> ProgressPerLevel { get; set; } = new List<int>() { 1, 2, 3, 3, 4 };
    }
}

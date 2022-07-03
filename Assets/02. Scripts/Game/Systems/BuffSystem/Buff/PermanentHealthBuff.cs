using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class PermanentHealthBuff : PermanentRawMaterialBuff
    {
        private float healthMultiplier;
      
        public float HealthMultiplier => healthMultiplier;

        public PermanentHealthBuff(float healthMultiplier,int currentLevel = 0, int currentProgress = 1) : base(currentLevel, currentProgress)
        {
            this.healthMultiplier = healthMultiplier;
        
        }

        public override void OnLevelUp(int previousLevel, int currentLevel)
        {
            if (OwnerIdentity.TryGetComponent<ISpaceshipConfigurationModel>(out ISpaceshipConfigurationModel spaceshipConfigurationModel)){
                spaceshipConfigurationModel.AddMaximumHealth((currentLevel - previousLevel) * healthMultiplier);
            }

        }

        public override string Name { get; } = "PermanentHealthBuff";
        public override string GetLocalizedDescriptionText(Language languege)
        {
            
            return Localization.GetFormat("GAME_PERM_BUFF_HEALTH_DESCRIPTION", languege, ((int)(healthMultiplier * 100)) * CurrentLevel);
        }

        public override string GetLocalizedName(Language languege)
        {
            return Localization.Get("GAME_PERM_BUFF_HEALTH", languege);
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
        public override int MaxLevel { get; set; } = 5;
        public override List<int> ProgressPerLevel { get; set; } = new List<int>() { 1, 2, 3, 4, 4 };
    }
}

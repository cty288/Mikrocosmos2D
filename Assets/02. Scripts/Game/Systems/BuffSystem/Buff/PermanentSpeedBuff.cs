using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class PermanentSpeedBuff : PermanentRawMaterialBuff {

        private float speedMultiplier;
     
        public float SpeedMultiplier => speedMultiplier;

        public PermanentSpeedBuff(float speedMultiplier, int currentLevel=0, int currentProgress=1): base(currentLevel, currentProgress)
        {
            this.speedMultiplier = speedMultiplier;
         
        }

        public override void OnLevelUp(int previousLevel, int currentLevel) {
            if (OwnerIdentity.TryGetComponent<ISpaceshipConfigurationModel>(out ISpaceshipConfigurationModel spaceshipModel)) {
                spaceshipModel.AddSpeedAndAcceleration((currentLevel - previousLevel) * speedMultiplier);
            }
          
        }

        public override string Name { get; } = "PermanentSpeedUpBuff";
        public override string GetLocalizedDescriptionText() {
            return Localization.GetFormat("GAME_PERM_BUFF_SPEED_UP_DESCRIPTION", ((int)(speedMultiplier * 100)) * CurrentLevel);
        }

        public override string GetLocalizedName() {
            return Localization.Get("GAME_PERM_BUFF_SPEED_UP");
        }

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
        public override int MaxLevel { get; set; } = 5;
        public override List<int> ProgressPerLevel { get; set; } = new List<int>() {1, 2, 2, 2, 2};
    }
}

using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class PermanentSpeedBuff : PermanentRawMaterialBuff {

        private float speedMultiplier;
        private ISpaceshipConfigurationModel spaceshipModel;
        public float SpeedMultiplier => speedMultiplier;

        public PermanentSpeedBuff(float speedMultiplier, ISpaceshipConfigurationModel spaceshipModel, int currentLevel=0, int currentProgress=1): base(){
            this.speedMultiplier = speedMultiplier;
            this.spaceshipModel = spaceshipModel;
        }

        public override void OnLevelUp(int previousLevel, int currentLevel) {
            if (spaceshipModel!=null) {
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

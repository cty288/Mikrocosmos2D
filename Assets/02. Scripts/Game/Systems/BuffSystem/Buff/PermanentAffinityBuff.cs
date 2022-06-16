using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyglot;

namespace Mikrocosmos
{
    public class OnPermanentAffinityAddition : BuffClientMessage {
        public float AdditionPercentage;
    }
    public class PermanentAffinityBuff : PermanentRawMaterialBuff {
        private float additionalAffinityAdditionPercentage;

        public float AdditionalAffinityAdditionPercentage => additionalAffinityAdditionPercentage;

        public PermanentAffinityBuff(float additionalAffinityAdditionPercentage = 0.1f, int currentLevel = 0, int currentProgressInLevel = 1) : base(currentLevel, currentProgressInLevel) {
            this.additionalAffinityAdditionPercentage = additionalAffinityAdditionPercentage;

        }

        public override void OnLevelUp(int previousLevel, int currentLevel)
        {
            /*
            ClientMessage = new OnPermanentVisionExpansion() {
                VisionChangeEvent = new OnVisionPermanentChange() {
                    IncreasePercentage = (currentLevel-previousLevel) * additionalVisionAdditionPercentage
                }
            };*/

        }

        public override string Name { get; } = "PermanentAffinityBuff";
        public override string GetLocalizedDescriptionText()
        {
            return Localization.GetFormat("GAME_PERM_BUFF_AFFINITY_DESCRIPTION", ((int)(additionalAffinityAdditionPercentage * 100)) * CurrentLevel);
        }

        public override string GetLocalizedName()
        {
            return Localization.Get("GAME_PERM_BUFF_AFFINITY");
        }


        private int previousLevel = 0;

        public override BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();


        public override int MaxLevel { get; set; } = 5;
        public override List<int> ProgressPerLevel { get; set; } = new List<int>() { 1, 2, 2, 2, 3};
    }
}

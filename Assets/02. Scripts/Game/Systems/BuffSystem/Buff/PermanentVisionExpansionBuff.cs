using System.Collections;
using System.Collections.Generic;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnVisionPermanentChange {
        public float IncreasePercentage;
    }
    public class OnPermanentVisionExpansion : BuffClientMessage
    {
        public OnVisionPermanentChange VisionChangeEvent;
    }
    public class PermanentVisionExpansionBuff : PermanentRawMaterialBuff {

        private float additionalVisionAdditionPercentage;

        public PermanentVisionExpansionBuff(float additionalVisionAdditionPercentage = 0.2f, int currentLevel = 0, int currentProgressInLevel=1) : base(currentLevel, currentProgressInLevel) {
            this.additionalVisionAdditionPercentage = additionalVisionAdditionPercentage;
            
        }

        public override void OnLevelUp(int previousLevel, int currentLevel) {
            /*
            ClientMessage = new OnPermanentVisionExpansion() {
                VisionChangeEvent = new OnVisionPermanentChange() {
                    IncreasePercentage = (currentLevel-previousLevel) * additionalVisionAdditionPercentage
                }
            };*/
            
        }

        public override string Name { get; } = "PermanentVisionExpansionBuff";
        public override string GetLocalizedDescriptionText(Language language) {
            return Localization.GetFormat("GAME_PERM_BUFF_VISION_EXPANSION_DESCRIPTION", language, ((int) (additionalVisionAdditionPercentage * 100)) * CurrentLevel);
        }

        public override string GetLocalizedName(Language language) {
            return Localization.Get("GAME_PERM_BUFF_VISION_EXPANSION", language);
        }


        private int previousLevel = 0;

        public override BuffClientMessage MessageToClient {
            get {
                int IncreaseLevel = (CurrentLevel - previousLevel);
                previousLevel = CurrentLevel;
                return new OnPermanentVisionExpansion()
                {
                    VisionChangeEvent = new OnVisionPermanentChange()
                    {
                        IncreasePercentage = IncreaseLevel * additionalVisionAdditionPercentage
                    }
                }; 
            }
            set{}
        }


        public override int MaxLevel { get; set; } = 5;
        public override List<int> ProgressPerLevel { get; set; } = new List<int>() {1, 2, 3, 4, 5};
    }
}

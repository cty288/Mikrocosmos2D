using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroFramework.ActionKit;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos {

    public class OnVisionExpansion : BuffClientMessage {
        public OnCameraViewChange CameraViewChangeEvent;
        public OnVisionRangeChange VisionRangeChangeEvent;
    }
    public class VisionExpansionBuff : ITimedBuff, IStackableBuff<VisionExpansionBuff> {
        private float cameraRangeAddition;
        private Vector2 visionLightExpansion;

        
        public VisionExpansionBuff(float maxDuration, float cameraRangeAddition, Vector2 visionLightExpansion) {
            this.cameraRangeAddition = cameraRangeAddition;
            this.visionLightExpansion = visionLightExpansion;
            this.RemainingTime = maxDuration;
            this.MaxDuration = maxDuration;

            
            UpdateMessageToClient();



            OnTimedActionEnd = CallbackAction.Allocate(() => {
                //UpdateMessageToClient(false);
            });
            OnTimedActionEnd.SetAutoRecycle(false);
        }

        private void UpdateMessageToClient(bool expand = true) {
            int factor = expand ? 1 : -1;
            MessageToClient = new OnVisionExpansion()
            {
                CameraViewChangeEvent = new OnCameraViewChange() { RadiusAddition =factor * (int)this.cameraRangeAddition },
                VisionRangeChangeEvent = new OnVisionRangeChange()
                    { InnerAddition = factor * (int)visionLightExpansion.x, 
                        OuterAddition = factor * (int)this.visionLightExpansion.y }
            };
        }
        public void OnBuffStacked(VisionExpansionBuff addedBuff) {
            cameraRangeAddition += addedBuff.cameraRangeAddition;
            visionLightExpansion += addedBuff.visionLightExpansion;
            RemainingTime += addedBuff.RemainingTime;
            UpdateMessageToClient();
        }

        public string Name { get; } = "VisionExpansionBuff";
        public string GetLocalizedDescriptionText() {
            return Localization.Get("GAME_BUFF_VISION_EXPANSION_DESCRIPTION");
        }

        public string GetLocalizedName() {
            return Localization.Get("GAME_BUFF_VISION_EXPANSION");
        }

        public BuffClientMessage MessageToClient { get; set; } = new OnVisionExpansion();

        public float MaxDuration { get; protected set; }
        public float RemainingTime { get; set; }
        public MikroAction OnTimedActionEnd { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroFramework.ActionKit;
using Mirror;
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

        private bool isTemporary;
        public VisionExpansionBuff(float maxDuration, float cameraRangeAddition, Vector2 visionLightExpansion, bool temporary = false) {
            this.cameraRangeAddition = cameraRangeAddition;
            this.visionLightExpansion = visionLightExpansion;
            this.RemainingTime = maxDuration;
            this.MaxDuration = maxDuration;
            isTemporary = temporary;
            
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
                    { InnerAddition = factor *  (int) (visionLightExpansion.x * 1.5f), 
                        OuterAddition = factor * (int) (this.visionLightExpansion.y * 1.5f) }
            };
        }
        public void OnBuffStacked(VisionExpansionBuff addedBuff) {
            if (!addedBuff.isTemporary) {
                cameraRangeAddition += addedBuff.cameraRangeAddition;
                visionLightExpansion += addedBuff.visionLightExpansion;
            }
          
            RemainingTime = Mathf.Min(RemainingTime + addedBuff.RemainingTime, MaxDuration);
            UpdateMessageToClient();
        }

        public string Name { get; } = "VisionExpansionBuff";
        public string GetLocalizedDescriptionText(Language language) {
            return Localization.Get("GAME_BUFF_VISION_EXPANSION_DESCRIPTION", language);
        }

        public string GetLocalizedName(Language language) {
            return Localization.Get("GAME_BUFF_VISION_EXPANSION", language);
        }

        public BuffClientMessage MessageToClient { get; set; } = new OnVisionExpansion();
        public IBuffSystem Owner { get; set; }
        public NetworkIdentity OwnerIdentity { get; set; }

        public void OnBuffAdded() {
            
        }

        public float MaxDuration { get; protected set; }
        public float RemainingTime { get; set; }
        public MikroAction OnTimedActionEnd { get; set; }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.ActionKit;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class VisionOcclusionDebuff : ITimedBuff, IStackableBuff<VisionOcclusionDebuff> {
        public void OnBuffStacked(VisionOcclusionDebuff addedBuff) {
            RemainingTime = Mathf.Min(RemainingTime + addedBuff.RemainingTime, MaxDuration);
        }

        public string Name { get; } = "VisionOcclusionDeBuff";

        public VisionOcclusionDebuff(float maxDuration) {
            this.MaxDuration = maxDuration;
            this.RemainingTime = maxDuration;
            OnTimedActionEnd = CallbackAction.Allocate(() => {
                //UpdateMessageToClient(false);
            });
            OnTimedActionEnd.SetAutoRecycle(false);
            
        }

      
        public string GetLocalizedDescriptionText(Language languege) {
            return Localization.Get("GAME_BUFF_VISION_OCCLUSION_DESCRIPTION", languege);
        }

        public string GetLocalizedName(Language languege) {
            return Localization.Get("GAME_BUFF_VISION_OCCLUSION", languege);
        }

        public BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
        public IBuffSystem Owner { get; set; }
        public NetworkIdentity OwnerIdentity { get; set; }
        public void OnBuffAdded() {
            
        }

        public float MaxDuration { get; }
        public float RemainingTime { get; set; }
        public MikroAction OnTimedActionEnd { get; set; }
    }
}

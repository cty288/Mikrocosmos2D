using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public enum PermanentBuffType {
        Affinity,
        Health,
        PowerUp,
        Speed,
        VisionExpansion
    }
    public class PermanentBuffFactory {
        public static void AddPermanentBuffToPlayer(PermanentBuffType buffType, IBuffSystem buffSystem, int initialLevel, int progressInLevel = 0) {
            switch (buffType) {
                case PermanentBuffType.Affinity:
                    buffSystem.AddBuff<PermanentAffinityBuff>(new PermanentAffinityBuff(0.1f, initialLevel,
                        progressInLevel));
                    break;
                case PermanentBuffType.Health:
                    buffSystem.AddBuff<PermanentHealthBuff>(new PermanentHealthBuff(0.2f, initialLevel, progressInLevel));
                    break;
                case PermanentBuffType.PowerUp:
                    buffSystem.AddBuff<PermanentPowerUpBuff>(new PermanentPowerUpBuff(0.1f, initialLevel, progressInLevel));
                    break;
                case PermanentBuffType.Speed:
                    buffSystem.AddBuff<PermanentSpeedBuff>(new PermanentSpeedBuff(0.2f, initialLevel, progressInLevel));
                    break;
                case PermanentBuffType.VisionExpansion:
                    buffSystem.AddBuff<PermanentVisionExpansionBuff>(
                        new PermanentVisionExpansionBuff(0.2f, initialLevel, progressInLevel));
                    break;
            }
        }
    }
}

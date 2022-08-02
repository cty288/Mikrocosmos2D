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
                    buffSystem.AddBuff<PermanentPowerUpBuff>(new PermanentPowerUpBuff(0.15f, initialLevel, progressInLevel));
                    break;
                case PermanentBuffType.Speed:
                    buffSystem.AddBuff<PermanentSpeedBuff>(new PermanentSpeedBuff(0.1f, initialLevel, progressInLevel));
                    break;
                case PermanentBuffType.VisionExpansion:
                    buffSystem.AddBuff<PermanentVisionExpansionBuff>(
                        new PermanentVisionExpansionBuff(0.15f, initialLevel, progressInLevel));
                    break;
            }
        }

        public static void ReducePermanentBuffForPlayer(PermanentBuffType buffType, IBuffSystem buffSystem, int level)
        {
            switch (buffType)
            {
                case PermanentBuffType.Affinity:
                    buffSystem.RawMaterialLevelDecrease(typeof(PermanentAffinityBuff), level);
                    break;
                case PermanentBuffType.Health:
                    buffSystem.RawMaterialLevelDecrease(typeof(PermanentHealthBuff), level);
                    break;
                case PermanentBuffType.PowerUp:
                    buffSystem.RawMaterialLevelDecrease(typeof(PermanentPowerUpBuff), level);
                    break;
                case PermanentBuffType.Speed:
                    buffSystem.RawMaterialLevelDecrease(typeof(PermanentSpeedBuff), level);
                    break;
                case PermanentBuffType.VisionExpansion:
                    buffSystem.RawMaterialLevelDecrease(typeof(PermanentVisionExpansionBuff), level);
                    break;
            }
        }
    }
}

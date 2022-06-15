using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class MetalBallViewController : BasicGoodsViewController {
        [SerializeField] private float visionIncrementPercentage = 0.2f;
        protected override void OnServerItemUsed() {
            base.OnServerItemUsed();
            if (Owner) {
                if (Owner.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                    buffSystem.AddBuff<PermanentVisionExpansionBuff>(new PermanentVisionExpansionBuff(visionIncrementPercentage));
                }
            }
            GoodsModel.ReduceDurability(1);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public class WoodTypeViewController : BasicGoodsViewController
    {
        [SerializeField] private float affinityIncrementPercentage = 0.1f;
        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            if (Owner) {
                if (Owner.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                    buffSystem.AddBuff<PermanentAffinityBuff>(new PermanentAffinityBuff(affinityIncrementPercentage));
                }
            }
            GoodsModel.ReduceDurability(1);
        }
    }
}

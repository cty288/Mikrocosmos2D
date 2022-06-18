using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class WaterTypeViewController : BasicGoodsViewController
    {
        [SerializeField] private float speedIncreasePercentage = 0.2f;
        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            if (Owner)
            {
                if (Owner.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                    buffSystem.AddBuff<PermanentSpeedBuff>(new PermanentSpeedBuff(speedIncreasePercentage));
                }
            }
            GoodsModel.ReduceDurability(1);
        }
    }
}

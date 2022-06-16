using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class FireTypeViewController : BasicGoodsViewController
    {
        [SerializeField] private float damageIncreasePercentage = 0.15f;
        protected override void OnServerItemUsed() {
            base.OnServerItemUsed();
            if (Owner)
            {
                if (Owner.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem))
                {
                    buffSystem.AddBuff<PermanentSpeedBuff>(new PermanentPowerUpBuff(damageIncreasePercentage));
                }
            }
            GoodsModel.ReduceDurability(1);
        }
    }
}

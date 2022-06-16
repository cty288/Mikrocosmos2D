using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class EarthTypeViewController : BasicGoodsViewController
    {
        [SerializeField] private float healthIncreasePercentage = 0.2f;
        protected override void OnServerItemUsed()
        {
            base.OnServerItemUsed();
            if (Owner)
            {
                if (Owner.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                    buffSystem.AddBuff<PermanentHealthBuff>(new PermanentHealthBuff(healthIncreasePercentage,
                        Owner.GetComponent<ISpaceshipConfigurationModel>()));
                }
            }
            GoodsModel.ReduceDurability(1);
        }
    }
}

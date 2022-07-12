using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class JellyGunBulletViewController : BasicBulletViewController {
        [ServerCallback]
        protected override void OnServerSpaceshipHit(int damage, GameObject spaceship) {
            base.OnServerSpaceshipHit(damage, spaceship);
            if (damage > 0) {
                if (spaceship.TryGetComponent<IBuffSystem>(out IBuffSystem buffSystem)) {
                    buffSystem.AddBuff<VisionOcclusionDebuff>(new VisionOcclusionDebuff(2.5f));
                }
            }
        }
    }
}

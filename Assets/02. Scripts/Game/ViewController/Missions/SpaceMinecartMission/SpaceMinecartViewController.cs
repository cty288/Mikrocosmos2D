using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class SpaceMinecartViewController : AbstractNetworkedController<Mikrocosmos>, IHaveMomentumViewController {
        public IHaveMomentum Model { get; protected set; }

        private void Awake() {
            Model = GetComponent<SpaceMinecartModel>();
        }
    }
}

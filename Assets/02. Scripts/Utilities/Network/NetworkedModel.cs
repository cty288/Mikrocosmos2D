using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public abstract class NetworkedModel : NetworkBehaviour, IModel
    {
        private IArchitecture architectureModel;
        public IArchitecture GetArchitecture() {
            return architectureModel;
        }

        public void SetArchitecture(IArchitecture architecture) {
            this.architectureModel = architecture;
        }

        public void Init() {
           
        }
    }
}

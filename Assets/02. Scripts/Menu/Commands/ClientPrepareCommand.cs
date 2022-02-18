using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientRequestPrepare {
        
    }
    public class ClientPrepareCommand : AbstractCommand<ClientPrepareCommand> {
      
        public ClientPrepareCommand(){}

        protected override void OnExecute() {
            this.SendEvent<OnClientRequestPrepare>();
        }
    }
}

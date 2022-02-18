using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientShowHostUI { }
    public class ShowHostUICommand : AbstractCommand<ShowHostUICommand>
    {
        protected override void OnExecute() {
            this.SendEvent<OnClientShowHostUI>();
        }
    }
}

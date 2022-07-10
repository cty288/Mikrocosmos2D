using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class PickaxeOreModel : BasicGoodsModel {
        
        public override void OnServerHooked() {
            base.OnServerHooked();
            UnHook();
        }
    }
}

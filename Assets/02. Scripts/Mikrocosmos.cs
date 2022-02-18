using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using UnityEngine;

namespace Mikrocosmos
{
    public class Mikrocosmos : Architecture<Mikrocosmos> {
        protected override void Init() {
            this.RegisterModel<ILocalPlayerInfoModel>(new LocalPlayerInfoModel());
           // this.RegisterSystem<IRoomMatchSystem>(RoomMatchSystem.Singleton);
            this.RegisterSystem<ITimeSystem>(new TimeSystem());
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using UnityEngine;

namespace Mikrocosmos
{
    public class Mikrocosmos : NetworkedArchitecture<Mikrocosmos> {
        protected override void Init() {
            this.RegisterModel<ILocalPlayerInfoModel>(new LocalPlayerInfoModel());
           // this.RegisterModel<ISpaceshipConfigurationModel>(new SpaceshipConfigurationModel());
           // this.RegisterModel<ISpaceshipModel>(new SpaceshipModel());
            this.RegisterSystem<ITimeSystem>(new TimeSystem());
        }

        protected override void SeverInit() {
            
        }

        protected override void ClientInit() {
           
        }
    }
}

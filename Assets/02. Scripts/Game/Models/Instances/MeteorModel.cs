using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IMeteorModel: IModel {

    }
    public class MeteorModel : AbstractBasicEntityModel, IMeteorModel {
        public override float SelfMass { get; } = 5f;
        public override string Name { get; } = "Meteor";
        public override void OnHooked() {
            
        }

        public override void OnFreed() {
           
        }
    }
}

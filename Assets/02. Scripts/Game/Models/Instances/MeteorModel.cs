using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IMeteorModel: IModel {

    }
    public class MeteorModel : AbstractBasicEntityModel, IMeteorModel {
        [field: SyncVar, SerializeField]
        public override float SelfMass { get; protected set; } = 5f;
        public override string Name { get; } = "Meteor";
        public override void OnHooked() {
            
        }

        public override void OnFreed() {
           
        }
    }
}

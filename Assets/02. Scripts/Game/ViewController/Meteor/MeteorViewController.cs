using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class MeteorViewController : BasicEntityViewController
    {
        public override IEntity Model { get; protected set; }

        private IMeteorModel GetModel()
        {
            return GetModel<IMeteorModel>();
        }
    }
}

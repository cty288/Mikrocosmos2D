using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public enum MoveMode {
        ByPhysics,
        ByTransform
    }
    public interface IHaveMomentum {

        MoveMode MoveMode { get; set; }
        float MaxSpeed { get; }

        float Acceleration { get;  }
        /// <summary>
        /// This is the self mass of this object
        /// </summary>
        float SelfMass { get;  set; }

        /// <summary>
        /// This is the total mass of this object, usually self mass; for player. it is equal to selfmass + backpack + hooked objects
        /// </summary>
        /// <returns></returns>
        float GetTotalMass();

      
    }
}

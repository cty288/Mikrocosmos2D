using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IHaveMomentum {
        float SelfMass { get;}
        float MaxSpeed { get;}

        float Acceleration { get;}

        float GetTotalMass();

        float GetMomentum();
    }
}

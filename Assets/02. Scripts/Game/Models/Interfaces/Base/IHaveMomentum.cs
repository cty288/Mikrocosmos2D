using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IHaveMomentum {
        BindableProperty<float> SelfMass { get;}
        BindableProperty<float> MaxSpeed { get;}

        BindableProperty<float> Acceleration { get;}

        float GetTotalMass();

        float GetMomentum();
    }
}

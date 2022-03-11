using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IDamagable {
        BindableProperty<float> CurrentHP { get; }
        float MaxHP { get; }

        BindableProperty<float> MomentumThredhold { get; }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IDamagable {
        float MaxMomentumReceive { get; }
        float MomentumThredhold { get; }
    }
}

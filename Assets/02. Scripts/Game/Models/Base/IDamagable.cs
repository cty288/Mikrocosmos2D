using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IDamagable: IHaveMomentum {
        float MaxMomentumReceive { get; }
        float MomentumThredhold { get; }

        /// <summary>
        /// -1 = infinite health
        /// </summary>
        int MaxHealth { get; }

        int CurrentHealth { get; set; }

        /// <summary>
        /// Return excessive momentum
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        float TakeRawMomentum(GameObject hit); //called by Hit IHaveMomentum

        int GetDamageFromExcessiveMomentum(float excessiveMomentum); //called by Hit IHaveMomentum

        //int OnHealthChange(int newHealth);

        //float OnReceiveExcessiveMomentum(float excessiveMomentum);
    }
}

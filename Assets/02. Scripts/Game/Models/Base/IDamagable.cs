using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Mirror;
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
        float TakeRawMomentum(float rawMomentum, float offset); //called by Hit IHaveMomentum

        int GetDamageFromExcessiveMomentum(float excessiveMomentum); //called by Hit IHaveMomentum

        int TakeRawDamage(int damage, NetworkIdentity damageDealer, int additionalOffset=0);

        void AddHealth(int health);
        
        bool CanAutoRecoverHealth { get; set; }
        
        public void AddMaximumHealth(float percentage);
        void OnReceiveExcessiveMomentum(float excessiveMomentum);
    }
}

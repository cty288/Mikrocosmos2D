using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public enum HookState
    {
        Freed,
        Hooked
    }

    public interface ICanAbsorbDamage : IHookable {
        
        bool AbsorbDamage { get; set; }
        void OnAbsorbDamage(float damage);
    }
    
   public interface IHookable: IHaveMomentum
    {
        public string Name { get; set; }

        public bool CanBeHooked { get; set; }
        public bool CanBeAddedToInventory { get; set; }

        //public bool CanBeUsed { get; set; }
      //  public int Durability { get; set; }

        //public int MaxDurability { get; }
        HookState HookState { get; }

        NetworkIdentity HookedByIdentity { get; }

        Transform HookedByTransform { get; }

        bool Hook(NetworkIdentity hookedBy);
         
        bool canDealMomentumDamage { get; set; }

        void UnHook(bool isShoot);

        void UnHook();
    }

    public enum ItemUseMode {
        UseWhenKeyDown,
        UseWhenPressingKey
    }

    /// <summary>
    /// ICanBeUsed is IHookable
    /// </summary>
    public interface ICanBeUsed : IHookable {
        public bool CanBeUsed { get; set; }
        
        public ItemUseMode UseMode { get; }

        /// <summary>
        /// Min time interval between two use (in seconds)
        /// </summary>
        public float Frequency { get; set; }
        public int Durability { get; set; }

        public int MaxDurability { get; set; }

        public bool IsUsing { get; }

        void OnItemUsed();

        void OnItemStopUsed();

        public void ReduceDurability(int count, bool isDestroyed = false);
    }

}

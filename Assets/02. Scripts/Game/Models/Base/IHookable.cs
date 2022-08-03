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
        int OnAbsorbDamage(float damage);
    }
    
   public interface IHookable: IHaveMomentum
    {
        public string Name { get; set; }

        public bool CanBeHooked { get; set; }
        public bool CanBeAddedToInventory { get; set; } 
        
        public float AdditionalMassWhenHookedMultiplier { get; set; }

        //public bool CanBeUsed { get; set; }
      //  public int Durability { get; set; }

        //public int MaxDurability { get; }
        HookState HookState { get; }

        NetworkIdentity HookedByIdentity { get; }

        NetworkIdentity LastHookedByIdentity { get; }
        Transform HookedByTransform { get; }

        bool TryHook(NetworkIdentity hookedBy);

        void OnServerHooked();

       


       bool canDealMomentumDamage { get; set; }

        /// <summary>
        /// This should only called by the hook
        /// </summary>
        /// <param name="isShoot"></param>
        /// <param name="isUnHookedByHookButton"></param>
        void UnHookByHook(bool isShoot, bool isUnHookedByHookButton);

        /// <summary>
        /// Call this method if the object want to unhooked from its owner
        /// </summary>
        /// <param name="isUnHookedByHookButton"></param>
        void UnHook(bool isUnHookedByHookButton);
    }

    public enum ItemUseMode {
        UseWhenKeyDown,
        UseWhenPressingKey
    }

    /// <summary>
    /// ICanBeUsed is IHookable
    /// </summary>
    public interface ICanBeUsed : IGoods {
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

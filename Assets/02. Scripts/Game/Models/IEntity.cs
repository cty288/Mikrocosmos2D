using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IEntity : IHookable, ICanBeShot {
        public bool Frozen { get; }
        
        public void SetFrozen(bool freeze);
        void ResetEntity();
    }

  
}

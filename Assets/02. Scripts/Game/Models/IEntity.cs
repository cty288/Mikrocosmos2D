using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IEntity : IHookable, ICanBeShot {
        void ResetEntity();
    }

  
}

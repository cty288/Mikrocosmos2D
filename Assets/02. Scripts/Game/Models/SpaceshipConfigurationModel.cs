using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ISpaceshipConfigurationModel  {
         float MoveForce { get; set; }

         IHookableViewController HookedItem { get; set; }

  

    }
    
}

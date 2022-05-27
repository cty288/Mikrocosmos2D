using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IHaveGravityViewController : IController {
        IHaveGravity GravityModel { get; }
    }
}

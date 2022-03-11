using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IEntity : IPickable, ICanBeShot {
       string Name { get; }
    }
}

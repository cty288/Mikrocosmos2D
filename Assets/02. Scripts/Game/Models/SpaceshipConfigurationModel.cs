using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ISpaceshipConfigurationModel : IModel, IEntity  {
        public float SelfMass { get; }

        public float BackpackMass { get; }

        public int EscapeNeedCount { get; }

        public float EscapeLossTime { get; }

        public int EscapeCounter { get; }

        /// <summary>
        /// Get only the mass of the connected object; if its a spaceship, also calculate its backpack
        /// This will also get the mass of the connected object's connect objects
        /// </summary>
        /// <returns></returns>
        public float GetConnectedObjectSoleMass();

        public void IncreaseEscapeCounter();
    }
    
}

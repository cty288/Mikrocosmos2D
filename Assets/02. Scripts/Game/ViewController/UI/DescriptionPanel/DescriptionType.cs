using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class DescriptionItem {
        public DescriptionPanel SpawnedDescriptionPanel;
        public DescriptionType DescriptionType;
    }
    public enum DescriptionType {
        Item,
        Mission
    }
}

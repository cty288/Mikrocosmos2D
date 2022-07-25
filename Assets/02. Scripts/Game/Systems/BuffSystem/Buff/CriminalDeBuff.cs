using System.Collections;
using System.Collections.Generic;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public class CriminalDeBuff : IBuff {
        public string Name { get; } = "CriminalDeBuff";
        public string GetLocalizedDescriptionText(Language languege) {
            return Localization.Get("GAME_BUFF_CRIMINAL_DESCRIPTION", languege);
        }

        public string GetLocalizedName(Language languege) {
            return Localization.Get("GAME_BUFF_CRIMINAL", languege);
        }

        public BuffClientMessage MessageToClient { get; set; } = new BuffClientMessage();
        public IBuffSystem Owner { get; set; }
        public NetworkIdentity OwnerIdentity { get; set; }
        public void OnBuffAdded() {
            
        }
    }
}

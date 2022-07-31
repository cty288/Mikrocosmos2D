using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnDescriptionItemAdd {
        public DescriptionItem Item;
    }
    
    public class AddOrUpdateDescriptionCommand : AbstractCommand<AddOrUpdateDescriptionCommand> {
        private DescriptionItem descriptionItem;
        public AddOrUpdateDescriptionCommand(DescriptionItem descriptionItem)
        {
            this.descriptionItem = descriptionItem;
        }

        public AddOrUpdateDescriptionCommand(){}
        protected override void OnExecute() {
            if (descriptionItem != null) {
                this.SendEvent<OnDescriptionItemAdd>(new OnDescriptionItemAdd() {
                    Item = descriptionItem
                });
            }
            
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct ChangeNameSuccess
    {

    }
    public class ChangeDisplayNameCommand : AbstractCommand<ChangeDisplayNameCommand>
    {
        private string nameToChange;
        protected override void OnExecute()
        {
            this.GetModel<ILocalPlayerInfoModel>().ChangeName(nameToChange);
            this.SendEvent<ChangeNameSuccess>();
        }

        public ChangeDisplayNameCommand() { }

        public ChangeDisplayNameCommand(string nameToChange)
        {
            this.nameToChange = nameToChange;

        }

       


    }
}

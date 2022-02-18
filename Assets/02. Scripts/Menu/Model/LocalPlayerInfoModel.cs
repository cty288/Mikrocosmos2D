using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ILocalPlayerInfoModel: IModel {
         public BindableProperty<string> NameInfo { get; }
         public void ChangeName(string name);
    }
    public class LocalPlayerInfoModel : AbstractModel, ILocalPlayerInfoModel
    {
        protected override void OnInit() {
            NameInfo.Value = ES3.LoadString("name", "");
        }

        public BindableProperty<string> NameInfo { get; private set; } = new BindableProperty<string>();
        public void ChangeName(string name) {
            NameInfo.Value = name;
            ES3.Save("name", name);
        }
    }
}

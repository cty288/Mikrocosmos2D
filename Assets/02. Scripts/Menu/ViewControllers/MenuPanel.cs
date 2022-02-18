using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;
using MikroFramework.Event;

namespace Mikrocosmos {
	public partial class MenuPanel : AbstractMikroController<Mikrocosmos> {
        private void Awake() {
            this.GetModel<ILocalPlayerInfoModel>().NameInfo.RegisterWithInitValue(OnNameChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnNameChange(string oldName, string newName) {
            TextName.text = newName;
        }
    }
}
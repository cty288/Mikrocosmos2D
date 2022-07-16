using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class CreateAvatarPanel : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private GameObject ObjCreateAvatarPanel;
		[SerializeField] private TMP_InputField InputName;
		[SerializeField] private Button BtnRandom;
		[SerializeField] private Button BtnSaveLook;
		[SerializeField] private Button BtnBack;
		[SerializeField] private GameObject ObjAvatarLayout;
	}
}
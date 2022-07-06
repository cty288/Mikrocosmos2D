using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class PlayerInfo : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private Image ImgAvatar;
		[SerializeField] private TMP_Text TextName;
		[SerializeField] private Button BtnKick;
		[SerializeField] private Button BtnPrepare;
		[SerializeField] private Button BtnUnPrepare;
	}
}
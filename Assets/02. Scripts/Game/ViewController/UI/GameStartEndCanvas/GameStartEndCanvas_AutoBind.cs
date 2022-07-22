using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class GameStartEndCanvas : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private Image ImgStartBG;
		[SerializeField] private GameObject ObjCountdown;
		[SerializeField] private TMP_Text TextCountdownTime;
	}
}
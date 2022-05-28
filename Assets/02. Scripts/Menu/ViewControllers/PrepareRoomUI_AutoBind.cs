using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class PrepareRoomUI : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private GameObject ObjTeam1Layout;
		[SerializeField] private GameObject ObjTeam2Layout;
		[SerializeField] private Button BtnChangeSide;
		[SerializeField] private Button BtnRoomLeaderStartRoom;
		[SerializeField] private Button BtnTestMode;
		[SerializeField] private Button BtnBack;
	}
}
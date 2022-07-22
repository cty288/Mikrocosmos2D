using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class PrepareRoomUI : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private GameObject ObjTeam1Panel;
		[SerializeField] private GameObject ObjTeam1SlotsBG;
		[SerializeField] private GameObject ObjTeam1Layout;
		[SerializeField] private Button BtnChangeSide;
		[SerializeField] private Button BtnRoomLeaderStartRoom;
		[SerializeField] private Button BtnTestMode;
		[SerializeField] private Button BtnBack;
		[SerializeField] private TMP_Text TextPort;
		[SerializeField] private TMP_Dropdown DropdownGameMode;
		[SerializeField] private GameObject ObjTeam2Panel;
		[SerializeField] private GameObject ObjTeam2SlotsBG;
		[SerializeField] private GameObject ObjTeam2Layout;
		[SerializeField] private GameObject ObjGameReadyToStartBG;
	}
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class FindServerPanel : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private GameObject ObjFindServerPanel;
		[SerializeField] private Transform TrRoomLayoutGroup;
		[SerializeField] private Button BtnRoomSearchPanelBackToMenu;
		[SerializeField] private Button BtnManualAddServer;
		[SerializeField] private GameObject ObjManualAddServerPanel;
		[SerializeField] private TMP_InputField InputIPInput;
		[SerializeField] private TMP_InputField InputPort;
		[SerializeField] private Button BtnAddServerJoinRoom;
		[SerializeField] private Button BtnAddServerBackRoom;
		[SerializeField] private GameObject ObjJoiningRoomPanel;
		[SerializeField] private TMP_Text TextJoinRoomPanelInfo;
		[SerializeField] private Button BtnCloseJoinRoomPanel;
	}
}
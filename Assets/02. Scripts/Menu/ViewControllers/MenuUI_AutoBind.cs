using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class MenuUI : AbstractMikroController<Mikrocosmos> {
	//	[SerializeField] private GameObject ObjNewPlayerPanelParent;
		[SerializeField] private TMP_InputField InputNameInput;
		[SerializeField] private Button BtnNameConfirmButton;
		[SerializeField] private GameObject ObjMenuPanel;
		[SerializeField] private TMP_Text TextName;
		[SerializeField] private Button BtnHostGame;
		[SerializeField] private Button BtnFindGame;
		[SerializeField] private GameObject ObjHostGameOptionPanel;
		[SerializeField] private Button BtnHostLocalNetwork;
		[SerializeField] private Button BtnHostSteam;
		[SerializeField] private GameObject ObjFindServerPanel;
		[SerializeField] private Transform TrRoomLayoutGroup;
		[SerializeField] private Button BtnRoomSearchPanelBackToMenu;
		[SerializeField] private Button BtnManualAddServer;
	}
}
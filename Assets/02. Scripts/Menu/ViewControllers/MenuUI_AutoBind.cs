using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class MenuUI : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private GameObject ObjNewPlayerPanelParent;
		[SerializeField] private TMP_InputField InputNameInput;
		[SerializeField] private Button BtnNameConfirmButton;
		[SerializeField] private GameObject ObjMenuPanel;
		[SerializeField] private TMP_Text TextName;
		[SerializeField] private Button BtnHostGame;
		[SerializeField] private Button BtnFindGame;
		[SerializeField] private GameObject ObjFindServerPanel;
		[SerializeField] private Transform TrRoomLayoutGroup;
		[SerializeField] private TMP_Text TextRoomName;
		[SerializeField] private TMP_Text TextRoomNumber;
		[SerializeField] private Button BtnJoinButton;
		[SerializeField] private TMP_Text TextRoomStatus;
		[SerializeField] private Button BtnRoomSearchPanelBackToMenu;
	}
}
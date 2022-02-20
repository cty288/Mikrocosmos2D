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
		[SerializeField] private TMP_Text TextName;
		[SerializeField] private Transform TrRoomLayoutGroup;
		[SerializeField] private TMP_Text TextRoomName;
		[SerializeField] private TMP_Text TextRoomNumber;
		[SerializeField] private Button BtnJoinButton;
		[SerializeField] private TMP_Text TextRoomStatus;
	}
}
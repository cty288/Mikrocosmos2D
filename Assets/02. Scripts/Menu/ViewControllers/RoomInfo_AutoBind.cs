using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MikroFramework;
using TMPro;
using MikroFramework.Architecture;

namespace Mikrocosmos {
	public partial class RoomInfo : AbstractMikroController<Mikrocosmos> {
		[SerializeField] private TMP_Text TextRoomName;
		[SerializeField] private TMP_Text TextRoomNumber;
		[SerializeField] private Button BtnJoinButton;
		[SerializeField] private TMP_Text TextRoomStatus;
	}
}
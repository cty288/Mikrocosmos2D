using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class MissionProgressElement : MonoBehaviour {
        [SerializeField] private Sprite[] missionEndSprites;
        private Image winIcon;
        private Image winTeamColorSprite;
        private void Awake() {
            winTeamColorSprite = transform.Find("WinTeamSprite").GetComponent<Image>();
            winIcon = winTeamColorSprite.transform.Find("Image").GetComponent<Image>();
        }

        public void OnMissionProgressStop(int winTeam) {
            if (winTeam > 0) {
                winTeamColorSprite.sprite = missionEndSprites[winTeam - 1];
                winIcon.enabled = true;
            }
            else {
                winTeamColorSprite.sprite = missionEndSprites[2];
                winIcon.enabled = false;
            }
        }
    }
}

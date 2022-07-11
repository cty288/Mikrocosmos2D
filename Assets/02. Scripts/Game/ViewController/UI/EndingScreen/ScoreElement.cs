using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class ScoreElement : MonoBehaviour {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;

        public void SetInfo(string name, int score) {
            nameText.text = name;
            if (score < 0) {
                scoreText.text = "";
                return;
            }
            scoreText.text = score.ToString();
        }
    }
}

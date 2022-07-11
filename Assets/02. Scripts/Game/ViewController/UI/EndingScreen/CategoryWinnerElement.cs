using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class CategoryWinnerElement : MonoBehaviour {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;

        public void SetInfo(string name, string descriptionText)
        {
            nameText.text = name;
            this.descriptionText.text = descriptionText;
        }
    }
}

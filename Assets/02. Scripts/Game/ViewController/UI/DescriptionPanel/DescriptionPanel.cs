using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    [RequireComponent(typeof(ContentSizeFitter))]
    public abstract class DescriptionPanel : MonoBehaviour {
        [SerializeField] protected Image showImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] protected TMP_Text descriptionText;

        public void SetInfo(Sprite showSprite, string nameText, string descriptionText) {
            this.showImage.sprite = showSprite;
            this.nameText.text = nameText;
            this.descriptionText.text = descriptionText;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        //protected abstract void OnSetInfo();
    }
}

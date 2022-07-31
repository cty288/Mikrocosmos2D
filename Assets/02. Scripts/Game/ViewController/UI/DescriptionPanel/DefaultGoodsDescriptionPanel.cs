using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class DefaultGoodsDescriptionPanel : DescriptionPanel {

        [SerializeField] private Transform hintSpawnPosition;
        [SerializeField] private float descriptionTextWidthIfNoHintObject;
        public void SetInfo(Sprite showSprite, string nameText, string descriptionText, GameObject hintGameObject)
        {
            if (hintGameObject) {
                Instantiate(hintGameObject, hintSpawnPosition);
            }
            else {
                base.descriptionText.GetComponent<RectTransform>().sizeDelta = new Vector2(
                    descriptionTextWidthIfNoHintObject, base.descriptionText.GetComponent<RectTransform>().sizeDelta.y);
            }
            
            SetInfo(showSprite, nameText, descriptionText);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }
}

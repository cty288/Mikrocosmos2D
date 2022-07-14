using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public struct OnAvatarSingleElementSelected {
        public int Index;
        public int ReplacedIndex;
        public bool IsBase;
        public int ElementLayer;
    }
    public class AvatarSingleElement : AbstractMikroController<Mikrocosmos>, ICanSendEvent {
        private GameObject selectedObject;
        private Image baseImage;

        [SerializeField]
        private int assetIndex;

        public int AssetIndex {
            get {
                return assetIndex;
            }
        }

        private void Awake() {
            selectedObject = transform.Find("Selected").gameObject;
            baseImage = transform.Find("Base/Element").GetComponent<Image>();
        }

        public void SetElement(int index, Sprite sprite) {
            assetIndex = index;
            baseImage.sprite = sprite;
        }

        public void SetSelection(bool isSelected) {
            selectedObject.SetActive(isSelected);
        }
    }
}

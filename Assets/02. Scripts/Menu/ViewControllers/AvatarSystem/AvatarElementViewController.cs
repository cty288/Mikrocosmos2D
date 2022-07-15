using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.UI;


namespace Mikrocosmos
{
    public class AvatarElementViewController : MonoBehaviour {
        private Image baseElement;
        [SerializeField]
        private List<AvatarSubElementViewController> subElements = new List<AvatarSubElementViewController>();

        public List<AvatarSubElementViewController> SubElements => subElements;


        private void Awake() {
            baseElement = transform.Find("Base").GetComponent<Image>();
            subElements = baseElement.gameObject.GetComponentsInChildren<AvatarSubElementViewController>(true).ToList();
          //  subElements.RemoveAt(0);
        
        }

        public void SetAvatar(Avatar avatar) {
            StartCoroutine(WaitToSetAvatar(avatar));
        }

        private IEnumerator WaitToSetAvatar(Avatar avatar) {
            while (true) {
                if (AvatarElementCashManager.Singleton.IsReady) {
                    break;
                }
                yield return null;
            }
            yield return null;
         
            foreach (var subElement in subElements) {
                subElement.gameObject.SetActive(false);
            }
            
            //0-100 is a special index for base
            if (avatar.Elements.Count > 0) {
                baseElement.gameObject.SetActive(true);
               

                int minimumLayout = -1;
                List<AvatarElement> elements = avatar.GetAllElements();
                elements.Sort(((element1, element2) => element1.Layer.CompareTo(element2.Layer)));
                for (int i = 0; i < elements. Count; i++) {
                    AvatarElement element = elements[i];

                    Sprite sprite = AvatarElementCashManager.Singleton.GetSpriteElementFromAsset(element.ElementIndex);
                    if (!sprite) {
                        continue;
                    }

                    if (element.ElementIndex < 100) {
                        baseElement.sprite = sprite;
                    }
                    else {
                        if (element.Layer > minimumLayout) {
                            minimumLayout = element.Layer;
                        }
                        else {
                            minimumLayout++;
                        }
                        int targetLayout = minimumLayout;
                        if (targetLayout < subElements.Count) {
                            Image subElementImage = subElements[targetLayout].GetComponent<Image>();
                            subElementImage.gameObject.SetActive(true);
                            subElementImage.sprite = sprite;
                            subElementImage.GetComponent<RectTransform>().SetOffset(element.Offset.x,
                                -element.Offset.x, element.Offset.y, -element.Offset.y);
                            subElements[targetLayout].Index =
                                element.ElementIndex;
                        }
                    }
                    
                }
            }
        }
    }
}

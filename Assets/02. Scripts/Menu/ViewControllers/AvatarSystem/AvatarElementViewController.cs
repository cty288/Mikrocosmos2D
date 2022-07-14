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
        private List<Image> subElements = new List<Image>();

        private ResLoader resLoader;
        
        private void Awake() {
            baseElement = transform.Find("Base").GetComponent<Image>();
            subElements = baseElement.gameObject.GetComponentsInChildren<Image>(true).ToList();
            subElements.RemoveAt(0);
            ResLoader.Create(loader => resLoader = loader);
        }

        public void SetAvatar(Avatar avatar) {
            StartCoroutine(WaitToSetAvatar(avatar));
        }

        private IEnumerator WaitToSetAvatar(Avatar avatar) {
            while (true) {
                if (resLoader!=null && resLoader.IsReady) {
                    break;
                }
                yield return null;
            }

            foreach (Image subElement in subElements) {
                subElement.gameObject.SetActive(false);
            }
            
            //0-100 is a special index for base
            if (avatar.Elements.Count > 0) {
                baseElement.gameObject.SetActive(true);
                avatar.Elements.Sort(((element1, element2) => element1.Layer.CompareTo(element2.Layer)));

                int minimumLayout = -1;
                for (int i = 0; i < avatar.Elements.Count; i++) {
                    AvatarElement element = avatar.Elements[i];
                    
                    Texture2D texture = resLoader.LoadSync<Texture2D>("profile", $"profile{element.ElementIndex}");
                    if (!texture) {
                        continue;
                    }
                    
                  
                    Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    
                    if (element.ElementIndex < 100) {
                        baseElement.sprite = sprite;
                    }
                    else {
                        if (element.Layer < minimumLayout) {
                            minimumLayout = element.Layer;
                        }
                        else {
                            minimumLayout++;
                        }
                        int targetLayout = minimumLayout;
                        if (targetLayout < subElements.Count) {
                            subElements[targetLayout].gameObject.SetActive(true);
                            subElements[targetLayout].sprite = sprite;
                        }
                    }
                    
                }
            }
        }
    }
}

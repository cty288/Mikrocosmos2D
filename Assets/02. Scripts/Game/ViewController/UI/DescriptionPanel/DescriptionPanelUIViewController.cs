using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class DescriptionPanelUIViewController : AbstractMikroController<Mikrocosmos> {
        private Dictionary<DescriptionType, DescriptionPanel>
            descriptionItems = new Dictionary<DescriptionType, DescriptionPanel>();

        private LayoutGroup descriptionLayoutGroup;

        [SerializeField]
        private float fadeOutTIme = 10f;
        

        private void Awake() {
            this.RegisterEvent<OnDescriptionItemAdd>(OnDescriptionItemAdd).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnDescriptionRemoved>(OnDescriptionRemoved).UnRegisterWhenGameObjectDestroyed(gameObject);
            descriptionLayoutGroup = GetComponentInChildren<LayoutGroup>();
        }

        private void OnDescriptionRemoved(OnDescriptionRemoved e) {
            StartCoroutine(DestroyDescriptionPanel(e.Type, 0));
        }

        private void OnDescriptionItemAdd(OnDescriptionItemAdd e) {
            
            if (e.Item.SpawnedDescriptionPanel) {
                //get all images and text components
                Image[] images = e.Item.SpawnedDescriptionPanel.GetComponentsInChildren<Image>(true);
                TMP_Text[] texts = e.Item.SpawnedDescriptionPanel.GetComponentsInChildren<TMP_Text>(true);
                foreach (Image image in images) {
                    image.color = new Color(1, 1, 1, 0);
                }
                foreach (TMP_Text text in texts) {
                    text.color = new Color(1, 1, 1, 0);
                }
                
                if (descriptionItems.ContainsKey(e.Item.DescriptionType)) {
                    Destroy(descriptionItems[e.Item.DescriptionType].gameObject);
                    descriptionItems[e.Item.DescriptionType] = e.Item.SpawnedDescriptionPanel;
                    StartCoroutine(RecoverColor(images, texts));
                }
                else {
                    //set all images and text components to transparent
                    
                    //lerp all images and text components to opaque
                    foreach (Image image in images) {
                        image.DOFade(1, 0.5f);
                    }

                    foreach (TMP_Text text in texts) {
                        text.DOFade(1, 0.5f);
                    }
                    //add the description panel to the dictionary
                    descriptionItems.Add(e.Item.DescriptionType, e.Item.SpawnedDescriptionPanel);
                }

                //after fadeOutTime, if the description panel is still in the dictionary, destroy it
                StartCoroutine(DestroyDescriptionPanel(e.Item, fadeOutTIme));
                LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionLayoutGroup.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(e.Item.SpawnedDescriptionPanel.GetComponent<RectTransform>());
            }
        }

        private IEnumerator RecoverColor(Image[] images, TMP_Text[] texts) {
            yield return null;
            foreach (Image image in images)
            {
                image.color = new Color(1, 1, 1, 1);
            }
            foreach (TMP_Text text in texts)
            {
                text.color = new Color(1, 1, 1, 1);
            }

        }
        private IEnumerator DestroyDescriptionPanel(DescriptionItem descriptionItem, float f) {
            
            yield return null;
         
            LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionLayoutGroup.GetComponent<RectTransform>());
            if (descriptionItem != null && descriptionItem.SpawnedDescriptionPanel) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionItem.SpawnedDescriptionPanel.GetComponent<RectTransform>());
            }
            
            yield return new WaitForSeconds(f);

            if (descriptionItems.ContainsKey(descriptionItem.DescriptionType) && descriptionItems[descriptionItem.DescriptionType] == descriptionItem.SpawnedDescriptionPanel) {
                StartCoroutine(DestroyDescriptionPanel(descriptionItem.DescriptionType, 0));
            }
        }

        private IEnumerator DestroyDescriptionPanel(DescriptionType type, float time) {
            yield return new WaitForSeconds(time);
            if (descriptionItems.ContainsKey(type)) {
                DescriptionPanel descriptionItem = descriptionItems[type];
                descriptionItem.gameObject.transform.SetParent(descriptionItem.gameObject.transform.parent.parent);
                Image[] images = descriptionItem.GetComponentsInChildren<Image>(true);
                TMP_Text[] texts = descriptionItem.GetComponentsInChildren<TMP_Text>(true);
                descriptionItems.Remove(type);

                foreach (Image image in images)
                {
                    image.DOFade(0, 0.5f);
                }
                foreach (TMP_Text text in texts)
                {
                    text.DOFade(0, 0.5f);
                }

                yield return new WaitForSeconds(0.5f);

                Destroy(descriptionItem.gameObject);
                LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionLayoutGroup.GetComponent<RectTransform>());
            }
        }
    }
}

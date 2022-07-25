using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class AvatarElementSelectionPanel : MonoBehaviour {
        private List<AvatarElementTypeLayout> elementTypeLayouts = new List<AvatarElementTypeLayout>();

        private void Awake() {
            elementTypeLayouts = transform.Find("obj_AvatarLayout")
                .GetComponentsInChildren<AvatarElementTypeLayout>(true)
                .ToList();
        }

        public void RandomSelect() {
            foreach (var elementTypeLayout in elementTypeLayouts) {
                elementTypeLayout.RandomSelect();
            }
        }

        public void StartFill(Avatar existingAvatar) {
            foreach (AvatarElementTypeLayout avatarElementTypeLayout in elementTypeLayouts) {
                avatarElementTypeLayout.FillElements(() => {
                    int selectedIndex = avatarElementTypeLayout.ElementIndexRange.x;

                    foreach (AvatarElement avatarElement in existingAvatar.Elements.Values) {
                        int index = avatarElement.ElementIndex;
                        if (avatarElementTypeLayout.ElementIndexRange.x <= index &&
                            avatarElementTypeLayout.ElementIndexRange.y > index) {
                            selectedIndex = avatarElement.ElementIndex;
                        }
                    }

                    avatarElementTypeLayout.SelectElement(selectedIndex, existingAvatar.Elements.Count == 0);
                });
            }
        }

        public void SelectElementFromExistingAvatar(Avatar existingAvatar) {
            foreach (AvatarElementTypeLayout avatarElementTypeLayout in elementTypeLayouts) {
              
                int selectedIndex = avatarElementTypeLayout.ElementIndexRange.x;

                foreach (AvatarElement avatarElement in existingAvatar.Elements.Values) {
                    int index = avatarElement.ElementIndex;
                    if (avatarElementTypeLayout.ElementIndexRange.x <= index &&
                        avatarElementTypeLayout.ElementIndexRange.y > index) {
                        selectedIndex = avatarElement.ElementIndex;
                    }
                }

                avatarElementTypeLayout.SelectElement(selectedIndex, existingAvatar.Elements.Count == 0);
                
            }
        }
    }        
}

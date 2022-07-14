using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mikrocosmos
{
    public class AvatarElementSelectionPanel : MonoBehaviour {
        private List<AvatarElementTypeLayout> elementTypeLayouts = new List<AvatarElementTypeLayout>();

        private void Awake() {
            elementTypeLayouts = transform.Find("obj_AvatarLayout").GetComponentsInChildren<AvatarElementTypeLayout>()
                .ToList();
        }

        public void StartFill(Avatar existingAvatar) {
            foreach (AvatarElementTypeLayout avatarElementTypeLayout in elementTypeLayouts) {
                avatarElementTypeLayout.FillElements(() => {
                    int selectedIndex = avatarElementTypeLayout.ElementIndexRange.x;
                    
                    foreach (AvatarElement avatarElement in existingAvatar.Elements) {
                        int index = avatarElement.ElementIndex;
                        if (avatarElementTypeLayout.ElementIndexRange.x <= index &&
                            avatarElementTypeLayout.ElementIndexRange.y > index) {
                            selectedIndex = avatarElement.ElementIndex;
                        }
                    }

                    avatarElementTypeLayout.SelectElement(selectedIndex);
                });
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class InventoryUIViewController : AbstractMikroController<Mikrocosmos> {
        [SerializeField] 
        private GameObject slotPrefab;

        private Transform initialBackPackBG;

      
        private List<GameObject> allItemSlots = new List<GameObject>();

        private ResLoader resLoader;
        private void Awake() {
            this.RegisterEvent<OnClientInventoryUpdate>(OnInventoryUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnStartInventoryInit>(OnInventoryInit).UnRegisterWhenGameObjectDestroyed(gameObject);

            initialBackPackBG = transform.Find("InitialBackPackBG");

            ResLoader.Create((loader => resLoader = loader));
        }

        private void OnInventoryInit(OnStartInventoryInit e) {

            for (int i = 0; i < e.InitialBackPackCapacity; i++) {
                GameObject slot = Instantiate(slotPrefab, initialBackPackBG);
                slot.transform.SetAsLastSibling();
                allItemSlots.Add(slot);
            }


            Image slotBG = allItemSlots[0].GetComponent<Image>();
            float alpha = slotBG.color.a;
            slotBG.color = new Color(1, 1, 1, alpha);

        }

        private void OnInventoryUpdate(OnClientInventoryUpdate e) {
            Debug.Log($"Inventory size: {e.AllSlots.Count}");
            for (int i = 0; i < e.AllSlots.Count; i++) {
                    Image itemImage = allItemSlots[i].transform.Find("ItemImage").GetComponent<Image>();
                    TMP_Text itemText = allItemSlots[i].transform.Find("ItemCount").GetComponent<TMP_Text>();
                    BackpackSlot slot = e.AllSlots[i];
                   
                    if (slot.ClientSlotCount > 0) {
                        itemImage.color = new Color(1, 1, 1, 1);
                        Texture2D texture = resLoader.LoadSync<Texture2D>("assets/goods", slot.SpriteName);
                        Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                        itemImage.sprite = sprite;
                        itemText.text = slot.ClientSlotCount.ToString();
                    }
                    else {
                        itemImage.color = new Color(1, 1, 1, 0);
                        itemText.text = "";
                    }

                    Image slotBG = allItemSlots[i].GetComponent<Image>();
                    float alpha = slotBG.color.a;

                    if (i == e.SelectedIndex) {
                        slotBG.color = new Color(1, 1, 1, alpha);
                    }
                    else {
                        slotBG.color = new Color(0, 0, 0, alpha);
                    }
                }
                
            
        }
    }
}

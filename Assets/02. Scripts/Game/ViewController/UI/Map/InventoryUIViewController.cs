using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Mikrocosmos
{
    public static class RectTransformExtensions
    {
        public static void SetLeft(this RectTransform rt, float left) {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void SetOffset(this RectTransform rt, float left, float right, float top, float bottom) {
            rt.SetLeft(left);
            rt.SetRight(right);
            rt.SetTop(top);
            rt.SetBottom(bottom);
        }


    }
    
    public class InventoryUIViewController : AbstractMikroController<Mikrocosmos>
    {
        [SerializeField]
        private GameObject slotPrefab;

        private Transform initialBackPackBG;

        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Sprite selectedSprite;

        [SerializeField] private float selectedSlotLocalY = -32;
        private float selectedSlotLocalYStart = -60;
        private List<GameObject> allItemSlots = new List<GameObject>();

        private ResLoader resLoader;
        private void Awake()
        {
            this.RegisterEvent<OnClientInventoryUpdate>(OnInventoryUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<OnStartInventoryInit>(OnInventoryInit).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnGoodsUpdateViewControllerDurability>(OnGoodsUpdateDurability).UnRegisterWhenGameObjectDestroyed(gameObject);
            initialBackPackBG = transform.Find("InitialBackPackBG");

            ResLoader.Create((loader => resLoader = loader));
        }

        private void OnGoodsUpdateDurability(OnGoodsUpdateViewControllerDurability e) {
            Image itemImage = allItemSlots[e.SlotNumber].transform.Find("ItemSlot/ItemDurabilityImage").GetComponent<Image>();
            if (e.DurabilitySprite == null && !e.UsePreviousSprite) {
                itemImage.color = new Color(1, 1, 1, 0f);
            }
            else {
                itemImage.color = new Color(1, 1, 1, 1f);
                if (!e.UsePreviousSprite) {
                    itemImage.sprite = e.DurabilitySprite;
                }
               
                DOTween.To(() => itemImage.fillAmount, x => itemImage.fillAmount = x, e.DurabilityFraction, 0.2f);
                Debug.Log($"Durability Fraction: {e.DurabilityFraction}");
            }
        }

        private void OnInventoryInit(OnStartInventoryInit e)
        {

            for (int i = 0; i < e.InitialBackPackCapacity; i++)
            {
                GameObject slot = Instantiate(slotPrefab, initialBackPackBG);
                slot.transform.SetAsLastSibling();
                allItemSlots.Add(slot);
                slot.transform.Find("ItemSlot/ItemShortcutID").GetComponent<TMP_Text>().text = (i+1).ToString();
                selectedSlotLocalYStart =
                    slot.transform.Find("ItemSlot").GetComponent<RectTransform>().anchoredPosition.y;
               

            }


            Image slotBG = allItemSlots[0].transform.Find("ItemSlot").GetComponent<Image>();
            float alpha = slotBG.color.a;
            slotBG.color = new Color(1, 1, 1, alpha);

        }

        private void OnInventoryUpdate(OnClientInventoryUpdate e)
        {
            Debug.Log($"Inventory size: {e.AllSlots.Count}");
            for (int i = 0; i < e.AllSlots.Count; i++)
            {
                Image itemImage = allItemSlots[i].transform.Find("ItemSlot/ItemImage").GetComponent<Image>();
                TMP_Text itemText = allItemSlots[i].transform.Find("ItemSlot/ItemCount").GetComponent<TMP_Text>();
                Image itemDurabilityImage = allItemSlots[i].transform.Find("ItemSlot/ItemDurabilityImage").GetComponent<Image>();
                BackpackSlot slot = e.AllSlots[i];
                
                if (slot.ClientSlotCount > 0) {
                   
                    itemDurabilityImage.enabled = true;
                    itemImage.color = new Color(1, 1, 1, 1);
                    Texture2D texture = resLoader.LoadSync<Texture2D>("assets/goods", slot.SpriteName);
                    Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    itemImage.sprite = sprite;
                    itemText.text = slot.ClientSlotCount.ToString();
                }
                else
                {
                    itemImage.color = new Color(1, 1, 1, 0);
                    itemText.text = "";
                    itemDurabilityImage.enabled = false;
                  
                }

                Image slotBG = allItemSlots[i].transform.Find("ItemSlot").GetComponent<Image>();
                RectTransform itemSlotTr = allItemSlots[i].transform.Find("ItemSlot").GetComponent<RectTransform>();
                GameObject selectedImage = itemSlotTr.transform.Find("SelectedImage").gameObject;
                RectTransform durabilityImage = itemSlotTr.transform.Find("ItemDurabilityImage").GetComponent<RectTransform>();
                if (i == e.SelectedIndex) {
                    slotBG.sprite = selectedSprite;
                  
                    itemSlotTr.DOAnchorPosY(selectedSlotLocalY, 0.3f);
                    selectedImage.SetActive(true);
                    durabilityImage.SetOffset(5.683799f, 5.494201f, 5.035349f, 8.26095f);
                    //itemSlotTr.DOLocalMoveY(selectedSlotLocalY, 0.3f);
                }
                else {
                    slotBG.sprite = unselectedSprite;
               
                    if (Math.Abs(itemSlotTr.localPosition.y - selectedSlotLocalYStart) > 0.1f) {
                        itemSlotTr.DOAnchorPosY(selectedSlotLocalYStart, 0.3f);
                        selectedImage.SetActive(false);
                        durabilityImage.SetOffset(0.3413018f, 0.2838982f, 0.3391983f, 0.3136017f);
                        //itemSlotTr.DOLocalMoveY(selectedSlotLocalYStart, 0.3f);
                    }
                   
                }
            }

        }
    }
}
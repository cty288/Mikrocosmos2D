using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class AvatarElementTypeLayout : AbstractMikroController<Mikrocosmos>, ICanSendEvent {
        [SerializeField] private Vector2Int elementIndexRange;
        [SerializeField] private float elementLayoutPageWidth = 1260;
        [SerializeField] private GameObject avatarSelectionElementPrefab;
        [SerializeField] private int elementLayer;
        public Vector2Int ElementIndexRange => elementIndexRange;

        private List<AvatarSingleElement> allElements = new List<AvatarSingleElement>();

        private Transform elementLayout;
        private Button nextPageButton;
        private Button lastPageButton;

        private ResLoader resLoader;
        private int selectedIndex = -1;

        [SerializeField] private int itemPerPage = 6;
        private void Awake() {
            elementLayout = transform.Find("Mask/ElementLayout");
            nextPageButton = transform.Find("NextPageButton").GetComponent<Button>();
            lastPageButton = transform.Find("LastPageButton").GetComponent<Button>();
            ResLoader.Create((loader => resLoader = loader));
            
        }

        public void FillElements(Action onFinished) {
            if (allElements.Count > 0) {
                return;
            }
            StartCoroutine(FillElementsUntilResLoaderReady(onFinished));
            
        }


        public void SelectElement(int index) {
            if (index >= elementIndexRange.y || index < elementIndexRange.x) {
                return;
            }

            this.SendEvent<OnAvatarSingleElementSelected>(new OnAvatarSingleElementSelected() {
                Index = index,
                ReplacedIndex = selectedIndex,
                IsBase = elementIndexRange.y <= 100,
                ElementLayer = elementLayer
            });

            allElements[index - elementIndexRange.x].SetSelection(true);
            if (selectedIndex >= elementIndexRange.x && selectedIndex < elementIndexRange.y) {
                allElements[selectedIndex - elementIndexRange.x].SetSelection(false);
            }
            selectedIndex = index;
        }


        private IEnumerator FillElementsUntilResLoaderReady(Action onFinished) {
            while (true) {
                yield return null;
                if (resLoader.IsReady) {
                    break;
                }
            }
            for (int i = elementIndexRange.x; i < elementIndexRange.y; i++) {
                Texture2D texture = resLoader.LoadSync<Texture2D>("profile", $"profile{i}");
                if (texture)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    GameObject selectionPrefab = Instantiate(avatarSelectionElementPrefab, elementLayout);
                    AvatarSingleElement element = selectionPrefab.GetComponent<AvatarSingleElement>();
                    element.SetElement(i, sprite); 
                    allElements.Add(element);
                }
            }
            onFinished?.Invoke();
        }
    }
}

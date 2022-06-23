/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using UnityEngine;
using UnityEngine.UI;



public class Window_Pointer : MonoBehaviour {


   
    [SerializeField] private Sprite crossSprite;
    private IMapPointerViewController pointerViewController;

    [SerializeField]
    public Transform target;
    private Vector3 targetDir;

    [SerializeField]
    private RectTransform pointerRectTransform;
    [SerializeField]
    private Image pointerImage;
    public LayerMask borderMask;
    [SerializeField] private bool isOffScreen;

    [SerializeField] private List<GameObject> hiddenObjectsWhenNoCrossSprite;
    private void Awake() {
    
        pointerViewController = GetComponent<IMapPointerViewController>();
        Show();
        
    }

    private void Start() {
        Update();
    }

    private void Update() {
        if (target) {
            Vector2 center = new Vector2(Screen.width, Screen.height);

            float borderSize = 100f;

            Vector3 targetPositionScreenPoint = Camera.main.WorldToScreenPoint(target.position);
            isOffScreen = targetPositionScreenPoint.x <= borderSize || targetPositionScreenPoint.x >= Screen.width - borderSize || targetPositionScreenPoint.y <= borderSize || targetPositionScreenPoint.y >= Screen.height - borderSize;

            if (isOffScreen)
            {
                foreach (GameObject o in hiddenObjectsWhenNoCrossSprite) {
                    o.SetActive(true);
                }
               // pointerRectTransform.gameObject.SetActive(true);
                RotatePointerTowardsTargetPosition();

                pointerImage.sprite = pointerViewController.PointerSprite;


                RaycastHit2D hit = Physics2D.Raycast(Camera.main.transform.position, targetDir, Mathf.Infinity, borderMask);
                if (hit.collider != null)
                {
                    pointerRectTransform.position = hit.point;
                    pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x, pointerRectTransform.localPosition.y, 0f);
                }




            }
            else
            {
                if (crossSprite) {
                    foreach (GameObject o in hiddenObjectsWhenNoCrossSprite)
                    {
                        o.SetActive(true);
                    }
                    pointerImage.sprite = crossSprite;
                    Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(targetPositionScreenPoint);
                    pointerRectTransform.position = pointerWorldPosition;
                    pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x, pointerRectTransform.localPosition.y, 0f);

                    pointerRectTransform.localEulerAngles = Vector3.zero;
                }
                else {
                    foreach (GameObject o in hiddenObjectsWhenNoCrossSprite)
                    {
                        o.SetActive(false);
                    }
                }
            }
        }
       
    }

    private void RotatePointerTowardsTargetPosition() {
        Vector3 toPosition = target.position;
        Vector3 fromPosition = Camera.main.transform.position;
        fromPosition.z = 0f;
        targetDir = (toPosition - fromPosition).normalized;
       // float angle = GetAngleFromVectorFloat(targetDir);
       // pointerRectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }
    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }
    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }
}

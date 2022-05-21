/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class Window_Pointer : MonoBehaviour {

   
    [SerializeField] private Sprite arrowSprite;
    [SerializeField] private Sprite crossSprite;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private RectTransform pointerRectTransform;
    private Image pointerImage;

    private void Awake() {
        pointerImage = pointerRectTransform.GetComponent<Image>();

        Show();
    }

    private void Update() {
        float borderSize = 100f;
        Vector3 targetPositionScreenPoint = Camera.main.WorldToScreenPoint(target.position);
        bool isOffScreen = targetPositionScreenPoint.x <= borderSize || targetPositionScreenPoint.x >= Screen.width - borderSize || targetPositionScreenPoint.y <= borderSize || targetPositionScreenPoint.y >= Screen.height - borderSize;

        if (isOffScreen) {
            RotatePointerTowardsTargetPosition();

            pointerImage.sprite = arrowSprite;
            Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
           
            //Vector3 cappedTargetScreenPosition = Camera.main.WorldToScreenPoint(Camera.main.transform.position)+  targetPositionScreenPoint.normalized * 500;
            if (cappedTargetScreenPosition.x <= borderSize) {

                cappedTargetScreenPosition = new Vector3(borderSize,
                    targetPositionScreenPoint.y * (-(Screen.width / 2) / targetPositionScreenPoint.x),-10);

                Debug.Log($"Target: {targetPositionScreenPoint.y * (-(Screen.width / 2) / targetPositionScreenPoint.x)}");
            }
            /*
            if (cappedTargetScreenPosition.y <= borderSize)
            {
                if (cappedTargetScreenPosition.x == targetPositionScreenPoint.x) {
                    cappedTargetScreenPosition.x *= (Screen.width / 2) / targetPositionScreenPoint.x;
                }
               
                if (cappedTargetScreenPosition.y == targetPositionScreenPoint.y) {
                    cappedTargetScreenPosition.y = borderSize;
                }
                // Debug.Log((Screen.width / 2) / targetPositionScreenPoint.x);
            }

            if (cappedTargetScreenPosition.x >= Screen.width - borderSize)
            {
                if (cappedTargetScreenPosition.y == targetPositionScreenPoint.y)
                {
                    cappedTargetScreenPosition.y *= (Screen.height / 2) / targetPositionScreenPoint.y;
                }
               

                if (cappedTargetScreenPosition.x == targetPositionScreenPoint.x)
                {
                    cappedTargetScreenPosition.x = Screen.width - borderSize;
                }

                
                //   Debug.Log((Screen.height / 2) / targetPositionScreenPoint.y);
            }

            if (cappedTargetScreenPosition.y >= Screen.height - borderSize)
            {
                if (cappedTargetScreenPosition.x == targetPositionScreenPoint.x)
                {
                    cappedTargetScreenPosition.x *= (Screen.width / 2) / targetPositionScreenPoint.x;
                }

               
                if (cappedTargetScreenPosition.y == targetPositionScreenPoint.y)
                {
                    cappedTargetScreenPosition.y = Screen.height - borderSize;
                }
                
                // Debug.Log((Screen.width / 2) / targetPositionScreenPoint.x);
            }*/










          //  Debug.Log(cappedTargetScreenPosition);

            Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(cappedTargetScreenPosition);
            pointerRectTransform.position = pointerWorldPosition;
            pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x, pointerRectTransform.localPosition.y, 0f);
        } else {
            pointerImage.sprite = crossSprite;
            Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(targetPositionScreenPoint);
            pointerRectTransform.position = pointerWorldPosition;
            pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x, pointerRectTransform.localPosition.y, 0f);

            pointerRectTransform.localEulerAngles = Vector3.zero;
        }
    }

    private void RotatePointerTowardsTargetPosition() {
        Vector3 toPosition = target.position;
        Vector3 fromPosition = Camera.main.transform.position;
        fromPosition.z = 0f;
        Vector3 dir = (toPosition - fromPosition).normalized;
        float angle = GetAngleFromVectorFloat(dir);
        pointerRectTransform.localEulerAngles = new Vector3(0, 0, angle);
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

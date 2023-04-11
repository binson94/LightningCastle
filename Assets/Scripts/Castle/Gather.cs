using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//집결
public class Gather : MonoBehaviour {

    Vector3 touchPos;
    Touch nowTouch;
    public Canvas parentCanvas;

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            Vector3 touchPosVector = Input.GetTouch(0).position;
            touchPos = Camera.main.ScreenToWorldPoint(touchPosVector);

            nowTouch = Input.GetTouch(0);

            Vector2 tmpPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, nowTouch.position, parentCanvas.worldCamera, out tmpPos);

            if ((tmpPos.y < -325) && (tmpPos.x < -470))
                return;
            else if ((tmpPos.x > 535) && (tmpPos.y < -55))
                return;

        }
    }
}

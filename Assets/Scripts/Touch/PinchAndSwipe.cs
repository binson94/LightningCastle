using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchAndSwipe : MonoBehaviour {

    //Pinch 관련
    public float ZoomSpeed = 0.01f;
    public Touch[] touchs = new Touch[2];
    public Vector2[] touchPrevPos = new Vector2[2];
    public bool CameraHold;

    public Canvas parentCanvas;
    public Camera UICamera;
    public Camera CharCamera;
    public Camera FireCamera;
    public Camera camera;

    //Swipe 관련
    private float SwipeSpeed = 0.2f;
    public float DeltaXLimit;
    public float DeltaYLimit;
    Vector3 TouchPrevPos;
    Touch nowTouch;

    private void Start()
    {
        DeltaXLimit = 0;
        DeltaYLimit = 0;
    }

    private void Update()
    {
        if (Time.timeScale != 0)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount == 1)
                {
                    if (!CameraHold)
                    {
                        nowTouch = Input.GetTouch(0);

                        Vector2 tmpPos;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, nowTouch.position, parentCanvas.worldCamera, out tmpPos);

                        if ((tmpPos.y < -325) && (tmpPos.x < -470))
                            return;
                        else if ((tmpPos.x > 535) && (tmpPos.y < -55))
                            return;

                        if (Mathf.Abs(nowTouch.deltaPosition.x) + Mathf.Abs(nowTouch.deltaPosition.y) < 0.01f)
                            return;

                        TouchPrevPos = -nowTouch.deltaPosition * 0.04f;

                        UICamera.transform.position += TouchPrevPos * SwipeSpeed;

                        CameraPosSet();
                    }
                }
                else if (Input.touchCount == 2)
                {
                    touchs[0] = Input.GetTouch(0);
                    touchs[1] = Input.GetTouch(1);

                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 tmpPos;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, touchs[0].position, parentCanvas.worldCamera, out tmpPos);

                        if ((tmpPos.y < -325) && (tmpPos.x < -470))
                            return;
                        else if ((tmpPos.x > 535) && (tmpPos.y < -55))
                            return;
                    }


                    touchPrevPos[0] = touchs[0].position - touchs[0].deltaPosition;
                    touchPrevPos[1] = touchs[1].position - touchs[1].deltaPosition;

                    float prevTouchDeltaMag = (touchPrevPos[0] - touchPrevPos[1]).magnitude;
                    float touchDeltaMag = (touchs[0].position - touchs[1].position).magnitude;

                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    camera.orthographicSize += deltaMagnitudeDiff * ZoomSpeed;

                    camera.orthographicSize = Mathf.Max(camera.orthographicSize, 2.534f);
                    camera.orthographicSize = Mathf.Min(camera.orthographicSize, 6.335f);
                    UICamera.orthographicSize = Mathf.Max(camera.orthographicSize, 2.534f);
                    UICamera.orthographicSize = Mathf.Min(camera.orthographicSize, 6.335f);
                    CharCamera.orthographicSize = Mathf.Max(camera.orthographicSize, 2.534f);
                    CharCamera.orthographicSize = Mathf.Min(camera.orthographicSize, 6.335f);
                    FireCamera.orthographicSize = Mathf.Max(camera.orthographicSize, 2.534f);
                    FireCamera.orthographicSize = Mathf.Min(camera.orthographicSize, 6.335f);

                    CameraPosSet();
                }
            }
        }
    }

    public void CameraPos(Transform CharPos)
    {
        UICamera.transform.position = CharPos.position + new Vector3(0, 0, -10);
        CameraPosSet();
    }

    private void CameraPosSet()
    {
        DeltaXLimit = 6.75f * (6.335f - camera.orthographicSize) / 3.801f;
        DeltaYLimit = 3.78f * (6.335f - camera.orthographicSize) / 3.801f;

        //x 좌표 관련
        //camera.transform.position = new Vector3(Mathf.Min(DeltaXLimit, camera.transform.position.x), camera.transform.position.y, camera.transform.position.z);
        //camera.transform.position = new Vector3(Mathf.Max(-DeltaXLimit, camera.transform.position.x), camera.transform.position.y, camera.transform.position.z);

        UICamera.transform.position = new Vector3(Mathf.Min(DeltaXLimit, UICamera.transform.position.x), UICamera.transform.position.y, UICamera.transform.position.z);
        UICamera.transform.position = new Vector3(Mathf.Max(-DeltaXLimit, UICamera.transform.position.x), UICamera.transform.position.y, UICamera.transform.position.z);

        //y 좌표 관련
        //camera.transform.position = new Vector3(camera.transform.position.x, Mathf.Min(DeltaYLimit, camera.transform.position.y), camera.transform.position.z);
        //camera.transform.position = new Vector3(camera.transform.position.x, Mathf.Max(-DeltaYLimit, camera.transform.position.y), camera.transform.position.z);

        UICamera.transform.position = new Vector3(UICamera.transform.position.x, Mathf.Min(DeltaYLimit, UICamera.transform.position.y), UICamera.transform.position.z);
        UICamera.transform.position = new Vector3(UICamera.transform.position.x, Mathf.Max(-DeltaYLimit, UICamera.transform.position.y), UICamera.transform.position.z);
    }
}

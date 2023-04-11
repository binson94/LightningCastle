using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SwipeReceiver : MonoBehaviour {

	protected virtual void OnSwipeLeft()
    {
        Debug.Log("Left");
    }

    protected virtual void OnSwipeRight()
    {
        Debug.Log("Right");
    }

    protected virtual void Update()
    {
        if(SwipeManager.Instance.IsSwiping(SwipeManager.SwipeDirection.Right))
        {
            OnSwipeRight();
        }
        if(SwipeManager.Instance.IsSwiping(SwipeManager.SwipeDirection.Left))
        {
            OnSwipeLeft();
        }
    }
}

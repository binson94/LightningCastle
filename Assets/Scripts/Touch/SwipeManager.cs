using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SwipeManager : MonoBehaviour {

    [Flags]
	public enum SwipeDirection
    {
        None = 0, Left = 1, Right=2
    }

    private static SwipeManager m_instance;

    public static SwipeManager Instance
    {
        get
        {
            if(!m_instance)
            {
                m_instance = new GameObject("SwipeManager").AddComponent<SwipeManager>();
            }

            return m_instance;
        }
    }

    public SwipeDirection Direction
    {
        get;
        private set;
    }

    private Vector3 m_touchPosition;
    private float m_swipeResistanceX = 300.0f;

    private void Start()
    {
        if (m_instance != this)
        {
            Debug.LogError("Dont instantiate SwipeManager manually");
            DestroyImmediate(this);
        }
    }

    private void Update()
    {
        Direction = SwipeDirection.None;

        if(Input.GetMouseButtonDown(0))
        {
            m_touchPosition = Input.mousePosition;
        }

        if(Input.GetMouseButtonUp(0))
        {
            Vector2 deltaSwipe = m_touchPosition - Input.mousePosition;

            if (Mathf.Abs(deltaSwipe.x) > m_swipeResistanceX)
            {
                Direction |= (deltaSwipe.x < 0) ? SwipeDirection.Right : SwipeDirection.Left;
            }
        }
    }

    public bool IsSwiping(SwipeDirection dir)
    {
        return (Direction & dir) == dir;
    }
}

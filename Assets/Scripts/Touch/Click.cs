using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Click : MonoBehaviour {
    
    private GameObject target;
    private Character targetC;
    Coroutine ClickCoroutine;

    public float ClickTime;
    public bool isClicking;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            CastRay();
            if ((target != null) && target.gameObject.tag == "Color")
            {
                ClickTime = 0;
                ClickCoroutine = StartCoroutine(ClickCheck());
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if(isClicking)
            {
                StopCoroutine(ClickCoroutine);
                isClicking = false;
            }

            if ((target != null) && (ClickTime > 1))
            {
                Debug.Log("hit");
                targetC = target.GetComponent<Character>();
                if (!targetC.isControl)
                    targetC.isHold = !targetC.isHold;
            }
        }
	}

    IEnumerator ClickCheck()
    {
        if (!isClicking)
        {
            isClicking = true;
            while (true)
            {
                CastRay();
                if ((target != null) && target.gameObject.tag == "Color")
                {
                    ClickTime += 0.03f;
                    yield return new WaitForSeconds(0.03f);
                }
                else
                {
                    yield break;
                }
            }
        }
    }

    private void CastRay()
    {
        target = null;
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0.1f);
        if (hit.collider != null)
        {
            target = hit.collider.gameObject;
        }
    }
}
*/
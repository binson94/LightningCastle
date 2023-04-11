using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangImage : MonoBehaviour {

	public void ImageStart()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(Spin());
    }
	
	IEnumerator Spin()
    {
        float i = 0;
        while(true)
        {
            transform.rotation = Quaternion.Euler(0, 0, -i);
            i += 8;
            yield return new WaitForSeconds(0.02f);
        }
    }
}

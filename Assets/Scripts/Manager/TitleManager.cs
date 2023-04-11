using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour {

    bool mainStart = false;
    public Text Title;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Stage"))
            PlayerPrefs.SetInt("Stage", 1);
    }

    // Update is called once per frame
    void Update () {
		if(Input.GetMouseButtonDown(0))
        {
            if (!mainStart)
            {
                mainStart = true;
                StartCoroutine(TitleFade());
            }
        }
	}

    IEnumerator TitleFade()
    {
        Debug.Log("Fade");
        for (float i = 1; i > 0; i -= 0.01f)
        {
            Title.color = new Color(0, 0, 0, i);
            yield return new WaitForSeconds(0.01f);
        }

        SceneManager.LoadScene(1);
    }
}

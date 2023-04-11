using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour {

    public Button Pause;
    public Button Resume;

    public GameObject PauseUI;

    public bool isPause;

	// Use this for initialization
	void Start () {
        isPause = false;
        PauseUI.SetActive(false);
        Resume.gameObject.SetActive(false);
	}

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                PauseBtn();
            }
    }

    public void PauseBtn()
    {
        isPause = true;
        Time.timeScale = 0;
        Pause.gameObject.SetActive(false);
        Resume.gameObject.SetActive(true);
        PauseUI.SetActive(true);
    }

    public void ResumeBtn()
    {
        isPause = false;
        Time.timeScale = 1;
        Pause.gameObject.SetActive(true);
        Resume.gameObject.SetActive(false);
        PauseUI.SetActive(false);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        SaveManager.instance.SaveAll();

        Application.Quit();
    }
}

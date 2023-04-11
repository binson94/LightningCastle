using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum CanvasIdx
{
    Main = 0, Refine = 1, Organize = 2, Trader = 3, Plant = 4, Smith = 5
}

public class DayManager : MonoBehaviour {

    //표시 관련
    //********************************
    public Text nowDayText;                 //현재 일 수를 알려주는 텍스트
    public SpriteRenderer dayBG;            //밤으로 갈 때 페이드 아웃
    public SpriteRenderer nightBG;          //밤으로 갈 때 페이드 인
    public SpriteRenderer Fireplace;        //밤으로 갈 때 페이드 아웃
    //********************************

    public List<GameObject> CanvasList;       //메인에 있는 모든 캔버스를 저장하는 리스트, idx는 CanvasIdx 나열형을 따름

    public GameObject PauseBtn;           //일시정지 버튼
    public GameObject ResumeBtn;          //계속 버튼
    public GameObject PauseUI;            //일시정지 시 뜨는 UI

    public Transform[] CharPos;           //각 캐릭 별로 위치

    public TraderManager traderM;

    string[] charNameString = new string[6];
    string[] charColorString = new string[8];

    private void Start()
    {
        //메인 캔버스만 활성화, 일시정지 해제
        Btn_Pause(false);

        SaveManager.instance.Load();
        SaveManager.instance.OrganizeTextSet();

        OrganizeManager.instance.ImageResize();

        for (int i = 0; i < 3; i++)
        {
            if (!GameManager.instance.AssignList[i])
                continue;

            GameManager.instance.AssignList[i].gameObject.SetActive(false);
        }

        //로고 fade in, fade out
        StartCoroutine(LogoFade());

        Debug.Log(string.Concat("Time : ", SaveManager.instance.rewardsave.Time.ToString(), "Kill : ", SaveManager.instance.rewardsave.EnemyKill.ToString()));
    }

    //일시정지 뒤로가기 버튼 대응
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Btn_Pause(true);
        }
    }

    //day text 보이기
    IEnumerator LogoFade()
    {
        nowDayText.text = string.Concat("Day ", (PlayerPrefs.GetInt("Stage")).ToString());

        float i;
        Color tmp = nowDayText.color;
        for (i = 0; i < 1; i += 0.01f)
        {
            tmp.a = i;
            nowDayText.color = tmp;
            yield return new WaitForSeconds(0.01f);
        }

        for (i = 1; i > 0; i -= 0.01f)
        {
            tmp.a = i;
            nowDayText.color = tmp;
            yield return new WaitForSeconds(0.01f);
        }
    }

    //idx에 따라 각각 UI를 띄워주는 함수, 각각 버튼에 idx 값이 할당되어 있음
    //0 : close(main), 1 : refine, 2 : organize, 3 : trader, 4 : plant, 5 : Equip
    public void Btn_Facility(int idx)
    {
        traderM.Btn_SelectChar(-1);

        foreach (GameObject g in CanvasList)
            g.SetActive(false);

        CanvasList[idx].SetActive(true);
    }
    
    //일시정지 버튼
    public void Btn_Pause(bool pause)
    {
        //다른 캔버스 모두 비활성화
        foreach (GameObject g in CanvasList)
            g.SetActive(false);

        //일시정지 해제 시에는 main canvas만 활성화
        CanvasList[(int)CanvasIdx.Main].SetActive(!pause);

        PauseBtn.SetActive(!pause);
        ResumeBtn.SetActive(pause);
        PauseUI.SetActive(pause);

        Time.timeScale = pause ? 0 : 1;
    }

    //종료 버튼
    public void Btn_Quit()
    {
        SaveManager.instance.SaveAll();

        Application.Quit();
    }

    //일몰 버튼 누르면 호출
    public void Btn_GoToNight()
    {
        //모든 캔버스 비활성화
        foreach (GameObject g in CanvasList)
            g.SetActive(false);

        Btn_Pause(false);

        StartCoroutine(Fade());
    }
    
    IEnumerator Fade()
    {
        for (float i = 0; i < 1; i += 0.01f)
        {
            dayBG.color = new Color(255, 255, 255, 1 - i);
            Fireplace.color = new Color(255, 255, 255, 1 - i);
            nightBG.color = new Color(255, 255, 255, i);
            yield return new WaitForSeconds(0.01f);
        }
        
        for (int i = 0; i < 3; i++)
        {
            if (!GameManager.instance.AssignList[i])
                continue;

            GameManager.instance.AssignList[i].transform.position = CharPos[i].position;
            GameManager.instance.AssignList[i].gameObject.SetActive(true);
        }

        SceneManager.LoadScene(2);
    }
}

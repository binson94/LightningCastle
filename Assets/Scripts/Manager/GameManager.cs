using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    1. 캐릭터 데이터와 편성 창 데이터 저장
    2. TraderManager에서 AddChar 호출 시, RoomList에 캐릭터 추가
    3. GameOver 시 모든 상황 멈추는 거 담당
 */
public class GameManager : MonoBehaviour {

    //외부 접근을 위한 요소
    static public GameManager instance = null;
    public NightManager NightM;

    //캐릭터 배정 관련
    //**********************************
    public Character[] RoomList = new Character[10];      //숙소에 존재하는 용병들
    public Character[] AssignList = new Character[3];     //전투 배정 캐릭터
    public int[] AssignCharIndex = new int[10];           //Assign하면 Assign한 캐릭터의 Index에 해당하는 곳에 Assign한 위치 저장
    //**********************************

    //현재 스테이지는 PlayerPrefs "Stage"에 저장

    //GameManager는 하나만 존재, AssignCharIndex 모든 원소 -1로 초기화
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);

            for (int i = 0; i < 10; i++)
                AssignCharIndex[i] = -1;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    //NightManager와 JoystickManager 정보 불러오기, NightManager의 Start 함수에서 호출
    public void ManagerUpdate()
    {
        //각각을 처리하는 스크립트를 가진 게임오브젝트는 각각의 태그를 가지고 있음
        NightM = GameObject.FindWithTag("NightManager").GetComponent<NightManager>();
    }

    //RoomList에 빈 자리가 있는 지 반환, TraderManager에서 호출해서 사용
    public bool HaveSeat()
    {
        foreach (Character tmp in RoomList)
        {
            if (!tmp)
                return true;
        }

        return false;
    }

    //구매한 캐릭터 RoomList에 추가, TraderManager에서 호출해서 사용
    public void AddChar(Character C)
    {
        for (int i = 0; i < 10; i++)
        {
            if (!RoomList[i])
            {
                RoomList[i] = C;
                DontDestroyOnLoad(RoomList[i].gameObject);
                break;
            }
        }

        SaveManager.instance.SaveCharData();
        OrganizeManager.instance.ImageResize();
    }

    //게임 오버 시 호출, NightManager의 EnemyInvasion 함수, JoystickManager의 CharChangeMain 함수에서 호출
    public void GameOver()
    {
        Time.timeScale = 0;
        NightM.GameOverCanvas.SetActive(true);
    }
}

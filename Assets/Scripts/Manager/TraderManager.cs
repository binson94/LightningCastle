using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    1. 용병마차에서 판매할 캐릭터 생성
    2. 캐릭터 구매 시 GameManager의 AddChar 호출, 구매한 캐릭터는 perchased[i] = true로 함
  */
public class TraderManager : MonoBehaviour
{ 
    float levelCorrection;                   //스테이지에 따른 레벨 보정값, +- 0.4해서 반올림함
    
    public GameObject[] characterPrefabs;           //캐릭터 프리팹

    Character[] perchaseList = new Character[4];    //4개의 인스턴스 생성
    bool[] perchased = new bool[4];                 //이미 구매되어있는지 채크

    public Text[] CharText;                         //구매 버튼에서 캐릭터 정보 표시
    public Button[] charButton;                     //선택 시 색 변경용
    public int index;                               //현재 선택된 캐릭터 index, 초기값 -1

    ColorBlock normalBtnColor;
    ColorBlock selectBtnColor;

    string[] charNameString = new string[6];
    string[] charColorString = new string[8];
    
    void Start()
    {
        StringInit();

        selectBtnColor = normalBtnColor = charButton[0].colors;
        selectBtnColor.normalColor = selectBtnColor.highlightedColor = selectBtnColor.pressedColor = Color.yellow;

        TradeReset();
    }

    void StringInit()
    {
        charNameString[0] = "Hero";
        charNameString[1] = "Chaser";
        charNameString[2] = "Crusher";
        charNameString[3] = "Guardian";
        charNameString[4] = "Alchemist";
        charNameString[5] = "Astronomer";

        charColorString[0] = "None";
        charColorString[1] = "Red";
        charColorString[2] = "Orange";
        charColorString[3] = "Yellow";
        charColorString[4] = "Green";
        charColorString[5] = "Blue";
        charColorString[6] = "Navy";
        charColorString[7] = "Purple";
    }

    //상점 창을 초기화하는 함수
    public void TradeReset()
    {
        //선택 초기화
        Btn_SelectChar(-1);

        //스테이지에 따른 보정 수치 로드
        switch (PlayerPrefs.GetInt("Stage"))
        {
            case 0:
                levelCorrection = 1;
                break;
            case 1:
                levelCorrection = 1.3f;
                break;
            case 2:
                levelCorrection = 2.8f;
                break;
            default:
                levelCorrection = 4;
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            perchased[i] = false;
            CharInitiate(i);
        }
    }
    
    //낮 시작 시 상점에서 판매하는 캐릭터 생성
    void CharInitiate(int index)
    {
        //레벨 보정 수치에 따라 레벨 설정, 클래스는 무작위
        CharClass tmpClass;
        ENUM.Color tmpColor;

        int level = Mathf.RoundToInt(levelCorrection + Random.Range(-0.4f, 0.4f));

        tmpClass = (CharClass)Random.Range(1, 7);
        tmpColor = (ENUM.Color)(Random.Range(0, 8) * 5);

        CharText[index].text = string.Concat("level ", level, "\n", charNameString[(int)tmpClass - 1]);

        CharText[index].text = string.Concat(CharText[index].text,"\n", charColorString[(int)tmpColor / 5]);

        //새로운 오브젝트 생성
        perchaseList[index] = Instantiate(characterPrefabs[(int)tmpClass - 1]).GetComponent<Character>();

        perchaseList[index].level = level;
        perchaseList[index].charColor = tmpColor;
        perchaseList[index].AnimationColorSet();

        perchaseList[index].gameObject.SetActive(false);
    }

    //구매할 캐릭터 선택(선택 취소 : -1, 그외는 0 ~ 3 할당)
    public void Btn_SelectChar(int idx)
    {
        //선택한 버튼 색깔 변경
        for (int i = 0; i < 4; i++)
            if (i == idx && !perchased[i])
                charButton[i].colors = selectBtnColor;
            else
                charButton[i].colors = normalBtnColor;

        index = idx;
    }

    //캐릭터 구매 버튼
    public void Btn_Perchase()
    {
        if (index < 0)
            return;

        if(!perchased[index])
        {
            if(GameManager.instance.HaveSeat())
            {
                perchased[index] = true;
                GameManager.instance.AddChar(perchaseList[index]);

                CharText[index].text = "Perchased";
                Btn_SelectChar(-1);
            }
        }
    }
}

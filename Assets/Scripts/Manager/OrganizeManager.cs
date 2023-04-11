using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrganizeManager : MonoBehaviour
{
    public static OrganizeManager instance = null;

    public int Selected = -1;                             //어떤 자리가 선택되어있는가, 0, 1, 2, 아무것도 선택 안됐으면 -1
    
    public GameObject CharSelectUI;                       //자리가 선택되었을 때만 보이는 UI, 캐릭터 할당을 위한 UI
    public RectTransform CharSelectPanel;                 //캐릭터를 선택하는 UI의 크기 변경을 위해
    public GameObject[] CharSelectButtons;                //GameManager의 RoomList 캐릭터 숫자만큼 활성화
    public Text[] CharSelectText;                         //편성 된 캐릭터 정보 표시 텍스트
    
    string[] charNameString = new string[6];
    string[] charColorString = new string[8];

    private void Awake()
    {
        instance = this;
        StringInit();
    }

    //RoomList의 캐릭터 수만큼 버튼 활성화, Panel 사이즈 조정 - DayManager의 Start 함수, GameManager의 AddChar 함수에서 호출
    //EquipButtons 보이기 설정
    public void ImageResize()
    {
        int i;
        for (i = 0; i < 10; i++)
        {
            if (!GameManager.instance.RoomList[i])
                break;

            CharSelectButtons[i].SetActive(true);
        }

        int size = (i + 1) * 300 + 100;     //+1은 cancel 버튼을 위해

        for (; i < 10; i++)
            CharSelectButtons[i].SetActive(false);
        

        CharSelectPanel.sizeDelta = new Vector2(size, 200);
    }
    
    //어떤 자리에 캐릭터를 선택할 지 결정하는 함수. 자리 별로 0, 1, 2로 정해져 있음
    public void Btn_AssignSelect(int idx)
    {
        CharSelectUI.SetActive(true);
        Selected = idx;
    }

    //자리 선택 취소
    public void Btn_AssignCencel()
    {
        Selected = -1;
        CharSelectUI.SetActive(false);
    }

    //현재 선택된 자리에 할당되어있는 캐릭터 취소
    public void Btn_CharAssignCancel()
    {
        //지금 추가하려는 자리에 이미 캐릭터가 할당되어 있으면
        if (GameManager.instance.AssignList[Selected])
        {
            for (int i = 0; i < 10; i++)
            {
                //지금 할당되어 있는 캐릭터 찾기
                if (GameManager.instance.AssignCharIndex[i] == Selected)
                {
                    GameManager.instance.AssignCharIndex[i] = -1;
                    break;
                }
            }
            GameManager.instance.AssignList[Selected] = null;
            CharSelectText[Selected].text = "NULL";

            SaveManager.instance.SaveCharData();        //저장
        }
    }

    //현재 선택된 자리에 RoomList[idx]를 할당
    public void Btn_CharAssign(int idx)
    {
        //Selected : 0 ~ 2 (AssignList의 Index 결정), 현재 선택된 자리
        //idx : 0 ~ 9 (RoomList와 AssignCharIndex의 Index 결정), 현재 선택된 캐릭터의 인덱스
        //AssignList에 할당

        //현재 선택한 캐릭터가 이미 Assign되어 있으면
        if (GameManager.instance.AssignCharIndex[idx] != -1)    //AssignCharIndex는 각 캐릭터가 어떤 자리에 할당되어 있는 지 저장, 할당되어 있지 않으면 -1
        {
            GameManager.instance.AssignList[GameManager.instance.AssignCharIndex[idx]] = null;  //그 캐릭터를 다른 자리에 할당하므로 원래 자리는 비움
            CharSelectText[GameManager.instance.AssignCharIndex[idx]].text = "NULL";            //원래 할당된 자리 텍스트 변경
        }

        //지금 추가하려는 자리에 이미 캐릭터가 할당되어 있으면
        if(GameManager.instance.AssignList[Selected])
        {
            for (int i = 0; i < 10; i++)
            {
                //지금 할당되어 있는 캐릭터 찾기
                if (GameManager.instance.AssignCharIndex[i] == Selected)
                {
                    GameManager.instance.AssignCharIndex[i] = -1;
                    break;
                }
            }
        }

        GameManager.instance.AssignList[Selected] = GameManager.instance.RoomList[idx]; //캐릭터 할당
        GameManager.instance.AssignCharIndex[idx] = Selected;                           //캐릭터 자리 저장
        SetCharText(CharSelectText[Selected], GameManager.instance.RoomList[idx]);      //그 자리의 텍스트 변경
        
        SaveManager.instance.SaveCharData();        //저장
    }

    //캐릭터 레벨, 직업, 색깔(아직 적용 x)을 텍스트에 적용, Btn_CharAssign 함수와 SaveManager의 LoadCharData 함수에서 호출
    public void SetCharText(Text t, Character c)
    {
        t.text = string.Concat("level ", c.level, "\n", charNameString[(int)c.charClass - 1]);

        t.text = string.Concat(t.text, "\n", charColorString[(int)c.charColor / 5]);

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

    public void Btn_ClearRoomList_Debug()
    {
        for (int i = 0; i < 3; i++)
        {
            Selected = -1;
            CharSelectText[i].text = "NULL";
        }

        SaveManager.instance.Btn_ClearRoom_Debug();
        SaveManager.instance.SaveCharData();

        ImageResize();
    }
}
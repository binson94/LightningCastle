using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantManager : MonoBehaviour
{
    int stage;  //현재 스테이지 저장
    int backStage = -1;  //돌아가고자 하는 스테이지, 기본 값 -1

    public List<Button> StageBtns;  //8개의 돌아갈 수 있는 포인트 (1, 6, 11, 16, 21, 26, 31, 36)
                                    //현재 스테이지가 포인트 + 5 이상이면 돌아갈 수 있음 ex) 6스테이지부터 1 스테이지로 돌아갈 수 있음
    public Text StageText;          //돌아가고자 하는 스테이지 표시, 인스펙터 창에서 할당
    bool[] CanBackStage = new bool[8];  //각 포인트로 돌아갈 수 있는 지 저장

    public TraderManager traderM;

    private void Start()
    {
        //현재 스테이지 정보 불러오기
        stage = PlayerPrefs.GetInt("Stage");
        StageBtnColorSet();
    }

    //현재 스테이지 정보와 비교해서 각각 포인트로 돌아갈 수 있는 지 설정 - 돌아갈 수 있으면 버튼 색 흰색, 안되면 버튼 색 회색
    void StageBtnColorSet()
    {
        StageText.text = "";

        ColorBlock cb;          //각각 버튼의 색을 변경하기 위해

        //현재 스테이지에 따라 버튼 명암 설정 - 돌아갈 수 있으면 흰색, 못 돌아가면 회색
        for (int i = 0; i < 8; i++)
        {
            if ((i * 5) + 5 < stage)    //현재 스테이지가 포인트 + 5 이상이면 버튼 색을 흰색으로 바꿈
            {
                cb = StageBtns[i].colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = Color.white;
                StageBtns[i].colors = cb;
                CanBackStage[i] = true;
            }
            else                         //넘지 못하면 뒤에있는 모든 원소도 넘지 못하므로 나머지 원소 모두 회색으로 바꿈
            {
                while (i < 8)
                {
                    cb = StageBtns[i].colors;
                    cb.normalColor = Color.gray;
                    cb.highlightedColor = Color.gray;
                    StageBtns[i].colors = cb;
                    CanBackStage[i] = false;
                    i++;
                }
            }
        }
    }

    //idx * 5 + 1 스테이지로 돌아가고자 설정, 각 버튼에 0 ~ 7 값이 할당되어 있음
    public void Btn_BackStage(int idx)
    {
        if(CanBackStage[idx])
        {
            backStage = idx * 5 + 1;
            StageText.text = backStage.ToString();
        }
    }

    //되돌아가고자 하는 스테이지로 돌아감
    public void Btn_Back()
    {
        if(backStage != -1)
        {
            stage = backStage;
            PlayerPrefs.SetInt("Stage", stage);
            
            StageBtnColorSet();
            backStage = -1;

            //되돌아간 스테이지를 기준으로 새로운 캐릭터로 상점 초기화
            traderM.TradeReset();
        }
        else
        {
            Debug.Log("Cant Back");
        }
    }

    public void Btn_Debug_StageUp()
    {
        PlayerPrefs.SetInt("Stage", 30);
        stage = 30;
        StageBtnColorSet();
    }
}

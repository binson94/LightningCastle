using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using UnityEngine.UI;

public class RefineManager : MonoBehaviour {

    const int DATA_LENGTH = 40;
    
    //지금 설정된 모양을 표시하는 텍스트
    public Text nowShapeText;

    //현재 가지고 있는 갯수를 표시하는 텍스트들
    public Text[] resourceTexts;
    public Text[] gemTexts;

    private ENUM.Shape nowShape = ENUM.Shape.Spade;

	// Use this for initialization
	void Start () {
        TextUpdate();
    }

    //디버그 전용 함수, 보석가루 50개와 각 색깔 염료 5개 추가
    public void Debug_add()
    {
        SaveManager.instance.matsave.Material[0] += 50;
        SaveManager.instance.matsave.Material[(int)ENUM.Color.Red] += 5;
        SaveManager.instance.matsave.Material[(int)ENUM.Color.Orange] += 5;
        SaveManager.instance.matsave.Material[(int)ENUM.Color.Yellow] += 5;
        SaveManager.instance.matsave.Material[(int)ENUM.Color.Green] += 5;
        SaveManager.instance.matsave.Material[(int)ENUM.Color.Blue] += 5;
        SaveManager.instance.matsave.Material[(int)ENUM.Color.Navy] += 5;
        SaveManager.instance.matsave.Material[(int)ENUM.Color.Purple] += 5;

        SaveManager.instance.SaveMaterialData();
        TextUpdate();
    }

    //디버그 전용 함수, 모든 자원 초기화
    public void Debug_reset()
    {
        for (int i = 0; i < 40; i++)
            SaveManager.instance.matsave.Material[i] = 0;

        SaveManager.instance.SaveMaterialData();
        TextUpdate();
    }
  
    //보석 제작 함수
    public void Btn_Create(int colorIdx)
    {
        //무색인 경우 
        if (colorIdx == 0)
        {
            //보석가루 갯수 확인 : 5개 이상이여야 함
            if (SaveManager.instance.matsave.Material[0] >= 5)
            {
                SaveManager.instance.matsave.Material[0] -= 5;
                SaveManager.instance.matsave.Material[(int)nowShape]++;

                SaveManager.instance.SaveMaterialData();
                TextUpdate();
            }
            else
            {
                Debug.LogError("재료가 모자랍니다.");
            }
        }
        else
        {//무색이 아닌 경우
            //보석가루 갯수와 색 염료 갯수 확인 :각각 5개, 1개 이상이여야 함
            if (SaveManager.instance.matsave.Material[(int)colorIdx] >= 1 && SaveManager.instance.matsave.Material[(int)nowShape] >= 1)
            {
                SaveManager.instance.matsave.Material[(int)colorIdx]--;
                SaveManager.instance.matsave.Material[(int)nowShape]--;
                SaveManager.instance.matsave.Material[(int)colorIdx + (int)nowShape]++;

                SaveManager.instance.SaveMaterialData();
                TextUpdate();
            }
            else
            {
                Debug.LogError("재료가 모자랍니다.");
            }
        }
    }

    //분해 함수
    public void Btn_Decomulation(int colorIdx)
    {
        //무색인 경우
        if(colorIdx == 0)
        {
            //보석이 한 개 이상 존재해야 함
            if (SaveManager.instance.matsave.Material[(int)nowShape] >= 1)
            {
                SaveManager.instance.matsave.Material[(int)nowShape]--;
                SaveManager.instance.matsave.Material[0] += 2;

                SaveManager.instance.SaveMaterialData();
                TextUpdate();
            }
            else
            {
                Debug.LogError("분해할 재료가 없습니다.");
            }
        }
        else
        {//무색이 아닌 경우
            //보석이 1개 이상 존재해야 함
            if (SaveManager.instance.matsave.Material[colorIdx + (int)nowShape] >= 1)
            {
                SaveManager.instance.matsave.Material[colorIdx + (int)nowShape]--;

                SaveManager.instance.matsave.Material[0] += 2;              //보석가루 증가
                SaveManager.instance.matsave.Material[colorIdx]++;     //색 염료 증가

                SaveManager.instance.SaveMaterialData();
                TextUpdate();
            }
            else
            {
                Debug.LogError("분해할 재료가 없습니다.");
            }
        }
    }

    public void Btn_SetShape(int shape)
    {
        nowShape = (ENUM.Shape)shape;
        TextUpdate();
    }

    private void TextUpdate()
    {
        //보석 가루, 색 염료 갯수 텍스트 설정
        for (int i = 0; i < 7; i++)
        {
            resourceTexts[i].text = SaveManager.instance.matsave.Material[i * 5].ToString();
        }

        //현재 가리키는 모양 텍스트 설정
        switch(nowShape)
        {
            case ENUM.Shape.Spade:
                nowShapeText.text = "Spade";
                break;
            case ENUM.Shape.Diamond:
                nowShapeText.text = "Diamond";
                break;
            case ENUM.Shape.Clover:
                nowShapeText.text = "Clover";
                break;
            case ENUM.Shape.Heart:
                nowShapeText.text = "Heart";
                break;
        }

        for (int i = 0; i < 8; i++)
        {
            gemTexts[i].text = SaveManager.instance.matsave.Material[(int)nowShape + i * 5].ToString();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.UI;

//참고용
/*************************
enum ItemGrade
{
    Zero = 0, One = 10, Two = 20, Three = 90
}

//2성 이상에만 존재
enum ItemColor
{
    Red = 0, Orange = 10, Yellow = 20, Green = 30, Blue = 40, Navy = 50, Purple = 60
}

enum ItemType
{
    Short = 0, Long = 1, Spear = 2, Orb = 3, Scroll = 4, Staff = 5, Sheild = 6, Iron = 7, Reather = 8, Rob = 9
}
*************************/

public class ItemInfo
{
    public string name = null;     //아이템 이름
    public int grade;       //성
    public int color;       //색깔
    public int type;        //종류

    public int[] stat = new int[2];     //기본 스텟(무기 : 최소 공격력과 최대 공격력, 방어구 : 물리 방어력과 마법 방어력);

    //2성 이상에서만 존재
    public float[] shopOption = new float[2];    //상점 구매 색깔 스텟 범위
    
    public float[] forgeOption = new float[2];   //대장간 제작 색깔 스텟 범위

    public ItemInfo(ItemInfo i)
    {
        name = i.name;
        grade = i.grade;
        color = i.color;
        type = i.type;
        stat[0] = i.stat[0];
        stat[1] = i.stat[1];
        shopOption[0] = i.shopOption[0];
        shopOption[1] = i.shopOption[1];
    }

    public ItemInfo()
    {
        shopOption[0] = shopOption[1] = forgeOption[0] = forgeOption[1] = 0;
    }

    //stat index
    public int GetStatIndex()
    {
        return (grade * 10 + type);
    }

    //option index
    public int GetOptionIdx()
    {
        return ((grade - 2) * 14 + (color - 1) + (type / 6) * 7);
    }
}

public class ItemManager : MonoBehaviour
{
    //아이템 160개
    //0번 대 : 0성, 10번 대 : 1성, 20번 대 : 2성 빨, 80번 대 : 2성 보, 90번 대 : 3성 빨, 150번 대 : 3성 보
    //*0 : 단검, *1 : 장검, *2 : 창, *3 : 보주, *4 : 주문서, *5 : 지팡이, *6 : 방패, *7 : 쇠갑옷, *8 : 가죽갑옷, *9 : 로브
    //아이템 보유 정보는 SaveManager의 itemsave 객체에 저장되어 있음

    public static ItemManager instance = null;
    public ItemInfo[] itemInfo = new ItemInfo[160];         //아이템 정보 테이블

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    //json 파일로 저장되어있는 아이템 데이터 불러오기
    void LoadData()
    {
        ItemInfo tmpItem = new ItemInfo();
        TextAsset loadtxt;

        //ItemNames.json - 아이템 이름(종류 10가지, 별 4가지, 색깔 7가지(2, 3성만) - 10 + 10 + 70 + 70 = 160개)
        //ItemStats.json - 아이템 스텟(종류 10가지, 별 4가지 - 40개)
        //ItemOptions.json - 아이템 옵션(별 2가지, 종류 2가지, 색깔 7가지 - 28개)
        JsonData[] jsons = new JsonData[3];     

        string path = "Items/";


        loadtxt = Resources.Load<TextAsset>(string.Concat(path, "ItemNames"));
        jsons[0] = JsonMapper.ToObject(loadtxt.text);

        loadtxt = Resources.Load<TextAsset>(string.Concat(path, "ItemStats"));
        jsons[1] = JsonMapper.ToObject(loadtxt.text);

        loadtxt = Resources.Load<TextAsset>(string.Concat(path, "ItemOptions"));
        jsons[2] = JsonMapper.ToObject(loadtxt.text);

        for (int i = 0; i < 160; i++)
        {
            //이름 성, 타입, 색 불러오기
            tmpItem.name = jsons[0][i]["itemName"].ToString();
            tmpItem.grade = int.Parse(jsons[0][i]["itemGrade"].ToString());
            tmpItem.type = int.Parse(jsons[0][i]["itemType"].ToString());
            tmpItem.color = int.Parse(jsons[0][i]["itemColor"].ToString());

            //기본 스텟 로드(공격력 또는 방어력)
            tmpItem.stat[0] = int.Parse(jsons[1][tmpItem.GetStatIndex()]["statMin"].ToString());
            tmpItem.stat[1] = int.Parse(jsons[1][tmpItem.GetStatIndex()]["statMax"].ToString());

            //2성 이상
            if(i >= 20)
            {
                //색깔 옵션 랜덤 범위 로드
                tmpItem.shopOption[0] = float.Parse(jsons[2][tmpItem.GetOptionIdx()]["shopMin"].ToString());
                tmpItem.shopOption[1] = float.Parse(jsons[2][tmpItem.GetOptionIdx()]["shopMax"].ToString());

                tmpItem.forgeOption[0] = float.Parse(jsons[2][tmpItem.GetOptionIdx()]["forgeMin"].ToString());
                tmpItem.forgeOption[1] = float.Parse(jsons[2][tmpItem.GetOptionIdx()]["forgeMax"].ToString());
            }

            itemInfo[i] = new ItemInfo(tmpItem);
        }
    }
}
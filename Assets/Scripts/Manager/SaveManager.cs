using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


//GameManager의 RoomList를 저장
[Serializable]
public class CharacterSave
{
    //장비 정보
    public int[] Weapon = new int[10];
    public int[] Armor = new int[10];

    public int[] Level = new int[10];       //RoomList에 있는 캐릭터들 레벨
    public int[] Color = new int[10];       //RoomList에 있는 캐릭터들 색  (0 : None, 1 : Red ... 7 : Purple)
    public int[] Class = new int[10];       //RoomList에 있는 캐릭터들 직업(0 : None, 1 : Hero, 2 : Chaser, 3 : Crusher)

    public int[] Index = new int[10];       //AssignCharIndex를 저장
}

[Serializable]
public class MaterialSave
{
    public int[] Material = new int[40];
}

[Serializable]
public class ItemSave
{
    public int count;                       //아이템 보유 갯수
    public int[] itemIdx = new int[200];    //아이템 index - 획득 순으로 앞에서부터 추가
    public int[] itemStat = new int[200];   //아이템 확정된 랜덤값
}


//시간 보상 : 0, 15, 60, 240
//처치 보상 : 0, 100, 1000, 10000
//보석 획득(드롭만) : 0, 10, 200, 3000(가루로 계산)
[Serializable]
public class RewardSave
{
    public int Time;
    public int EnemyKill;
    public int JemDrop;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance = null;              //외부 접근 용

    public CharacterSave charsave = new CharacterSave();    //직렬화해서 저장용 - 캐릭터 정보
    public MaterialSave matsave = new MaterialSave();       //직렬화해서 저장용, 직접 사용 - 자원 정보
    public ItemSave itemsave = new ItemSave();              //직렬화해서 저장용, 장비 정보
    public RewardSave rewardsave = new RewardSave();        //직렬화해서 저장용, 직접 사용 - 업적 정보

    bool loaded = false;      //로드를 한 번만 하기 위해 존재

    public GameObject[] CharPrefabs;                        //직업별 프리팹


    //인스턴스 생성
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }


    //디버그 용 - 3레벨인 각 직업 캐릭터 하나 씩 생성해서 저장
    private void Debug_Char()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Char.dat");

        for (int i = 0; i < 3; i++)
        {
            charsave.Level[i] = 3;
            charsave.Class[i] = i + 1;
            charsave.Index[i] = -1;

            charsave.Weapon[i] = charsave.Armor[i] = 0;
        }

        for (int i = 3; i < 10; i++)
        {
            charsave.Level[i] = 0;
            charsave.Color[i] = 0;
            charsave.Class[i] = 0;
            charsave.Index[i] = -1;

            charsave.Weapon[i] = charsave.Armor[i] = 0;
        }

        charsave.Color[0] = (int)ENUM.Color.Red;
        charsave.Color[1] = (int)ENUM.Color.Navy;
        charsave.Color[2] = (int)ENUM.Color.Yellow;

        bf.Serialize(file, charsave);
        file.Close();
    }


    //저장 관련
    //**********************************************
    //게임 종료 시 모두 저장
    public void SaveAll()
    {
        SaveCharData();
        SaveMaterialData();
        SaveRewardData();
        SaveItemData();
    }

    //GameManager의 RoomList에 있는 캐릭터 정보를 dat 파일로 저장, GameManager의 AddChar 함수에서 호출
    public void SaveCharData()
    {
        Debug.Log(Application.persistentDataPath);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Char.dat");

        int i;
        //RoomList에 있는 캐릭터 정보 저장하기
        for (i = 0; i < 10; i++)
        {
            //빈칸이면 그 뒤는 계속 빈칸이므로 break
            if (GameManager.instance.RoomList[i] == null)
                break;

            charsave.Level[i] = GameManager.instance.RoomList[i].level;
            charsave.Color[i] = (int)GameManager.instance.RoomList[i].charColor;
            charsave.Class[i] = (int)GameManager.instance.RoomList[i].charClass;
        }

        //남은 칸 0으로 채움, Level 0이 없다는 표시이기 때문에 필요
        for (; i < 10; i++)
        {
            charsave.Level[i] = 0;
            charsave.Color[i] = 0;
            charsave.Class[i] = 0;
        }

        //AssignCharIndex 저장
        for (i = 0; i < 10; i++)
            charsave.Index[i] = GameManager.instance.AssignCharIndex[i];


        bf.Serialize(file, charsave);
        file.Close();
    }

    //현재 matsave 객체에 저장된 정보를 dat 파일로 저장, RefineManager의 Create, Decomulation 함수에서 호출
    public void SaveMaterialData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Material.dat");

        bf.Serialize(file, matsave);
        file.Close();
    }

    //현재 rewardsave 객체에 저장된 정보를 dat 파일로 저장, NightManager의 Btn_LoadDay, Btn_FailLoad 함수에서 호출
    public void SaveRewardData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Reward.dat");

        bf.Serialize(file, rewardsave);
        file.Close();
    }

    //현재 itemsave 객체에 저장된 정보를 dat 파일로 저장, ItemManager에서 호출
    public void SaveItemData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Item.dat");

        bf.Serialize(file, itemsave);
        file.Close();
    }
    //**********************************************


    //불러오기 관련
    //**********************************************
    //Load 함수들은 게임 시작 시 한 번만 호출됨, DayManager의 Start에서 호출
    public void Load()
    {
        if (!loaded)
        {
            loaded = true;

            LoadItemData();
            LoadCharData();
            LoadMaterialData();
            LoadRewardData();
        }
    }

    //dat 파일로 저장된 데이터 불러오기, 캐릭터 인스턴스 생성해서 GameManager의 RoomList와 연결, DayManager의 Start에서 호출
    void LoadCharData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (File.Exists(Application.persistentDataPath + "/Char.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/Char.dat", FileMode.Open);

            charsave = (CharacterSave)bf.Deserialize(file);

            for (int i = 0; i < 10; i++)        //데이터 불러와서 그 데이터로 RoomList 채우기
            {
                if (charsave.Level[i] == 0)     //레벨이 0으로 저장된 정보는 무시
                {
                    GameManager.instance.RoomList[i] = null;
                    continue;
                }

                //저장된 클래스 숫자에 따라 인스턴스 생성
                GameManager.instance.RoomList[i] = Instantiate(CharPrefabs[charsave.Class[i] - 1]).GetComponent<Character>();

                //레벨, 색깔, 클래스 정보 불러옴
                GameManager.instance.RoomList[i].level = charsave.Level[i];
                GameManager.instance.RoomList[i].charColor = (ENUM.Color)charsave.Color[i];
                GameManager.instance.RoomList[i].charClass = (CharClass)charsave.Class[i];
                GameManager.instance.RoomList[i].AnimationColorSet();

                //씬이 넘어가도 사라지지 않게 함
                GameManager.instance.RoomList[i].gameObject.SetActive(false);
                DontDestroyOnLoad(GameManager.instance.RoomList[i].gameObject);
            }

            //AssignCharIndex 불러오기
            for (int i = 0; i < 10; i++)
            {
                GameManager.instance.AssignCharIndex[i] = charsave.Index[i];

                if (charsave.Index[i] != -1)
                {
                    GameManager.instance.AssignList[charsave.Index[i]] = GameManager.instance.RoomList[i];
                }
            }

            file.Close();
        }
        else
        {
            //디버그 용, 캐릭터 초기화(hero, chaser, crusher 각 3레벨로 하나씩)
            Debug_Char();
            LoadCharData();
        }
    }

    //dat 파일로 저장된 데이터 불러오기, matsave 객체로 불러옴
    void LoadMaterialData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (File.Exists(Application.persistentDataPath + "/Material.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/Material.dat", FileMode.Open);

            matsave = (MaterialSave)bf.Deserialize(file);

            file.Close();
        }
        else
        {
            for (int i = 0; i < 40; i++)
                matsave.Material[i] = 0;
        }
    }

    //dat 파일로 저장된 데이터 불러오기, rewardsave 객체로 불러옴
    void LoadRewardData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (File.Exists(Application.persistentDataPath + "/Reward.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/Reward.dat", FileMode.Open);

            rewardsave = (RewardSave)bf.Deserialize(file);

            file.Close();
        }
        else
        {
            rewardsave.Time = rewardsave.EnemyKill = rewardsave.JemDrop = 0;
        }

        StartCoroutine(TimeCount());
    }

    //dat 파일로 저장된 데이터 불러오기, itemsave 객체로 불러옴
    void LoadItemData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (File.Exists(Application.persistentDataPath + "/Item.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/Item.dat", FileMode.Open);

            itemsave = (ItemSave)bf.Deserialize(file);

            file.Close();
        }
        else
        {
            itemsave.count = 0;
            for (int i = 0; i < 200; i++)
            {
                itemsave.itemIdx[i] = -1;
                itemsave.itemStat[i] = 0;
            }
        }
    }
    //**********************************************


    public void OrganizeTextSet()
    {
        for (int i = 0; i < 10; i++)
        {
            if (charsave.Index[i] != -1)
                OrganizeManager.instance.SetCharText(OrganizeManager.instance.CharSelectText[charsave.Index[i]], GameManager.instance.RoomList[i]);
        }
    }

    //모든 캐릭터 제거
    public void Btn_ClearRoom_Debug()
    {
        for (int i = 0; i < 10; i++)
        {
            if (GameManager.instance.RoomList[i])
                Destroy(GameManager.instance.RoomList[i].gameObject);
            GameManager.instance.AssignCharIndex[i] = -1;
        }

        for (int i = 0; i < 3; i++)
            GameManager.instance.AssignList[i] = null;
        
        Debug_Char();
        LoadCharData();
    }

    //업적 - 시간재기
    IEnumerator TimeCount()
    {
        while (true)
        {
            rewardsave.Time++;
            yield return new WaitForSeconds(1);
        }
    }
}

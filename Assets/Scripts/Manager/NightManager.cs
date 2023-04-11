using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LitJson;

public class NightManager : MonoBehaviour
{
    //적 프리팹
    //***********************************
    public GameObject[] enemyPrefabs;                 //적들의 프리팹 정보를 저장하고 있는 배열
                                                      //인스펙터 창에서 직접 할당해줌
    //***********************************

    //MonsterAdd 함수에서 이용할 List들, index는 0 ~ 3번 존재(한 맵 당 몬스터 최대 4종류)
    //각 스테이지마다 최대 4종류의 몬스터가 나옴, index는 그거 결정
    //***********************************
    int[] enemyKindIdxs = new int[4];        //몬스터 종류 index - enemyPrefabs 결정
    int[] enemyAmounts = new int[4];         //몬스터 총 량
    int[] enemySpawnOnces = new int[4];      //한 번에 소환되는 양
    float[] enemyBeforeDelays = new float[4];//첫 번째 소환 전 딜레이
    float[] enemySpawnDelays = new float[4]; //소환 사이의 딜레이

    int[] startIdx = new int[4];             //TotalEnemys의 startIdx[i] ~ startIdx[i + 1] - 1까지가 i 몬스터
    List<Enemy> totalEnemys = new List<Enemy>();         //스테이지 내에 생성한 모든 적 저장, 스테이지 클리어 판정을 위해 존재
    //***********************************

    //캐릭터 관련
    //***********************************
    private Character[] chars = new Character[3];      //조종하는 캐릭터들, GameManager의 AssignList를 불러옴
    public GameObject DummyCharacter;                  //AssignList가 비어있을 때 채울 Dummy Prefab
    public Slider[] HPBars;                            //체력바들, Start 함수에서 캐릭터별로 할당, 인스펙터 창에서 이미 할당되어 있음
    //***********************************

    //아이템 드롭 관련
    //***********************************
    private float StageDropRate;                       //적이 죽었을 때 아이템이 뜰 확률 계수
    private float StageDropAmtBonus;                   //적이 죽었을 때 아이템 갯수 계수
    
    private bool[] CanItemDrop = new bool[4];          //무색 가루 : 0, 무색 보석 : 1, 색 염료 : 2, 유색 보석 : 3
                                                       //각각 재료가 뜰 수 있는 지 저장
    public int[] NewDrop = new int[40];                //새로 드랍된 재료는 여기에 저장하고 게임 클리어 시 한번에 반영
    //***********************************

    public JoystickManager joystickM;                           //JoyStickManager 불러옴, 인스펙터 창에서 할당
    public AudioSource audioSource;                             //배경음악, 인스펙터 창에서 할당

    public SpriteRenderer NightBG;                              //여기부터
    public SpriteRenderer DayBG;                                //
    public SpriteRenderer Fireplace;                            //여기까지는 이미지 페이드 인, 페이드 아웃 용으로, 인스펙터 창에서 할당

    public GameObject GameOverCanvas;                           //게임 오버 시 gameOverCanvas 보이게 함
    public GameObject GameClearCanvas;                          //게임 클리어 시 gameClaerCanvas 보이게 함

    private int StageNum;                                       //Start 함수에서 PlayerPrefs의 Stage 값 읽어옴
    public int CastleHP = 25;                                   //성 체력

    int[] damages = new int[3];
    public Text[] damageText;               //캐릭터 데미지 표

    private void Start()
    {
        StageNum = PlayerPrefs.GetInt("Stage");
        GameManager.instance.NightM = this;
        
        LoadCharData(); //GameManager에서 캐릭터 정보 불러오기
        StageLoad();    //스테이지 정보 불러오기
        StartBattle();  //전투 시작
    }

    //캐릭터 불러오기
    //************************************
    //GameManager의 AssignList 불러오기 - Start 함수에서 호출
    void LoadCharData()
    {
        for (int i = 0; i < 3; i++)
        {
            chars[i] = GameManager.instance.AssignList[i];

            //데미지 표 초기화
            damages[i] = 0;
            damageText[i].text = "0";

            //할당되어 있지 않으면 더미를 만들어서 비활성화
            if (!chars[i])
            {
                chars[i] = Instantiate(DummyCharacter).GetComponent<Character>();
                chars[i].gameObject.SetActive(false);

                chars[i].hpBar = HPBars[i];     //체력바 연동은 하는 데 바로 비활성화
                HPBars[i].gameObject.SetActive(false);
            }
            else
            {
                chars[i].hpBar = HPBars[i];     //체력바 연동

                //수호자면 쉴드 바도 연동
                if (chars[i].charClass == CharClass.Guardian)
                    chars[i].GetComponent<Guardian>().shieldBar = HPBars[i + 3];

                chars[i].CharInstantiate(i);     //캐릭터 별 특수 오브젝트 생성(부메랑, 총알 등)
                chars[i].BoolInit();             //캐릭터 상태 초기화
            }

            //shield bar는 기본적으로 비활성화
            HPBars[i + 3].gameObject.SetActive(false);
        }

        joystickM.LoadCharData(chars);
    }
    //************************************


    //스테이지 불러오기
    //************************************
    //json에 저장되어있는 스테이지 정보 불러오기, Start 함수에서 호출
    private void StageLoad()
    {
        TextAsset TxtAsset;
        string loadStr;
        JsonData json;

        TxtAsset = Resources.Load<TextAsset>("StageInfo");

        loadStr = TxtAsset.text;
        json = JsonMapper.ToObject(loadStr);

        int idx, amount, spawnOnce;
        float beforeDelay, spawnDelay;

        StageDropRate = float.Parse(json[StageNum]["StageDropRate"].ToString());
        StageDropAmtBonus = float.Parse(json[StageNum]["StageDropAmtBonus"].ToString());

        for (int i = 0; i < 4; i++)
            startIdx[i] = 0;

        for (int i = 1; i <= 4; i++)
        {
            idx = (int)json[StageNum][string.Concat("Index", i.ToString())];
            amount = (int)json[StageNum][string.Concat("Amount", i.ToString())];
            spawnOnce = (int)json[StageNum][string.Concat("SpawnOnce", i.ToString())];
            beforeDelay = float.Parse(json[StageNum][string.Concat("BeforeDelay", i.ToString())].ToString());
            spawnDelay = float.Parse(json[StageNum][string.Concat("Delay", i.ToString())].ToString());

            for (int j = i; j < 4; j++)
                startIdx[j] += amount;

            MonsterAdd(i - 1, idx, amount, spawnOnce, beforeDelay, spawnDelay);
        }
    }

    //몬스터 생성하는 함수 - StageLoad 함수에서 호출
    private void MonsterAdd(int idx, int monIdx, int amount, int spawnOnce, float beforeDelay, float spawnDelay)
    {
        if (monIdx == 0)
            return;

        enemyKindIdxs[idx] = monIdx;
        enemyAmounts[idx] = amount;
        enemySpawnOnces[idx] = spawnOnce;
        enemyBeforeDelays[idx] = beforeDelay;
        enemySpawnDelays[idx] = spawnDelay;
    }
    //************************************


    //전투 시작
    //************************************
    //전투 시작 - Start 함수에서 호출
    private void StartBattle()
    {
        //더미가 아닌 캐릭터 키고 스텟 불러오기
        for (int i = 0; i < 3; i++)
        {
            if (chars[i].charClass != CharClass.Dummy)
            {
                chars[i].gameObject.SetActive(true);
                chars[i].BasicStatLoad();
                chars[i].TotalStatLoad();
            }
        }

        Time.timeScale = 1;
        MonsterInstantiate();

        for (int i = 0; i < 4; i++)
        {
            StartCoroutine(Spawn(i));
        }

        joystickM.charMoveCoroutine = StartCoroutine(joystickM.ControlCharMove());

        audioSource.Play();
        StartCoroutine(GameClearCheck());
    }

    //몬스터 오브젝트들 생성, StartBattle 함수에서 호출
    private void MonsterInstantiate()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < enemyAmounts[i]; j++)
            {
                totalEnemys.Add(Instantiate(enemyPrefabs[enemyKindIdxs[i]]).GetComponent<Enemy>());
                totalEnemys[totalEnemys.Count - 1].StatLoad(this);
                totalEnemys[totalEnemys.Count - 1].gameObject.SetActive(false);
            }
        }
    }

    //몬스터 등장(몬스터 종류별로 개별적으로 동작 == 4번 호출), StartBattle 함수에서 호출
    IEnumerator Spawn(int idx)
    {
        float delay;
        yield return new WaitForSeconds(enemyBeforeDelays[idx]);

        for (int i = 0; i < enemyAmounts[idx];)
        {
            for (int j = 0; j < enemySpawnOnces[idx]; j++)
            {
                if (i == enemyAmounts[idx])
                    break;
                totalEnemys[startIdx[idx] + i].transform.position = new Vector3(10.9f, -5.1f, 0);
                totalEnemys[startIdx[idx] + i].gameObject.SetActive(true);
                totalEnemys[startIdx[idx] + i++].AttackMain();
            }

            delay = Random.Range(0.6f, 1.4f) * enemySpawnDelays[idx];
            yield return new WaitForSeconds(delay);
        }
    }
    //************************************


    //클리어 관련
    //************************************
    //몬스터를 전부 잡았는 지 체크, 몬스터를 전부 잡았으면 GameClear 호출, StartBattle 함수에서 호출
    IEnumerator GameClearCheck()
    {
        yield return new WaitForSeconds(10f);

        reset:
        foreach (Enemy now in totalEnemys)
        {
            if (now.isActiveAndEnabled)
            {
                yield return new WaitForSeconds(3f);
                goto reset;
            }
        }

        Debug.Log("Clear");
        yield return new WaitForSeconds(0.5f);
        GameClear();
    }
    
    //gameClearCheck 코루틴에서 호출, GameClearCanvas 보이게 함
    public void GameClear()
    {
        GameClearCanvas.SetActive(true);
    }

    //드랍된 아이템들 저장, FadeOut 호출
    public void Btn_LoadDay()
    {
        GameClearCanvas.SetActive(false);

        //드롭된 재료 아이템들 적용
        for (int i = 0; i < 40; i++)
        {
            //아이템 드롭 디버그
            if (NewDrop[i] != 0)
                Debug.Log(i.ToString() + ' ' + NewDrop[i].ToString());

            SaveManager.instance.matsave.Material[i] += NewDrop[i];

            //보석 가루나 색 염료 제외, 보석이 드랍된 경우 업적에 적용
            if (i % 5 != 0)
                SaveManager.instance.rewardsave.JemDrop += NewDrop[i];
        }

        SaveManager.instance.SaveMaterialData();
        SaveManager.instance.SaveRewardData();

        StartCoroutine(FadeOut());
    }

    //게임 클리어 시 배경 이미지 FadeOut
    IEnumerator FadeOut()
    {
        for (float i = 0; i < 1; i += 0.01f)
        {
            NightBG.color = new Color(255, 255, 255, 1 - i);
            Fireplace.color = new Color(255, 255, 255, i);
            DayBG.color = new Color(255, 255, 255, i);
            yield return new WaitForSeconds(0.01f);
        }

        Scene_LoadDay();
    }

    //FadeOut 끝나면 호출, 낮 씬 로드하면서 스테이지 증가
    public void Scene_LoadDay()
    {
        PlayerPrefs.SetInt("Stage", ++StageNum);
        SceneManager.LoadScene(1);
    }

    //게임 오버 시 낮 씬 로드, 스테이지는 그대로
    public void Btn_FailLoad()
    {
        SaveManager.instance.SaveRewardData();

        SceneManager.LoadScene(1);
    }
    //************************************


    //몬스터 관련
    //************************************
    //몬스터 사망 시마다 호출, 각각 재료 아이템 드롭 결정
    public void ItemDrop(float MonDropRate, float MonDropAmtBonus)
    {
        float[] DropRate = new float[4];
        int[] DropAmount = new int[4];

        for (int i = 0; i < 4; i++)
            DropRate[i] = StageDropRate * MonDropRate;  //모두 기본적으로는 두 확률의 곱은 식에 들어감

        DropRate[0] *= 0.1f;        //보석 가루 계수
        DropRate[1] *= 0.03f;       //무색 보석 계수
        DropRate[2] *= 0.04f;       //색 염료 계수
        DropRate[3] *= 0.01f;       //유색 보석 계수

        for (int i = 0; i < 4; i++)
        {
            int Probability = Random.Range(0, 100);  //0 이상 99 이하 임의 값

            if (CanItemDrop[i])          //각 재료가 드롭 가능하면
            {
                if (Probability < DropRate[i] * 100) //백분위로 확률 비교
                {
                    int Amount = 1;     //드롭 갯수, 식 나오면 수정
                    switch (i)
                    {
                        case 0: //보석 가루
                            NewDrop[0] += Amount;
                            break;
                        case 1: //무색 보석
                            NewDrop[Random.Range(1, 5)] += Amount;
                            break;
                        case 2: //색 염료
                            NewDrop[Random.Range(1, 8) * 5] += Amount;
                            break;
                        case 3: //유색 보석
                            NewDrop[Random.Range(1, 8) * 5 + Random.Range(1, 5)] += Amount;
                            break;
                    }
                }
            }
        }
    }
    
    //2층 왼쪽 끝에 몬스터 침입 시 호출, LastGate에서 호출
    public void EnemyInvasion(int amount)
    {
        CastleHP -= amount;

        if(CastleHP < 0)
        {
            GameManager.instance.GameOver();
        }
    }

    //딜미터기에 적용
    public void DamageUpdate(int idx, int damage)
    {
        damages[idx] += damage;
        damageText[idx].text = damages[idx].ToString();
    }
    //************************************
}
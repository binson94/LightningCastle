using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
/***********************************************************************************
 적 클래스에 상속하기 위한 부모 클래스

    <전투 관련 내용>
    전투를 위한 정수형 변수 HP, attack, attack_speed,
    그리고 Character형 변수인 target 선언
************************************************************************************/

public class Enemy : MonoBehaviour
{
    //기본 정보
    //*********************************
    public int monsterIndex = 1;
    //*********************************

    //스텟 관련
    //*******************************************
    public float[] stats = new float[17];
    /********************************************
    00 공격 타입        01 레벨           02 최대 체력
    03 최소 공격력      04 최대 공격력     05 사거리
    06 공격 속도        07 물리 방어력     08 마법 방어력
    09 이동 속도        10 넉백 저항      11 재료 드랍 확률
    12 재료 드랍 보너스  13 보석가루 드랍  14 무색보석 드랍
    15 색염료 드랍       16 유색보석 드랍
    *********************************************/
    
    static string[] statName = new string[17];

    public int currentHP;           //현재 체력
    //*******************************************



    //상태 이상 관련
    //*******************************************
    public Rigidbody2D myRigid2D;   //넉백 적용을 위한 요소
    public bool isKnockBack;        //넉백 중 여부

    public float stunTime;          //stun 지속 시간
    public bool isStun;             //stun 여부

    public float slowTime;          //slow 지속 시간
    public bool isSlow;             //슬로우 여부

    bool isDeath;                   //사망 여부
    //*******************************************

    //자동 전투 관련
    //*******************************************
    public List<Character> targets; //타겟 리스트
    public Character currentTarget; //가장 가까운 타겟
    public Vector2 targetVector;    //타겟 방향 벡터
    float targetDistance;           //가장 가까운 타겟과의 거리

    public bool isAttack;           //공격 중

    public bool isChase = false;    //추적 중 여부
    Coroutine Chase;                //추적 코루틴

    public Area nowArea;            //현재 위치

    float AttackBeforeDelay;
    //*******************************************

    //애니메이션 관련
    //*************************
    public Animator anim;
    //*************************

    //아이템 드롭, 데미지 표
    //*************************
    bool isDrop = false;        //아이템 드롭을 한 번만 계산하도록 제한
    NightManager nightM;
    //*************************

    private void Start()
    {
        targets = new List<Character>();
        myRigid2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        nowArea = GetArea(transform.position);
    }

    public void StatnameInitialization()
    {
        statName[0] = "AttackType";
        statName[1] = "Level";
        statName[2] = "HP";
        statName[3] = "AttackMin";
        statName[4] = "AttackMax";
        statName[5] = "AttackRange";
        statName[6] = "AttackSpeed";
        statName[7] = "PhysicalDefense";
        statName[8] = "MagicDefense";
        statName[9] = "MovementSpeed";
        statName[10] = "KnockbackResist";
        statName[11] = "monDropRate";
        statName[12] = "monDropAmtBonus";
        statName[13] = "nDust";
        statName[14] = "nJam";
        statName[15] = "cDust";
        statName[16] = "cJam";
    }

    //전투 관련
    //**********************************************
    //공격 메인 함수
    public void AttackMain()
    {
        if (!isAttack)
        {
            isAttack = true;

            AnimationSet(Anim.Idle);

            SetTargetByDistance();

            if (currentTarget == null)
            {
                isAttack = false;

                if (isActiveAndEnabled)
                    StartCoroutine(MoveToCastle());
            }
            else if(isActiveAndEnabled)
            {
                Chase = StartCoroutine(ChaseAndAttack());
            }
        }
    }

    //접근 후 공격
    private IEnumerator ChaseAndAttack()
    {
        if (!isChase)
        {
            isChase = true;
            AnimationSet(Anim.Move);

            while (targetDistance > stats[(int)EnemyStat.Range])
            {
                if (!isKnockBack)
                {
                    transform.Translate(new Vector2(-1, 0) * stats[(int)EnemyStat.MoveSpeed] * 0.003f);
                }

                if (myRigid2D.velocity.x <= 0)
                    isKnockBack = false;

                SetTargetByDistance();

                if (currentTarget == null)
                {
                    if (isActiveAndEnabled)
                    {
                        isChase = false;
                        StartCoroutine(AttackReady());
                        yield break;
                    }
                }

                yield return new WaitForSeconds(0.03f);
            }

            isChase = false;
            AnimationSet(Anim.Attack);
        }
    }

    //실제 피해를 입히는 함수 - 애니메이션 호출
    public void AttackDamage() 
    {
        if (currentTarget != null)
        {
            currentTarget.GetDamage((int)Random.Range(stats[(int)EnemyStat.AttackMin], stats[(int)EnemyStat.AttackMax] + 1), stats[0] == 0);

            if (currentTarget.DeathCheck())
            {
                targets.Remove(currentTarget);
                currentTarget.gameObject.SetActive(false);
                currentTarget = null;
            }
        }
    }

    //공격 딜레이 - 애니메이션 호출
    public IEnumerator AttackReady()
    {
        float i;
        yield return new WaitForSeconds(i = 1 / (4 * stats[(int)EnemyStat.AttackSpd]));      //대기 시간

        AnimationSet(Anim.Idle);

        for (; i < 1 / stats[(int)EnemyStat.AttackSpd]; i += 0.03f)
        {
            AnimationSet(Anim.Move);
            SetTargetByDistance();

            //추격
            if (currentTarget != null && !isKnockBack)
            {
                if (targetDistance > stats[(int)EnemyStat.Range])
                {
                    transform.Translate(targetVector * stats[(int)EnemyStat.MoveSpeed] * 0.003f);
                }
            }
            
            yield return new WaitForSeconds(0.03f);
        }

        isAttack = false;
        AttackMain();
    }

    //피해를 받는 함수
    public void GetDamage(int damage, bool type, int idx)
    {
        //물리, 마법 타입 구분 : 물리 : true, 마법 : false
        if (type)
            damage -= (int)stats[(int)EnemyStat.PhyDefence];
        else
            damage -= (int)stats[(int)EnemyStat.MagDefence];

        if (damage < 1)
            damage = 1;

        currentHP -= damage;

        nightM.DamageUpdate(idx, damage);
    }

    //데미지 입을 때마다 호출, 체력이 0 이하이면 사망 설정
    public bool DeathCheck()
    {
        if (currentHP <= 0)
        {
            currentHP = 0;
            Death();
            return true;
        }

        return false;
    }

    void Death()
    {
        gameObject.SetActive(false);
    }

    //타겟이 없는 경우 - 성을 향해 진행
    public IEnumerator MoveToCastle()
    {
        while (targets.Count < 1)
        {
            if (!(isKnockBack || isStun))
            {
                AnimationSet(Anim.Move);
                transform.Translate(new Vector2(-1, 0) * stats[(int)EnemyStat.MoveSpeed] * 0.003f);
            }

            if (myRigid2D.velocity.x <= 0)
                isKnockBack = false;

            yield return new WaitForSeconds(0.03f);
        }
    }
    //**********************************************


    //타겟 관련
    //****************************************
    //거리에 따른 타겟 설정
    private void SetTargetByDistance()
    {
        if (targets.Count > 0)
        {
            targets.Sort(Compare);

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].currentHP <= 0)
                {
                    targets.RemoveAt(i);
                }
            }

            if (targets.Count > 0)
            {
                currentTarget = targets[0];
                SetTargetDistance();
            }
            else
                currentTarget = null;
        }
        else
            currentTarget = null;
    }

    //현재 타겟 적과의 거리를 계산
    private void SetTargetDistance()  //타겟과의 거리와 방향 결정
    {
        if (currentTarget != null)
        {
            targetVector = (currentTarget.gameObject.transform.position - gameObject.transform.position);
            targetDistance = Vector2.Distance(gameObject.transform.position, currentTarget.gameObject.transform.position);
            targetVector.y = 0;

            targetVector.Normalize();
        }
    }

    //타겟 배열 정렬을 위한 비교 함수 - 거리가 가까운 순 정렬
    int Compare(Character c1, Character c2)
    {
        float d1 = Vector3.Distance(transform.position, c1.transform.position);
        float d2 = Vector3.Distance(transform.position, c2.transform.position);

        if (d1 < d2)
            return -1;
        else if (d1 == d2)
            return 0;
        else
            return 1;
    }
    //****************************************

        
    //상태 이상 관련
    //***********************************
    //넉백 코루틴 호출
    public void Knockback(float knockbackRate)
    {
        if (isActiveAndEnabled && !isKnockBack)
            StartCoroutine(KnockbackCoroutine(knockbackRate));
    }

    //실제 넉백
    IEnumerator KnockbackCoroutine(float knockbackRate)
    {
        if (myRigid2D != null)
            if (!isKnockBack)
            {
                isKnockBack = true;
                Vector2 knockbackVector;

                if (targetVector.x > 0)
                    knockbackVector = Vector2.left * (1 - stats[(int)EnemyStat.KnockbackReg]) * knockbackRate;
                else
                    knockbackVector = Vector2.right * (1 - stats[(int)EnemyStat.KnockbackReg]) * knockbackRate;
                

                myRigid2D.velocity = 2 * knockbackVector;

                while (myRigid2D.velocity.x > 0)
                {
                    yield return new WaitForSeconds(0.03f);
                }

                isKnockBack = false;
            }
    }

    //캐릭터에서 호출, Stun 걸려 있지 않으면 걸고, 걸려있으면 시간 초기화
    public void StunTimeSet(float Stuntime)
    {
        this.stunTime = Mathf.Max(this.slowTime, slowTime);

        StartCoroutine(Stun());
    }

    //스턴 시간 업데이트하는 코루틴
    private IEnumerator Stun()
    {
        if (isStun)
            yield break;

        isStun = true;
        while (stunTime > 0)
        {
            stunTime -= 0.03f;
            yield return new WaitForSeconds(0.03f);
        }
        stunTime = 0;


        isStun = false;
    }

    //캐릭터에서 호출, 둔화 걸고, 걸려있으면 시간 초기화
    public void SlowTimeSet(float SlowTime)
    {
        this.slowTime = Mathf.Max(this.slowTime, SlowTime);

        StartCoroutine(Slow());
    }

    //슬로우 시간 업데이트하는 코루틴
    public IEnumerator Slow()
    {
        yield return null;
    }

    //캐릭터에서 호출, 독 걸기
    public void PoisonSet(int Damage)
    {
        StartCoroutine(Poison(Damage / 4));
    }

    //독 데미지 주는 코루틴
    private IEnumerator Poison(int Damage)
    {
        for(int i =0; i < 4;i++)
        {
            yield return new WaitForSeconds(1f);
            currentHP -= Damage;

            if (DeathCheck())
                yield break;
        }

    }
    //*********************************


    //스텟 관련
    //*********************************
    public void StatLoad(NightManager nm)
    {
        nightM = nm;
        BasicStatLoad();
        TotalStatLoad();
    }

    private void BasicStatLoad()
    {
        if (statName[0] != "AttackType")
            StatnameInitialization();

        TextAsset txtAsset;
        string loadStr;
        JsonData json;
        string DeltaStr = "Delta";

        txtAsset = Resources.Load<TextAsset>("Stats/EnemyStat");
        loadStr = txtAsset.text;
        json = JsonMapper.ToObject(loadStr);

        //공격 타입은 로드
        stats[(int)EnemyStat.AttackType] = float.Parse(json[monsterIndex][statName[(int)EnemyStat.AttackType]].ToString());

        //레벨 난수로 설정, 데이터 레벨 값에 -0.5에서 0.5 사이 값을 더한 후 반올림
        float tmpLevel = float.Parse(json[monsterIndex][statName[(int)EnemyStat.Level]].ToString());
        stats[(int)EnemyStat.Level] = (int)Mathf.Round(tmpLevel + Random.Range(-0.5f, 0.501f));

        //기준 레벨은 내림한 것, 기준 레벨과 같으면 DeltaStr을 빈 string으로 설정
        if (stats[(int)EnemyStat.Level] == (int)tmpLevel)
            DeltaStr = "";

        //체력부터 드롭 보너스까지는 레벨에 따른 변동이 있음, 그래서 레벨에 따라 데이터 불러옴
        int i;
        for (i = (int)EnemyStat.HPMax; i <= (int)EnemyStat.DropBonus; i++)
        {
            stats[i] = float.Parse(json[monsterIndex][string.Concat(statName[i], DeltaStr)].ToString());
        }

        //드랍과 관해서는 레벨에 따른 변동이 없음, 그래서 그냥 데이터 불러옴
        for (; i < 17; i++)
        {
            stats[i] = float.Parse(json[monsterIndex][statName[i]].ToString());
        }
    }

    private void TotalStatLoad()
    {
        currentHP = (int)stats[(int)EnemyStat.HPMax];

        AttackBeforeDelay = 0.3f;
    }
    //*********************************


    //애니메이션 설정 함수
    void AnimationSet(Anim anim)
    {
        this.anim.SetInteger("Now State", (int)anim);
    }

    //현재 위치 반환
    Area GetArea(Vector3 pos)
    {
        //윗 층
        if (pos.y > -4f)
        {
            //발코니
            if (pos.x > -4.5f)
                return Area.Balcony;
            //2층
            else
                return Area.Floor2;
        }
        //아래 층
        else
        {
            if (pos.x > -2.5f)
                return Area.Outer;
            else if (pos.x > -3.1f)
                return Area.BalconyUnder;
            else if (pos.x > -6.6f)
                return Area.Floor1Right;
            else
                return Area.Floor1Left;
        }
    }

    //몬스터 사망 시 호출, 아이템 드롭 결정, 몬스터 처치 수 증가
    public void ItemDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            GameManager.instance.NightM.ItemDrop(stats[(int)EnemyStat.DropRate], stats[(int)EnemyStat.DropBonus]);

            //업적 - 몬스터 처치 수 증가
            SaveManager.instance.rewardsave.EnemyKill++;

            gameObject.SetActive(false);
        }
    }
}
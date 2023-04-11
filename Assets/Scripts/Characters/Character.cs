using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
/***********************************************************************************
    캐릭터에 상속하기 위한 부모 클래스

    <변수 내용>
    - 레벨, 무기, 방어구
    - 10개의 기본 스텟(체력 - 1, 공격력 - 4, 방어력 - 1, 마법저항력 - 1, 공격속도 - 2, 이동속도 - 1)
    - 각 스텟에 관련된 총합 스텟
    - 전투 타겟 관련
    - 점프력
    - 스킬 쿨타임 및 선딜레이

    <함수 내용>
    - 스텟 이름 초기화 - 모든 캐릭터에 관하여 한 번만 실행
    - 각 캐릭터의 Attack 실행 트리거
    - 타겟과의 거리 계산과 타겟 설정
    
************************************************************************************/

public class Character : MonoBehaviour {

    //기본 정보
    //************************************
    //가변
    public int level = 1;            //레벨
    public ENUM.Color charColor;     //색깔
    public int weapon = -1;       //무기
    public int armor = -1;        //방어구

    public int assignIdx;            //전투 시 할당 된 인덱스, 데미지 표에서 사용

    //불변
    public CharClass charClass;      //직업 - 프리팹 자체에서 설정됨
    //************************************


    //스텟 관련
    //************************************
    protected float[] stats = new float[10];  //레벨에 따른 기본 스텟, json data 불러옴
    static string[] statName = new string[10];//스텟 이름, json data 불러오기 용

    public float[] totalStats = new float[10];//장비, 버프 포함 총 스텟
    public int currentHP;                     //현재 체력

    //넉백 관련 - TotalStatLoad 함수에서 설정
    public float basicKnockbackRate;
    public float skill1KnockbackRate;
    public float skill2KnockbackRate;   //분쇄자는 오버히트 강타

    //사거리 관련 - TotalStatLoad 함수에서 설정
    protected float basicAttackRange;
    protected float skill1Range;
    protected float skill2Range;     //분쇄자는 오버히트 강타
    //***********************************


    //자동 전투 타겟 관련
    //***********************************
    public Enemy currentTarget;     //현재 타겟
    protected Vector2 targetVector; //현재 타겟 방향 벡터(x축 요소만 존재)
    protected float targetDistance; //현재 타겟과의 거리
    public List<Enemy> targets;     //캐릭터와 일정 거리 내에 있는 모든 적
    //***********************************


    //booleans
    //***********************************    
    public bool isHold;         //위치 고정
    public bool isSelfHold;     //자동 전투 시 위치 고정

    public bool isJumping;      //점프

    public bool isControl;      //직접 컨트롤
    public bool isBtnAttack;    //직접 컨트롤 공격 중

    public bool isChase;        //자동 전투 - 추격 중
    public bool isAttack;       //전투 중
    public bool isGather;       //집결
    public bool isWhirlWind;    //분쇄자 - whirlwind
    //**********************************


    //포탈 관련
    //**********************************
    public bool isCanTeleport;
    public bool isCanTeleportLeft = true;
    public Vector3 teleportPos = new Vector3();

    public Area nowArea;
    //**********************************


    //스킬 관련
    //**********************************
    public float basicCooldown;
    public float[] cooldowns = new float[2];

    public float[] cooldownStart = new float[2];

    public Vector2 moveVector;
    //**********************************

    //체력 바 관련
    //**********************************
    public Slider hpBar;
    public GameObject hpBarPos;
    public GameObject arrowPos;
    //**********************************

    //층
    //**********************************
    public Transform PortalPos;         //2층 포탈의 위치, SelfHold 시 이용
    //**********************************

    //추적 코루틴
    public Coroutine chaseCoroutine;
    public Coroutine gatherCoroutine;

    //애니메이션 관련
    //**********************************
    protected Animator charAnim;

    public Animator[] animArray;    //모든 색깔 animator들
    //**********************************

    //직업별 관련
    //**********************************
    public bool isShieldOn;     //수호자 실드 온오프 관련

    bool isBuffOn;              //연금술사 버프 온오프
    float buffTime;             //연금술사 버프 시간

    public bool isMilkyway;     //천문학자 은하수 온오프
    //**********************************



    private void Update()
    {
        nowArea = GetArea(transform.position);

        if (hpBar)
        {
            hpBar.value = (float)currentHP / totalStats[(int)CharStat.HPMax];
            hpBar.transform.position = hpBarPos.transform.position;
        }
    }

    public void StatnameInitialization()
    {
        statName[0] = "MaxHP";
        statName[1] = "AttackMin";
        statName[2] = "AttackMax";
        statName[3] = "ClawAttackMin";
        statName[4] = "ClawAttackMax";
        statName[5] = "PhysicalDefense";
        statName[6] = "MagicDefense";
        statName[7] = "AttackSpeed";
        statName[8] = "ClawAttackSpeed";
        statName[9] = "MovementSpeed";
    }

    //가상 함수
    //***************************************
    //각 캐릭터의 공격을 위한 가상 함수 - 모든 캐릭터 override
    public virtual void AttackMain()
    {

    }

    //각 캐릭터의 스킬 1을 위한 가상 함수 - 모든 캐릭터 override
    public virtual void Skill1()
    {

    }

    //각 캐릭터의 스킬 2를 위한 가상 함수 - 모든 캐릭터 override
    public virtual void Skill2()
    {

    }

    //각 캐릭터의 버튼을 통한 직접 공격을 위한 가상 함수 - 모든 캐릭터 override
    public virtual void AttackBtn()
    {

    }

    //스킬 관련 오브젝트 생성 함수 - Hero, Chaser, Alchemist, Astronomer override
    public virtual void CharInstantiate(int idx)
    {
        if (!PortalPos)
            PortalPos = GameObject.FindWithTag("Portal2").transform;

        assignIdx = idx;
    }
    
    //기본 공격 쿨타임 함수 - 버튼 이미지 적용을 위해 - Chaser override
    protected virtual IEnumerator AttackCooldown()
    {
        basicCooldown = 1 / totalStats[(int)CharStat.AttackSpeed];

        while (basicCooldown > 0)
        {
            basicCooldown -= 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        basicCooldown = 0;
    }

    //매 스테이지 시작마다 상태 초기화 - 모든 캐릭터 override
    public virtual void BoolInit()
    {
        targets.Clear();
        isHold = isSelfHold = isJumping = isChase = isAttack = isGather = isWhirlWind = isBtnAttack = false;
    }

    //천문학자용 - 충전 완료
    public virtual void ChargeEnd()
    {

    }

    //천문학자용 - skill2 완료
    public virtual void MilkyWayEnd()
    {

    }

    //현재 이동 가능한 지 반환하는 함수
    public virtual bool CanMove()
    {
        return (!isHold && !isSelfHold);
    }

    //피해를 받는 함수 - Guardian override
    public virtual void GetDamage(int damage, bool type)
    {
        //물리, 마법 타입 구분 : 물리 : true, 마법 : false
        if (type)
            damage -= (int)totalStats[(int)CharStat.PhysicalDefense];
        else
            damage -= (int)totalStats[(int)CharStat.MagicDefense];

        if (damage < 1)
            damage = 1;

        currentHP -= damage;
    }

    //캐릭터 스탯 로드 - 모든 캐릭터 override
    public virtual void TotalStatLoad()
    {

    }
    //***************************************

    //애니메이션 관련
    //***************************************
    //캐릭터 애니메이션 설정 함수 - Hero override
    public virtual void AnimationSet(Anim anim)
    {
       charAnim.SetInteger("Now State", (int)anim);
    }

    public virtual void AnimationSet(string name, int value)
    {
        charAnim.SetInteger(name, value);
    }

    public virtual void AnimationSet(string name, bool b)
    {
        charAnim.SetBool(name, b);
    }

    public virtual void AnimationColorSet()
    {
        if (charClass == CharClass.Dummy)
            return;

        for (int i = 0; i < 8; i++)
            if ((int)charColor / 5 == i)
            {
                animArray[i].gameObject.SetActive(true);
                charAnim = animArray[i];
            }
            else
                animArray[i].gameObject.SetActive(false);
    }
    //***************************************


    //점프 + 텔레포트
    //***************************************

    //점프 + DownStair 텔레포트 수행
    public void Jump()
    {
        if (isCanTeleport)
            Teleport();
        else
        {
            if (!isJumping)
            {
                if (GetComponent<Rigidbody2D>().velocity.y < 0.01f)
                {
                    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 6), ForceMode2D.Impulse);
                    isJumping = true;
                }
            }
        }
    }
    
    public void Teleport()
    {
        if (isCanTeleport)
        {
            isCanTeleportLeft = false;
            transform.position = teleportPos;
        }
    }

    //집결을 통한 SelfHold - 자동 전투에서 포탈 이용 시
    public void TeleportHold()
    {
        isSelfHold = true;
        Gather(PortalPos.position + new Vector3(-basicAttackRange, 0, 0));
        StartCoroutine(SelfHold());
    }

    //적이 올라올 때까지 대기
    private IEnumerator SelfHold()
    {
        //적이 2층에 올라올 때까지 대기
        while ((!currentTarget || (int)currentTarget.nowArea < 4) && !isControl)
        {
            SetTargetByDistance();
            yield return new WaitForSeconds(0.1f);
        }

        //적이 2층에 올라오면 공격 재개
        isSelfHold = false;
        StopAction();
        AnimationSet(Anim.Idle);
        AttackMain();
    }

    //점프 착지 관련
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform" || collision.gameObject.tag == "Balcony")
        {
            isJumping = false;
        }
    }

    //점프 시작 관련
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            if (gameObject.GetComponent<Rigidbody2D>().velocity.y != 0)
            {
                isJumping = true;
            }
        }
    }

    //발코니에서 점프 중일 때, 이미 발코니를 지난 경우 다시 충돌을 하게 설정
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Balcony")
        {
            GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    //버그 추락 방지 - 바닥과 충돌 시 다시 충돌체 켜기
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Platform")
            GetComponent<BoxCollider2D>().isTrigger = false;
    }
    //***************************************

    //타겟 설정
    //*****************************************
    //거리에 따른 타겟 설정 - targets[0]이 가장 가까운 타겟
    protected void SetTargetByDistance()
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
    protected void SetTargetDistance()  //타겟과의 거리와 방향 결정
    {
        if (currentTarget != null)
        {
            targetVector = (currentTarget.gameObject.transform.position - gameObject.transform.position);
            targetDistance = Vector2.Distance(gameObject.transform.position, currentTarget.gameObject.transform.position);
            targetVector.y = 0;

            targetVector.Normalize();
        }
    }

    //targets 정렬용
    int Compare(Enemy e1, Enemy e2)
    {
        float d1 = Vector3.Distance(transform.position, e1.transform.position);
        float d2 = Vector3.Distance(transform.position, e2.transform.position);

        if (d1 < d2)
            return -1;
        else if (d1 == d2)
            return 0;
        else
            return 1;
    }

    //대상 타겟과 같은 층에 있는 지 반환
    protected bool IsEqualFloor(Enemy target)
    {
        return ((int)nowArea < 4 && (int)target.nowArea < 4) || ((int)nowArea >= 4 && (int)target.nowArea >= 4);
    }
    //*****************************************


    //집결
    //*****************************************
    //모든 행동을 멈추는 함수(추격, 공격)
    public void StopAction()
    {
        isAttack = false;

        if (isChase)
        {
            StopCoroutine(chaseCoroutine);
            isChase = false;
        }
    }

    //현재 위치 반환
    protected Area GetArea(Vector3 pos)
    {
        //윗 층
        if(pos.y > -4f)
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

    //집결을 실행하는 함수
    public void Gather(Vector2 pos)
    {
        if (isGather || isHold || isShieldOn)
            return;
        
        isGather = true;
        StopAction();
        Area dest = GetArea(pos);

        StopAction();

        gatherCoroutine = StartCoroutine(GatherMove(dest, pos.x));
    }

    //집결 이동 코루틴
    private IEnumerator GatherMove(Area dest, float xPos)
    {
        const float ERROR_MAX = 0.15f;

        Vector3 moveVector = new Vector3();
        AnimationSet(Anim.Move);

        while (true)
        {
            float error = xPos - transform.position.x;

            //밖
            if (nowArea == Area.Outer)
            {
                //밖 -> 밖, 발코니 아래, 1층 오른쪽 : 직선 움직임
                if ((int)dest < 3)
                {
                    if (error > ERROR_MAX)
                        moveVector.Set(1, 0, 0);
                    else if (error < -ERROR_MAX)
                        moveVector.Set(-1, 0, 0);
                    else
                        break;
                }
                //밖 -> 1층 왼쪽, 발코니, 2층 : 발코니 아래로 이동(왼쪽)
                else
                {
                    moveVector.Set(-1, 0, 0);
                }
            }
            //발코니 아래
            else if (nowArea == Area.BalconyUnder)
            {
                //발코니 아래 -> 밖, 발코니 아래, 1층 오른쪽 : 직선 움직임
                if ((int)dest < 3)
                {
                    if (error > ERROR_MAX)
                        moveVector.Set(1, 0, 0);
                    else if (error < -ERROR_MAX)
                        moveVector.Set(-1, 0, 0);
                    else
                        break;
                }
                //발코니 아래 -> 1층 왼쪽, 발코니, 2층 : 점프
                else
                {
                    Jump();
                }
            }
            //1층 오른쪽
            else if (nowArea == Area.Floor1Right)
            {
                //1층 오른쪽 -> 밖, 발코니 아래, 1층 오른쪽, 1층 왼쪽 : 직선 움직임
                if ((int)dest < 4)
                {
                    if (error > ERROR_MAX)
                        moveVector.Set(1, 0, 0);
                    else if (error < -ERROR_MAX)
                        moveVector.Set(-1, 0, 0);
                    else
                        break;
                }
                //1층 오른쪽 -> 발코니, 2층 : 발코니 아래로 이동(오른쪽)
                else
                {
                    moveVector.Set(1, 0, 0);
                }
            }
            //1층 왼쪽
            else if(nowArea == Area.Floor1Left)
            {
                //1층 왼쪽 -> 밖(0), 발코니 아래(1), 발코니(4), 2층(5) : 텔레포트 타기
                if(dest != Area.Floor1Right && dest != Area.Floor1Left)
                {
                    if (!isCanTeleportLeft)
                        moveVector.Set(1, 0, 0);
                    else
                        moveVector.Set(-1, 0, 0);
                }
                //1층 왼쪽 -> 1층 오른쪽, 1층 왼쪽 : 직선 움직임
                else
                {
                    if (error > ERROR_MAX)
                        moveVector.Set(1, 0, 0);
                    else if (error < -ERROR_MAX)
                        moveVector.Set(-1, 0, 0);
                    else
                        break;
                }
            }
            //발코니
            else if(nowArea == Area.Balcony)
            {
                //발코니 -> 밖(0), 발코니 아래(1), 1층 오른쪽(2) : 떨어지기(오른쪽)
                if ((int)dest < 3)
                    moveVector.Set(1, 0, 0);
                //발코니 -> 1층 왼쪽 : 왼쪽 텔레포트 이용
                else if (dest == Area.Floor1Left)
                    if (isCanTeleport)
                        Teleport();
                    else
                        moveVector.Set(-1, 0, 0);
                //발코니 -> 발코니, 2층 : 직선 움직임
                else
                {
                    if (error > ERROR_MAX)
                        moveVector.Set(1, 0, 0);
                    else if (error < -ERROR_MAX)
                        moveVector.Set(-1, 0, 0);
                    else
                        break;
                }
            }
            //2층
            else
            {
                //2층 -> 밖, 발코니 아래, 1층 오른쪽 : 발코니로 이동
                if((int)dest < 3)
                {
                    moveVector.Set(1, 0, 0);
                }
                //2층 -> 1층 왼쪽 : 텔레포트 타기
                else if(dest == Area.Floor1Left)
                {
                    if (isCanTeleport)
                        Teleport();
                    else
                        moveVector.Set(1, 0, 0);
                }
                //2층 -> 발코니, 2층 : 직선 움직임
                else
                {
                    if (error > ERROR_MAX)
                        moveVector.Set(1, 0, 0);
                    else if (error < -ERROR_MAX)
                        moveVector.Set(-1, 0, 0);
                    else
                        break;
                }
            }
            
            Move(moveVector);
            
            yield return new WaitForSeconds(0.03f);
        }

        AnimationSet(Anim.Idle);
        isGather = false;
        AttackMain();
    }

    //집결을 멈추는 함수
    public void StopGather()
    {
        if (isGather)
        {
            StopCoroutine(gatherCoroutine);
            isGather = false;

            AttackMain();
        }
    }
    //*****************************************

        
    //캐릭터 스탯 로드
    public void BasicStatLoad()
    {
        string path = "Stats/";
        if (statName[0] != "MaxHP")
            StatnameInitialization();

        if (level <= 0)
            return;

        TextAsset txtAsset;
        string loadStr;
        JsonData json;

        switch (charClass)
        {
            case CharClass.Hero:
                path = string.Concat(path, "HeroStat");
                break;
            case CharClass.Chaser:
                path = string.Concat(path, "ChaserStat");
                break;
            case CharClass.Crusher:
                path = string.Concat(path, "CrusherStat");
                break;
            case CharClass.Guardian:
                path = string.Concat(path, "GuardianStat");
                break;
            case CharClass.Astronomer:
                path = string.Concat(path, "AstronomerStat");
                break;
            case CharClass.Alchemist:
                path = string.Concat(path, "AlchemistStat");
                break;
            default:
                return;
        }

        txtAsset = Resources.Load<TextAsset>(path);
        loadStr = txtAsset.text;
        json = JsonMapper.ToObject(loadStr);

        for (int i = 0; i < 10; i++)
            stats[i] = float.Parse(json[level - 1][statName[i]].ToString());
    }

    //캐릭터가 죽었는 지 반환하는 함수
    public bool DeathCheck()
    {
        if (currentHP <= 0)
        {
            currentHP = 0;
            if (charClass != CharClass.Dummy && charClass != CharClass.Spirit)
                hpBar.gameObject.SetActive(false);

            return true;
        }

        return false;
    }

    //이동하는 함수
    public void Move(Vector3 vec)
    {
        transform.localScale = new Vector3(vec.x, 1, 1);
        transform.Translate(vec * totalStats[(int)CharStat.MovementSpeed] * 0.003f);
    }

    //타겟 방향 보기
    public void SeeTarget()
    {
        if (currentTarget)
            transform.localScale = new Vector3(targetVector.x, 1, 1);
    }

    //버프
    //*****************************************
    //연금술사 신비한 가루 공격력 버프
    public void DamageBuff(Alchemist al)
    {
        int buff = (int)(0.2f * Random.Range(al.totalStats[(int)CharStat.AttackMin], al.totalStats[(int)CharStat.AttackMax] + 1));

        buffTime = 5f;

        if (!isBuffOn)
            StartCoroutine(DamageBuffCoroutine(buff));
    }

    //연금술사 공격력 버프 코루틴
    IEnumerator DamageBuffCoroutine(int buff)
    {
        if(!isBuffOn)
        {
            isBuffOn = true;
            totalStats[(int)CharStat.AttackMin] += buff;
            totalStats[(int)CharStat.AttackMax] += buff;

            while(buffTime > 0)
            {
                buffTime -= 0.5f;
                yield return new WaitForSeconds(0.5f);
            }

            isBuffOn = false;
            totalStats[(int)CharStat.AttackMin] -= buff;
            totalStats[(int)CharStat.AttackMax] -= buff;
        }
    }
    //*****************************************
}

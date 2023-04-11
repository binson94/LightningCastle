using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***********************************************************************************
    연금술사 클래스
    
************************************************************************************/

public class Alchemist : Character
{
    
    //기본 공격 관련
    //*******************************
    public Vector2 shotVector;                  //사격할 방향 벡터
    public float shotSpeed = 3.5f;              //투사체 속도
    public GameObject bottlePrefab;             //약병 프리팹
    Potion[] bottles = new Potion[5];           //약병 인스턴스
    int nowBottle;                              //현재 사용 중인 약병 index
    const int BOTTLE_MAX = 5;

    public Transform shotPos;                   //총알 발사 시 출발 위치
    //*******************************

    //패시브 관련
    //*******************************
    bool isPassiveOn;
    public float shoeWithWingsTime;               //패시브 지속시간
    //*******************************

    //스킬 1 관련
    //*******************************
    public GameObject dustPrefab;               //신비한 가루 프리팹
    Dust dustInstance;                          //신비한 가루 인스턴스
    //*******************************

    //*******************************
    public GameObject spiritPrefab;          //정령 프리팹
    Spirit spiritInstance;                //정령 인스턴스
    //*******************************
    
    //공격 메인 함수
    public override void AttackMain()
    {
        //중복 호출 방지
        if (!isControl && !isAttack && !isGather)
        {
            //애니메이션 설정
            AnimationSet("Now State", (int)Anim.Idle);

            AttackStart();
        }
    }

    //타겟 설정 및 Chase 코루틴 호출
    private void AttackStart()
    {
        //중복 호출 방지
        if (!isControl && !isAttack)
        {
            //타겟 설정
            SetTargetByDistance();
            
            //타겟이 존재
            if (currentTarget != null)
            {
                isAttack = true;
                if (isActiveAndEnabled)
                    chaseCoroutine = StartCoroutine(ChaseAndAttack());
            }
            //타겟이 없음 -> 리스트 비어있음 -> 함수 끝
            else
            {
                isAttack = false;
            }
        }
    }

    //접근 후 공격
    private IEnumerator ChaseAndAttack()
    {
        //중복 호출 방지
        if (!isChase)
        {
            isChase = true;

            //애니메이션 설정
            AnimationSet(Anim.Move);

            //접근
            while (targetDistance > basicAttackRange)
            {
                //홀드 중 아님
                if (CanMove())
                {
                    Move(targetVector);
                }

                SetTargetByDistance();

                //타겟이 없음 -> 리스트 비어있음 -> 함수 끝
                if (currentTarget == null)
                {
                    isAttack = isChase = false;
                    yield break;
                }

                yield return new WaitForSeconds(0.03f);
            }
            isChase = false;

            SeeTarget();
            AnimationSet(Anim.Attack);
        }
    }

    //사격 벡터 결정
    public void SetShotVector()
    {
        if (currentTarget != null)
        {
            if (currentTarget.isChase)
            {
                Vector2 tempVector;
                float i;
                for (i = 0; i < 12; i += 0.03f)
                {
                    tempVector = currentTarget.transform.position - shotPos.position;

                    //적의 이동 방향으로 목적 Vector 보정
                    tempVector += i * currentTarget.stats[(int)EnemyStat.MoveSpeed] * 0.1f * currentTarget.targetVector;

                    //보정 성공 - 오차가 +- 0.07 이하
                    if (((shotSpeed * i + 0.07f) > Vector2.Distance(tempVector, new Vector2(0, 0)))
                        && ((shotSpeed * i - 0.07f) < Vector2.Distance(tempVector, new Vector2(0, 0))))
                    {
                        shotVector = new Vector2(currentTarget.transform.position.x, currentTarget.transform.position.y)
                                    + i * 0.1f * currentTarget.stats[(int)EnemyStat.MoveSpeed] * currentTarget.targetVector
                                    - new Vector2(shotPos.position.x, shotPos.position.y);
                        break;
                    }
                }

                //보정 실패 -> 그냥 발사
                if (i >= 12)
                {
                    shotVector.Set(currentTarget.transform.position.x - shotPos.position.x, currentTarget.transform.position.y - shotPos.position.y);
                }
            }
            //적이 이동 중이 아님 -> 그냥 발사
            else
            {
                shotVector.Set(currentTarget.transform.position.x - shotPos.position.x, currentTarget.transform.position.y - shotPos.position.y);
            }
        }
        //타겟이 없음 -> 앞으로 발사
        else
        {
            shotVector.x = transform.localScale.x;
            shotVector.y = 0;
        }
    }

    //사격
    public void Shot()
    {
        //총알 위치 설정
        bottles[nowBottle].transform.position = shotPos.position;

        //총알 활성화
        bottles[nowBottle].gameObject.SetActive(true);
        bottles[nowBottle].moveVector = shotVector;
        bottles[nowBottle].PotionStart(basicAttackRange);

        //다음 총알 설정
        nowBottle = (nowBottle + 1) % 5;
    }

    //공격 후딜레이
    public IEnumerator AttackReady()
    {
        if (isControl)
            yield break;


        float i;
        //딜레이의 1 / 4는 움직이지 못함
        StartCoroutine(AttackCooldown());
        yield return new WaitForSeconds(i = 1 / (4 * totalStats[(int)CharStat.AttackSpeed]));

        //딜레이의 3 / 4는 추적 또는 카이팅
        while (basicCooldown > 0)
        {
            if (currentTarget != null)
            {
                if (CanMove())
                {
                    //타겟이 사거리보다 멀리 있음 -> 추적
                    if (targetDistance - basicAttackRange > 0.1f)
                    {
                        AnimationSet(Anim.Move);
                        Move(targetVector);
                    }
                    //타겟이 사거리보다 가까이 있음 -> 후퇴
                    else if (targetDistance - basicAttackRange < -0.1f)
                    {
                        //나는 2층, 타겟은 1층인 경우에는 움직이지 않음
                        if (!((int)nowArea > 3 && currentTarget && (int)currentTarget.nowArea < 4))
                        {
                            AnimationSet(Anim.Move);
                            Move(-targetVector);
                        }
                    }
                    //타겟이 사거리 즈음 있음 -> 가만히 있음
                    else
                    {
                        AnimationSet(Anim.Idle);
                    }
                }
                //홀드 중 -> 가만히 있음
                else
                {
                    AnimationSet(Anim.Idle);
                }

                SetTargetByDistance();
            }
            //타겟이 없음 -> 가만히 있음
            else
            {
                AnimationSet(Anim.Idle);
            }

            yield return new WaitForSeconds(0.03f);
        }

        isAttack = false;
        AttackMain();
    }

    //버튼을 통한 공격 실행
    public override void AttackBtn()
    {
        if (!isBtnAttack && !isAttack && basicCooldown <= 0)
        {
            SetTargetByDistance();

            //사격
            isAttack = true;
            isBtnAttack = true;

            SeeTarget();
            AnimationSet(Anim.Attack);
        }
    }

    //버튼을 통한 공격 후딜레이
    public IEnumerator BtnAttackReady()
    {
        StartCoroutine(AttackCooldown());
        yield return new WaitForSeconds(1 / (4 * totalStats[(int)CharStat.AttackSpeed]));
        isAttack = false;
        yield return new WaitForSeconds(3 / (4 * totalStats[(int)CharStat.AttackSpeed]));
        isBtnAttack = false;
    }

    //패시브
    //공격 타격 등 패시브 켜는 상황에 호출
    //*******************************************
    public void PassiveUpdate()
    {
        shoeWithWingsTime = 2f;

        if (!isPassiveOn)
            StartCoroutine(ShoeWithWings());
    }

    //패시브 - 이동속도 증가
    IEnumerator ShoeWithWings()
    {
        if (!isPassiveOn)
        {
            isPassiveOn = true;
            totalStats[(int)CharStat.MovementSpeed] = stats[(int)CharStat.MovementSpeed] * 1.3f;

            while (shoeWithWingsTime > 0)
            {
                shoeWithWingsTime -= 0.05f;
                yield return new WaitForSeconds(0.05f);
            }

            shoeWithWingsTime = 0;
            totalStats[(int)CharStat.MovementSpeed] = stats[(int)CharStat.MovementSpeed];
            isPassiveOn = false;
        }
    }
    //*******************************************


    //직접 컨트롤 시 Skill 버튼과 연동
    //*******************************************
    public override void Skill1()
    {
        if (isAttack)
            return;

        //홀드 중 아님
        if (isActiveAndEnabled)
            StartCoroutine(MysteriousDust());
    }

    //Skill 1 - 신비한 가루
    IEnumerator MysteriousDust()
    {
        if(!isAttack && cooldowns[0] <= 0)
        {
            isAttack = true;
            SetTargetByDistance();

            //타겟 있으면 타겟 방향으로 발사
            if (currentTarget != null)
                dustInstance.moveVector = targetVector;
            //타겟 없으면 앞으로 발사
            else
                dustInstance.moveVector.Set(transform.localScale.x, 0);

            SeeTarget();
            AnimationSet(Anim.Skill1);

            for (cooldowns[0] = cooldownStart[0]; cooldowns[0] > 0; cooldowns[0] -= 0.05f)
            {
                yield return new WaitForSeconds(0.05f);
            }
            cooldowns[0] = 0;
        }
    }

    //신비한 가루 투척 함수 - 애니메이션 직접 호출
    public void DustThrow()
    {
        dustInstance.transform.position = transform.position;
        dustInstance.gameObject.SetActive(true);
        dustInstance.DustStart(skill1Range);
    }

    //신비한 가루 모션 종료 - 애니메이션 직접 호출
    public void DustEnd()
    {
        isAttack = false;
        AnimationSet("Now State", (int)Anim.Idle);
        AttackMain();
    }
    //******************************************


    //직접 컨트롤 시 Skill 버튼과 연동
    //*******************************************
    public override void Skill2()
    {
        if (isAttack)
            return;

        if (isActiveAndEnabled)
            StartCoroutine(SpiritOfLight());
    }

    //Skiil 2 - 빛의 정령 소환
    IEnumerator SpiritOfLight()
    {
        if(!isAttack && cooldowns[1] <= 0)
        {
            isAttack = true;

            SeeTarget();
            AnimationSet(Anim.Skill2);

            for (cooldowns[1] = cooldownStart[1]; cooldowns[1] > 0; cooldowns[1] -= 0.05f)
                yield return new WaitForSeconds(0.05f);
            cooldowns[1] = 0;
        }
    }

    //정령 소환 함수 - 애니메이션 직접 호출
    public void SommonSpirit()
    {
        spiritInstance.targets.Clear();
        spiritInstance.transform.position = transform.position + new Vector3(0.5f * transform.localScale.x, 0, 0);

        spiritInstance.gameObject.SetActive(true);
        spiritInstance.SpiritStart();
    }

    //정령 소환 모션 종료 - 애니메이션 직접 호출
    public void SommonEnd()
    {
        isAttack = false;
        AnimationSet("Now State", (int)Anim.Idle);
        AttackMain();
    }
    //*******************************************

    //총 스텟 로드
    public override void TotalStatLoad()
    {
        for (int i = 0; i < 10; i++)
            totalStats[i] = stats[i];

        currentHP = (int)totalStats[(int)CharStat.HPMax];

        basicAttackRange = 3.5f;
        basicKnockbackRate = 0.5f;

        skill1Range = 3.75f;
        skill2Range = 0.5f;

        basicCooldown = 0;
        cooldowns[0] = cooldowns[1] = 0;
        cooldownStart[0] = 6f;
        cooldownStart[1] = 20f;
    }

    //캐릭터와 관련된 오브젝트 생성 - 병, 가루, 정령
    public override void CharInstantiate(int idx)
    {
        if (!PortalPos)
            PortalPos = GameObject.FindWithTag("Portal2").transform;

        //기본 공격 - 병 생성
        //**************************************************************
        for (nowBottle = 0; nowBottle < BOTTLE_MAX; nowBottle++)
        {
            bottles[nowBottle] = Instantiate(bottlePrefab).GetComponent<Potion>();
            bottles[nowBottle].alchemist = this;
            bottles[nowBottle].AnimationColorSet();

            bottles[nowBottle].gameObject.SetActive(false);
        }

        nowBottle = 0;
        //**************************************************************

        //skill 1 - 가루 생성
        //************************************************************
        dustInstance = Instantiate(dustPrefab).GetComponent<Dust>();
        dustInstance.alchemist = this;
        dustInstance.gameObject.SetActive(false);
        //************************************************************

        //skill 2 - 정령 생성
        //***************************************************
        spiritInstance = Instantiate(spiritPrefab).GetComponent<Spirit>();
        spiritInstance.alchmist = this;
        spiritInstance.charColor = charColor;
        spiritInstance.AnimationColorSet();
        spiritInstance.gameObject.SetActive(false);
        //***************************************************

        assignIdx = idx;
    }

    public override void BoolInit()
    {
        isPassiveOn = false;
        shoeWithWingsTime = 0;
        base.BoolInit();
    }
}
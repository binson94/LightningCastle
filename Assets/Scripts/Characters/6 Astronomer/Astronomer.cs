using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astronomer : Character
{
    //모든 스킬 공통
    //*******************************
    bool isCharge;                  //충전 중
    public Transform shotPos;       //투사체 시작 위치
    public Vector2 shotVector;             //사격 방향

    float charge;
    //*******************************

    //기본 공격 관련
    //*******************************
    public GameObject moonPrefab;   //기본공격 투사체 프리팹
    Moon[] moons = new Moon[5];     //기본공격 인스턴스
    int moonIdx;
    const int MOON_MAX = 5;

    float shotSpeed;                //투사체 속도
    //*******************************

    //skill 1 관련
    //*******************************
    public GameObject meteorPrefab; //유성 투사체 프리팹
    Meteor meteorInstance;          //유성 인스턴스
    //*******************************


    //skill 2 관련
    //********************************
    Coroutine milkywayCoroutine;
    float milkywayTime;
    public LineRenderer milkywayEffect;
    public Transform milkywayStartpos;
    //********************************

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
                    AnimationSet(Anim.Idle);
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
        moons[moonIdx].transform.position = shotPos.position;

        //총알 활성화
        moons[moonIdx].targetVector = shotVector.normalized;
        moons[moonIdx].gameObject.SetActive(true);
        moons[moonIdx].MoonStart(charge, basicAttackRange);

        //차지 종료
        charge = 0;
        AnimationSet("ChargeEnd", 0);

        //다음 총알 설정
        moonIdx = (moonIdx + 1) % MOON_MAX;
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
                        if (!((int)nowArea > 3 && (int)currentTarget.nowArea < 4))
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
        AnimationSet(Anim.Idle);

        isAttack = false;
        yield return new WaitForSeconds(3 / (4 * totalStats[(int)CharStat.AttackSpeed]));
        isBtnAttack = false;
    }


    //충전 관련
    //******************************************
    public void ChargeStart()
    {
        if (!isCharge && isActiveAndEnabled)
            StartCoroutine(Charge());
    }

    IEnumerator Charge()
    {
        if(!isCharge)
        {
            isCharge = true;
            charge = 0;

            while (isCharge)
            {
                charge = Mathf.Min(charge + 0.05f, 2);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    //충전 완료
    public override void ChargeEnd()
    {
        AnimationSet("ChargeEnd", 1);

        if(isMilkyway)
        {
            StopCoroutine(milkywayCoroutine);

            isAttack = false;
            isMilkyway = false;
        }

        isCharge = false;
    }
    //******************************************


    //직접 컨트롤 시 Skill 버튼과 연동
    //*******************************************
    public override void Skill1()
    {
        if (isAttack)
            return;

        //홀드 중 아님
        if (isActiveAndEnabled)
            StartCoroutine(Meteor());
    }

    IEnumerator Meteor()
    {
        if(!isAttack && cooldowns[0] <= 0)
        {
            SetTargetByDistance();

            //if(currentTarget != null && targetDistance < skill1Range)
            {
                isAttack = true;

                SeeTarget();
                AnimationSet(Anim.Skill1);

                for (cooldowns[0] = cooldownStart[0]; cooldowns[0] > 0; cooldowns[0] -= 0.05f)
                    yield return new WaitForSeconds(0.05f);
            }
        }
    }

    //유성우 투사체 발사
    public void MeteorShot()
    {
        Vector3 pos;
        if (currentTarget)
            pos = currentTarget.transform.position;
        else
            pos = transform.position;
        
        meteorInstance.targetPos = pos;
        meteorInstance.transform.position = transform.position + new Vector3(0, 4.32f, 0);
        meteorInstance.moveVector = (pos - meteorInstance.transform.position).normalized;

        meteorInstance.gameObject.SetActive(true);
        meteorInstance.MeteorStart(charge);
       

        charge = 0;
        AnimationSet("ChargeEnd", 0);
    }

    //유성우 모션 종료
    public void MeteorEnd()
    {
        isAttack = false;
        AnimationSet(Anim.Idle);
    }
    //******************************************


    //직접 컨트롤 시 Skill 버튼과 연동
    //*******************************************
    public override void Skill2()
    {
        if (isActiveAndEnabled)
            StartCoroutine(MilkyWay());
    }

    IEnumerator MilkyWay()
    {
        if(!isAttack && cooldowns[1] <= 0)
        {
            isAttack = true;
            milkywayCoroutine = null;
            isMilkyway = true;

            SetTargetByDistance();
            SeeTarget();
            AnimationSet(Anim.Skill2);
            
            for (cooldowns[1] = cooldownStart[1]; cooldowns[1] > 0; cooldowns[1] -= 0.05f)
                yield return new WaitForSeconds(0.05f);
            cooldowns[1] = 0;
        }
    }

    public void MilkyWayStart()
    {
        milkywayCoroutine = StartCoroutine(MilkyWayAttack());
    }

    IEnumerator MilkyWayAttack()
    {
        bool hit;
        isCharge = true;
        charge = 0;

        milkywayEffect.startColor = Color.red;
        milkywayEffect.endColor = Color.blue;
        milkywayEffect.SetPosition(0, milkywayStartpos.position);
        milkywayEffect.SetPosition(1, milkywayStartpos.position);
        milkywayEffect.gameObject.SetActive(true);

        while(charge < 2)
        {
            hit = false;
            SetTargetByDistance();

            //x축과 평행한 직선 상 가장 가까운 타겟이 사거리 내에 있는 지 체크 후 피해입힘
            for (int i = 0; i < targets.Count; i++)
                if(IsEqualFloor(targets[i]) && Vector3.Distance(transform.position, targets[i].transform.position) <= 6.5f)
                {
                    milkywayEffect.SetPosition(1, new Vector3(targets[i].transform.position.x, milkywayStartpos.position.y, 0));
                    targets[i].GetDamage((int)(0.5f * Random.Range(totalStats[(int)CharStat.AttackMin], totalStats[(int)CharStat.AttackMax] + 1)),
                        false, assignIdx);
                    hit = true;
                    break;
                }

            //타겟이 없음 -> 이펙트만 표시
            if (!hit)
                milkywayEffect.SetPosition(1, milkywayStartpos.position + new Vector3(6.5f * transform.localScale.x, 0, 0));

            Debug.Log("milkyway hit");
            charge += 0.125f;
            yield return new WaitForSeconds(0.125f);
        }

        milkywayEffect.gameObject.SetActive(false);
        isCharge = false;
        MilkyWayEnd();
    }
    
    public override void MilkyWayEnd()
    {
        if (isMilkyway)
        {
            if (milkywayCoroutine != null)
                StopCoroutine(milkywayCoroutine);

            isAttack = isMilkyway = false;
            AnimationSet(Anim.Idle);
        }
    }
    //*******************************************

    //총 스텟 로드
    public override void TotalStatLoad()
    {
        for (int i = 0; i < 10; i++)
            totalStats[i] = stats[i];

        currentHP = (int)totalStats[(int)CharStat.HPMax];

        basicAttackRange = 4.5f;
        basicKnockbackRate = 0.9f;

        skill1Range = 5f;
        skill2Range = 6.5f;

        basicCooldown = 0;
        cooldowns[0] = cooldowns[1] = 0;
        cooldownStart[0] = 10f;
        cooldownStart[1] = 12f;
    }

    //캐릭터와 관련된 오브젝트 생성 - 병, 가루, 정령
    public override void CharInstantiate(int idx)
    {
        if (!PortalPos)
            PortalPos = GameObject.FindWithTag("Portal2").transform;

        //기본 공격 - 달 생성
        //**************************************************************
        for (int i = 0; i < MOON_MAX; i++)
        {
            moons[i] = Instantiate(moonPrefab).GetComponent<Moon>();
            moons[i].astronomer = this;

            moons[i].gameObject.SetActive(false);
        }
        //**************************************************************

        meteorInstance = Instantiate(meteorPrefab).GetComponent<Meteor>();
        meteorInstance.astronomer = this;
        meteorInstance.gameObject.SetActive(false);

        assignIdx = idx;
    }

    public override void BoolInit()
    {
        isMilkyway = false;
        base.BoolInit();
    }
}

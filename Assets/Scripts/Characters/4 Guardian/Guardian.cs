using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/***********************************************************************************
    수호자 클래스
    
    
************************************************************************************/

public class Guardian : Character
{
    //보호막 게이지 관련
    //********************************
    public Slider shieldBar;
    public GameObject shieldBarPos;
    //********************************

    //패시브 관련
    //********************************
    public float justice;             //정의 게이지
    public float justiceMax = 100;    //정의 게이지 최대치
    float justiceTime;                //정의 게이지 감소 선딜레이, 피해 입을 시 3으로 초기화
    //********************************

    //기본 공격 - 방패 들기
    //********************************
    int currentShield;
    int totalShield;

    public Coroutine shieldCoroutine = null; //쉴드 회복 코루틴
    //********************************

    //Skill 1 - Scourge
    //********************************
    //********************************

    //Skill 2 - Bravery
    //********************************
    List<GameObject> braveryList = new List<GameObject>(); //용기 사용 시 밀칠 적 리스트
    public bool isBravery;          //용기 사용 중 여부
    //********************************


    //업데이트 함수 - 체력 바, 쉴드 바 위치 잡기
    private void Update()
    {
        nowArea = GetArea(transform.position);

        if (hpBar)
        {
            hpBar.value = (float)currentHP / totalStats[(int)CharStat.HPMax];
            hpBar.transform.position = hpBarPos.transform.position;
        }
        if (shieldBar)
        {
            shieldBar.value = (float)currentShield / totalShield;
            shieldBar.transform.position = shieldBarPos.transform.position;
        }
    }


    //전투 관련
    //************************************
    //공격 메인 함수
    public override void AttackMain()
    {
        //직접 컨트롤 중이 아닐 때에만 실행
        if (!isControl)
        {
            if (!isAttack)
            {
                if (!isShieldOn)
                {
                    AnimationSet(Anim.Idle);

                    AttackStart();
                }
                else if (isActiveAndEnabled)
                    StartCoroutine(SkillUse());
            }
        }
    }
    
    //추격 시작 - ChaseAndShield 호출
    private void AttackStart()
    {
        if (!isControl && !isAttack && !isShieldOn)
        {
            //타겟 설정
            SetTargetByDistance();

            //타겟 설정 완료 시 추적 시작
            if(currentTarget)
            {
                isAttack = true;

                if (isActiveAndEnabled)
                    chaseCoroutine = StartCoroutine(ChaseAndShield());
            }
        }
    }

    //접근 후 보호막 켜기
    public IEnumerator ChaseAndShield()
    {
        if (!isChase && !isShieldOn)
        {
            isChase = true;
            AnimationSet(Anim.Move);

            while (targetDistance > basicAttackRange)
            {
                if (CanMove())
                {
                    if ((int)nowArea > 3 && currentTarget && (int)currentTarget.nowArea < 4)
                        Move(Vector3.right);
                    else
                        Move(targetVector);
                }

                SetTargetByDistance();

                if (currentTarget)
                {
                    if (currentTarget.DeathCheck())
                    {
                        targets.Remove(currentTarget);
                        SetTargetByDistance();
                        
                        if (!currentTarget)
                        {
                            isAttack = isChase = false;
                            yield break;
                        }
                    }
                }
                else
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

    //버튼을 통한 보호막 온오프
    public override void AttackBtn()
    {
        if (!isAttack)
        {
            if (isShieldOn)
            {
                isAttack = true;
                AnimationSet(Anim.Idle);
            }
            else
            {
                isAttack = true;
                SeeTarget();
                AnimationSet(Anim.Attack);
            }
        }
    }

    //자동 전투 - 방패 들고 있을 시 스킬 사용
    IEnumerator SkillUse()
    {
        while (isShieldOn && !isControl)
        {
            SetTargetByDistance();

            if (targetDistance < 1f)
                if (cooldowns[1] <= 0)
                    Skill2();
                else if (cooldowns[0] <= 0)
                    Skill1();

            yield return new WaitForSeconds(1f);
        }
    }
    //************************************

    //패시브 관련
    //************************************
    //비전투 시 정의 게이지 감소
    IEnumerator JusticeRestore()
    {
        while (isActiveAndEnabled)
        {
            while(justiceTime > 0)
            {
                justiceTime -= 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            justice = Mathf.Max(0, justice - 2);
            yield return new WaitForSeconds(0.1f);
        }
    }
    //************************************


    //보호막 관련
    //************************************
    //보호막 켜는 함수
    public void ShieldOn()
    {
        isAttack = false;
        isShieldOn = true;
        AnimationSet("IsShield", true);
        shieldBar.gameObject.SetActive(true);

        //쉴드 회복 코루틴 끄기
        if (shieldCoroutine != null)
            StopCoroutine(shieldCoroutine);

        //자동 전투 시에는 스킬 온
        if (!isControl)
            StartCoroutine(SkillUse());
    }

    //보호막 끄는 함수
    public void ShieldOff()
    {
        isShieldOn = false;
        AnimationSet("IsShield", false);

        isAttack = false;
        shieldBar.gameObject.SetActive(false);

        //쉴드 회복 코루틴 켜기
        shieldCoroutine = StartCoroutine(ShieldRestore());

        //자동 전투 시에는 회복 이동
        if (!isControl)
            StartCoroutine(RestoreMove());
    }

    //보호막 회복하는 함수 - shieldOff 함수에서 호출
    IEnumerator ShieldRestore()
    {
        yield return new WaitForSeconds(3);

        while (currentShield < totalShield)
        {
            currentShield += (int)(totalShield * 0.1f);

            if (currentShield >= totalShield)
            {
                currentShield = totalShield;
                break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    //자동 전투 시 후퇴 함수
    IEnumerator RestoreMove()
    {
        while (!isControl && currentShield != totalShield && !isAttack)
        {
            SetTargetByDistance();

            if (currentTarget)
            {
                float dis = Mathf.Abs(transform.position.x - currentTarget.transform.position.x) - currentTarget.stats[(int)EnemyStat.Range] - 0.5f;

                if (dis > 0.1f)
                {
                    AnimationSet(Anim.Move);
                    Move(targetVector);
                }
                else if (dis < -0.1f)
                {
                    AnimationSet(Anim.Move);
                    Move(-targetVector);
                }
                else
                    AnimationSet(Anim.Idle);
            }
            else
                AnimationSet(Anim.Idle);

            yield return new WaitForSeconds(0.03f);
        }

        AttackMain();
    }
    //************************************


    //Skill 1
    //**********************************
    //직접 컨트롤 시 Skill 버튼과 연동
    public override void Skill1()
    {
        if (isAttack)
            return;

        if (isActiveAndEnabled)
            StartCoroutine(Scourge());
    }

    //Skill 1 - Scourge
    private IEnumerator Scourge()
    {
        SetTargetByDistance();

        if (!isAttack && (currentTarget != null) && (targetDistance <= skill1Range) && cooldowns[0] <= 0)
        {
            isAttack = true;
            SeeTarget();
            AnimationSet(Anim.Skill1);

            for (cooldowns[0] = cooldownStart[0]; cooldowns[0] > 0; cooldowns[0] -= 0.05f)
            {
                yield return new WaitForSeconds(0.05f);
            }

            cooldowns[0] = 0;
        }
        else
        {
            AttackStart();
        }
    }

    //Skill 1 피해 함수
    public void ScourgeDamage()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            //1. y축 거리 체크, 2. 내 앞에 존재, 3. 너무 멀리 있지 않음
            if (Mathf.Abs(targets[i].transform.position.y - transform.position.y) < 0.1f &&
                targets[i].transform.position.x - transform.position.x > transform.localScale.x * 0.1f &&
                Mathf.Abs(targets[i].transform.position.x - transform.position.x) < 2f)
            {
                targets[i].GetDamage((int)(1.2f * Random.Range(totalStats[(int)CharStat.AttackMin], totalStats[(int)CharStat.AttackMax] + 1) * (1 + justice / 200)), false, assignIdx);
                if (targets[i].DeathCheck())
                {
                    targets[i].ItemDrop();
                    targets.RemoveAt(i);
                }
            }
        }
    }
    //**********************************


    //Skill 2
    //**********************************
    //직접 컨트롤 시 Skill 버튼과 연동
    public override void Skill2()
    {
        if (isAttack || isBravery)
            return;

        if (isActiveAndEnabled)
            StartCoroutine(Bravery());
    }
    
    //Skill 2 - Bravery
    private IEnumerator Bravery()
    {
        //타겟 설정
        SetTargetByDistance();

        if (currentTarget == null)
            yield break;
        
        if (!isAttack && !isBravery && cooldowns[1] <= 0)
        {
            cooldowns[1] = 6;

            SeeTarget();
            AnimationSet(Anim.Skill2);
            

            for (cooldowns[1] = cooldownStart[1]; cooldowns[1] > 0; cooldowns[1] -= 0.05f)
            {
                yield return new WaitForSeconds(0.05f);
            }

            cooldowns[1] = 0;
        }
    }

    //BraveryMove Coroutine 호출 - 애니메이션에서 직접 호출
    public void Skill2Move()
    {
        if (isActiveAndEnabled)
            StartCoroutine(BraveryMove());
    }

    //용기 실제 이동하는 함수 - 애니메이션 호출
    IEnumerator BraveryMove()
    {
        Vector2 braveVector = targetVector;
        Vector3 addVector = Vector3.right / 2;

        //리스트 비우기
        braveryList.Clear();

        if (!isAttack && !isBravery)
        {
            isAttack = true;
            isBravery = true;
            SeeTarget();
            addVector *= transform.localScale.x;

            //이동
            for (float i = 0; i < 2.25f; i += 0.24f)
            {
                foreach (Enemy ene in targets)
                {
                    if (!braveryList.Contains(ene.gameObject))
                        //1. y축 거리 체크, 2. 내 앞에 존재, 3. 너무 멀리 있지 않음
                        if (Mathf.Abs(ene.transform.position.y - transform.position.y) < 0.1f &&
                            ene.transform.position.x - transform.position.x > transform.localScale.x * 0.1f &&
                            Mathf.Abs(ene.transform.position.x - transform.position.x) < 0.6f)
                        {
                            braveryList.Add(ene.gameObject);
                            ene.GetDamage((int)(0.9f * Random.Range(totalStats[(int)CharStat.AttackMin], totalStats[(int)CharStat.AttackMax]) * (1 + justice / 200)), false, assignIdx);
                        }
                }

                transform.Translate(braveVector * 0.24f);

                foreach (GameObject ene in braveryList)
                {
                    ene.transform.position = transform.position + addVector;
                }

                yield return new WaitForSeconds(0.03f);
            }

            isBravery = false;

            if (isShieldOn)
                AnimationSet(Anim.Attack);
            else
                AnimationSet(Anim.Idle);

            isAttack = false;
            AttackMain();
        }
    }
    //**********************************


    public override void TotalStatLoad()
    {
        for (int i = 0; i < 10; i++)
            totalStats[i] = stats[i];

        currentHP = (int)totalStats[(int)CharStat.HPMax];
        currentShield = totalShield = 2 * currentHP;

        basicAttackRange = 3.2f;
        skill1Range = 2;
        skill2Range = 2.25f;

        basicKnockbackRate = 1.25f;
        skill1KnockbackRate = 1.75f;

        basicCooldown = 0;
        cooldowns[0] = cooldowns[1] = 0;
        cooldownStart[0] = 8f;
        cooldownStart[1] = 10f;

        StartCoroutine(JusticeRestore());
    }

    public override void GetDamage(int damage, bool type)
    {
        //물리, 마법 타입 구분 : 물리 : true, 마법 : false
        if (type)
            damage -= (int)totalStats[(int)CharStat.PhysicalDefense];
        else
            damage -= (int)totalStats[(int)CharStat.MagicDefense];

        if (damage < 1)
            damage = 1;

        //실드가 켜있는 경우 -> 실드가 우선적으로 피해를 입음
        if (isShieldOn)
        {
            //입는 피해보다 남은 실드량이 더 많은 경우, 실드가 모든 피해 입음
            if (currentShield > damage)
            {
                currentShield -= damage;
                justice += 200f * damage / totalShield;

                if (justice > justiceMax)
                    justice = justiceMax;
            }
            //입는 피해가 남은 실드량보다 많은 경우, 실드 전부 소진하고 남은 피해는 체력이 받음
            else
            {
                justice += 200f * currentShield / totalShield;
                if (justice > justiceMax)
                    justice = justiceMax;

                damage -= currentShield;
                currentHP -= damage;

                currentShield = 0;
                AnimationSet(Anim.Idle);
            }
        }
        //실드가 꺼있는 경우, 체력이 받음
        else
            currentHP -= damage;

        justiceTime = 3;
    }

    public override void BoolInit()
    {
        justice = 0;
        justiceTime = 0;
        isBravery = false;

        base.BoolInit();
    }

    public override bool CanMove()
    {
        return base.CanMove() && !isShieldOn;
    }
}

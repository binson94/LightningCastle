using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***********************************************************************************
    분쇄자 클래스

    <변수 내용>
    - Skill 2 관련 (시전 중 boolean, 스킬 충돌체)
    - Passive 카운트

    <함수 내용>
    - AttackMain -> ChaseAndAttack -> AttackDamage + AreaDamage -> AttackReady -> AttackMain : 공격 알고리즘
    - Passive를 위한 추가 공격 함수 OverheatAttackDamage
    - Smite, WhirlWind 스킬 함수
    - 스텟 json 파일 로드
    
************************************************************************************/

public class Crusher : Character {

    //Skill 2 관련
    public WhirlWindRange whirlWindRange;

    //Passive 관련
    public int overheatCount;

    /****************************
     이      하      함       수
     ***************************/

    //공격 메인 함수
    public override void AttackMain()
    {
        if (!isControl && !isGather && !isAttack)
        {
            AnimationSet(Anim.Idle);

            //가능하면 강타 사용
            if (cooldowns[0] <= 0)
            {
                SetTargetByDistance();
                if ((currentTarget != null) && (targetDistance <= skill1Range))
                {
                    Skill1();
                }
                else
                {
                    AttackStart();
                }
            }
            //가능하면 휠윈드 사용
            else if (cooldowns[1] <= 0)
            {
                SetTargetByDistance();
                if ((currentTarget != null) && targetDistance < 1.5f)
                {
                    Skill2();
                }
                else
                {
                    AttackStart();
                }
            }
            else
            {
                AttackStart();
            }
        }
    }

    private void AttackStart()
    {
        if(!isControl && !isAttack)
        {
            isAttack = true;

            SetTargetByDistance();

            if (currentTarget != null)
            {
                if (isActiveAndEnabled)
                    chaseCoroutine = StartCoroutine(ChaseAndAttack());
            }
            else
                isAttack = false;
        }
    }

    //접근 후 공격
    public IEnumerator ChaseAndAttack()
    {
        if (!isChase)
        {
            isChase = true;
            AnimationSet(Anim.Move);

            //같은 층에 있고 사거리 내에 있을 때까지
            while ((currentTarget != null && !IsEqualFloor(currentTarget)) ||(currentTarget != null && targetDistance > basicAttackRange))
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
                    if (currentTarget.currentHP <= 0)
                    {
                        targets.Remove(currentTarget);
                        SetTargetByDistance();
                        if (currentTarget == null)
                            break;
                    }
                }
                else
                {
                    SetTargetByDistance();
                    if (currentTarget == null)
                        break;
                }

                yield return new WaitForSeconds(0.03f);

            }
            isChase = false;

            SeeTarget();
            if (overheatCount < 6)
            {
                AnimationSet(Anim.Attack);
                overheatCount++;
            }
            else
            {
                //overheat Attack
                AnimationSet(Anim.Special);
                overheatCount = 0;
            }
        }
    }

    //실제 피해를 입히는 함수
    public void AttackDamage()
    {
        AreaDamage(1, basicAttackRange, basicKnockbackRate);
    }

    //광역 피해 - 피해 배수, 사거리, 넉백 계수를 매개변수로 받음
    private void AreaDamage(float coeff, float range, float knockBack)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            //1. y축 거리 체크, 2. 내 앞에 존재, 3. 너무 멀리 있지 않음
            if (Mathf.Abs(targets[i].transform.position.y - transform.position.y) < 0.1f &&
                targets[i].transform.position.x - transform.position.x > transform.localScale.x * 0.05f &&
                Mathf.Abs(targets[i].transform.position.x - transform.position.x) < range)
            {
                targets[i].GetDamage((int)(coeff * Random.Range(totalStats[(int)CharStat.AttackMin], totalStats[(int)CharStat.AttackMax] + 1)), true, assignIdx);
                targets[i].Knockback(knockBack);

                if (targets[i].DeathCheck())
                {
                    targets[i].ItemDrop();
                    targets.RemoveAt(i);
                }
            }
        }
    }

    //공격 딜레이
    public IEnumerator AttackReady()
    {
        StartCoroutine(AttackCooldown());
        yield return new WaitForSeconds(1 / (4 * totalStats[(int)CharStat.AttackSpeed]));      //대기 시간
        AnimationSet(Anim.Idle);

        while (basicCooldown > 0)
        {
            if (currentTarget != null)                                    //카이팅 또는 추격
            {
                if (!isControl && CanMove())
                {
                    AnimationSet(Anim.Move);

                    if (targetDistance - basicAttackRange > 0.01f)
                    {
                        if ((currentTarget != null) && transform.position.y - currentTarget.transform.position.y > 0.05f)
                        {
                            Move(Vector3.right);
                        }
                        else
                        {
                            Move(targetVector);
                        }
                    }
                    else if (targetDistance - basicAttackRange < -0.1f)
                    {
                        if ((currentTarget != null) && transform.position.y - currentTarget.transform.position.y > 0.05f)
                        {
                            Move(Vector3.left);
                        }
                        else
                        {
                            Move(-targetVector);
                        }
                    }
                }

                SetTargetByDistance();
            }
            else
            {
                SetTargetByDistance();
            }

            yield return new WaitForSeconds(0.03f);
        }

        isAttack = false;
        AttackMain();
    }

    //버튼을 통한 공격 실행
    public override void AttackBtn()
    {
        if (isAttack || isWhirlWind)
            return;

        if (!isBtnAttack && basicCooldown <= 0)
        {
            SetTargetByDistance();

            if (currentTarget != null)
            {
                if (targetDistance < basicAttackRange)
                {
                    isAttack = true;
                    isBtnAttack = true;

                    SeeTarget();
                    AnimationSet(Anim.Attack);
                }
            }
        }
    }

    //버튼을 통한 공격 코루틴
    public IEnumerator BtnAttackReady()
    {
        StartCoroutine(AttackCooldown());
        yield return new WaitForSeconds(1 / (4 * totalStats[(int)CharStat.AttackSpeed]));
        AnimationSet(Anim.Idle);
        isAttack = false;
        yield return new WaitForSeconds(3 / (4 * totalStats[(int)CharStat.AttackSpeed]));
        isBtnAttack = false;
    }

    //직접 컨트롤 시 Skill 1과 연동
    public override void Skill1()
    {
        if (isAttack || isWhirlWind)
            return;

        if (isActiveAndEnabled)
            StartCoroutine(Smite());
    }

    //Skill 1 - Smite
    private IEnumerator Smite()
    {
        if (cooldowns[0] <= 0)
        {
            SetTargetByDistance();
            if ((currentTarget != null) && (!isAttack) && cooldowns[0] <= 0)
            {
                SeeTarget();

                if (overheatCount < 6)
                {
                    if (targetDistance <= skill1Range)
                    {
                        isAttack = true;

                        AnimationSet(Anim.Skill1);

                        for (cooldowns[0] = cooldownStart[0]; cooldowns[0] > 0; cooldowns[0] -= 0.05f)
                        {
                            yield return new WaitForSeconds(0.05f);
                        }

                        cooldowns[0] = 0;
                    }
                    else
                        AttackStart();
                }
                else if (targetDistance <= 3.75f)
                {
                    isAttack = true;

                    AnimationSet(Anim.Skill1);

                    for (cooldowns[0] = 8; cooldowns[0] > 0; cooldowns[0] -= 0.05f)
                    {
                        yield return new WaitForSeconds(0.05f);
                    }

                    cooldowns[0] = 0;
                }
                else if (!isControl)
                    AttackStart();

            }
            else if (!isControl)
                AttackStart();
        }
        else if (!isControl)
            AttackStart();
    }

    public void SmiteDamage()
    {
        if (currentTarget)
        {
            if (overheatCount < 6)
            {
                overheatCount++;

                AreaDamage(0.8f, skill1Range, skill1KnockbackRate);
            }
            else
            {
                overheatCount = 0;
                
                AreaDamage(0.8f, skill2Range, skill2KnockbackRate);
            }
        }
    }

    //직접 컨트롤 시 Skill 2와 연동
    public override void Skill2()
    {
        if (isAttack || isWhirlWind)
            return;

        StartCoroutine(WhirlWind());
    }

    //Skill 2 - WhirlWind
    private IEnumerator WhirlWind()
    {
        if (cooldowns[1] <= 0)
        {
            AnimationSet(Anim.Skill2);

            StartCoroutine(WhirlWindCooldown());        //쿨타임 계산 시작

            whirlWindRange.WhirlWindList.Clear();
            isWhirlWind = true;
            whirlWindRange.gameObject.SetActive(true);
            whirlWindRange.skillOn = true;

            totalStats[(int)CharStat.MovementSpeed] = stats[(int)CharStat.MovementSpeed] - 4;

            if (overheatCount < 6)
            {
                overheatCount++;
                whirlWindRange.isOverheat = false;

                yield return new WaitForSeconds(2f);

                whirlWindRange.skillOn = false;
                isWhirlWind = false;
            }
            else
            {
                overheatCount = 0;
                whirlWindRange.isOverheat = true;

                yield return new WaitForSeconds(3f);

                whirlWindRange.skillOn = false;
                isWhirlWind = false;
            }

            AnimationSet(Anim.Idle);

            if (!isControl)
                AttackMain();

            totalStats[(int)CharStat.MovementSpeed] = stats[(int)CharStat.MovementSpeed];

            yield return new WaitForSeconds(1f);
            whirlWindRange.gameObject.SetActive(false);
        }
        else
            AttackStart();
    }

    private IEnumerator WhirlWindCooldown()
    {
        for (cooldowns[1] = cooldownStart[1]; cooldowns[1] > 0; cooldowns[1] -= 0.05f)
        {
            yield return new WaitForSeconds(0.05f);
        }

        cooldowns[1] = 0;
    }

    //공통
    public override void TotalStatLoad()
    {
        for (int i = 0; i < 10; i++)
            totalStats[i] = stats[i];

        currentHP = (int)totalStats[(int)CharStat.HPMax];
        
        basicAttackRange = 2;
        skill1Range = 2.25f;
        skill2Range = 3.75f;

        basicKnockbackRate = 0.75f;
        skill1KnockbackRate = 2;
        skill2KnockbackRate = 2.75f;
        
        basicCooldown = 0;
        cooldowns[0] = cooldowns[1] = 0;
        cooldownStart[0] = 10f;
        cooldownStart[1] = 10f;
    }

    public override void BoolInit()
    {
        overheatCount = 0;
        base.BoolInit();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spirit : Character
{
    public Alchemist alchmist;

    public void SpiritStart()
    {
        BoolInit();
        SpiritStatLoad();

        StartCoroutine(LifeTime());
        AttackMain();
    }

    //자동 전투 관련
    //****************************************
    //공격 메인 함수
    public override void AttackMain()
    {
        //공격 중복 방지
        if (!isAttack)
        {
            AnimationSet("Now State", (int)Anim.Idle);

            AttackStart();
        }
    }

    //타겟 설정 및 Chase 코루틴 호출
    private void AttackStart()
    {
        //직접 컨트롤 중이 아닐 때만 실행
        if (!isAttack)
        {
            isAttack = true;

            if (!currentTarget)
                if (targets.Count > 0)
                {
                    SetTargetByDistance();
                    chaseCoroutine = StartCoroutine(ChaseAndAttack());
                }
                else
                    isAttack = false;
            else
            {
                SetTargetByDistance();
                chaseCoroutine = StartCoroutine(ChaseAndAttack());
            }
        }
    }

    //접근 후 공격
    public IEnumerator ChaseAndAttack()
    {
        //코루틴 중첩 방지
        if (!isChase)
        {
            isChase = true;

            //이동 애니메이션 재생
            AnimationSet(Anim.Move);

            while (targetDistance > basicAttackRange)
            {
                //나는 2층에 있고, 적은 1층에 있는 경우, 발코니 쪽으로 이동하여 내려가려 함
                if ((int)nowArea > 3 && currentTarget && (int)currentTarget.nowArea < 4)
                    Move(Vector3.right);
                else
                    Move(targetVector);


                //한 프레임 이동 후 타겟 재설정
                SetTargetByDistance();

                //재설정 후 타겟 없음 -> 리스트 비어있음 -> 함수 종료
                if (!currentTarget)
                {
                    isAttack = isChase = false;
                    AnimationSet(Anim.Idle);
                    yield break;
                }

                yield return new WaitForSeconds(0.03f);
            }
            isChase = false;

            SeeTarget();
            AnimationSet(Anim.Attack);
        }
    }

    //실제 피해를 입히는 함수 - 애니메이션에서 직접 호출
    public void AttackDamage()
    {
        //타겟이 있는 경우에만 작동
        if (currentTarget)
        {
            //데미지 계산 : min ~ max 사이 랜덤 값 - 타겟 방어력
            currentTarget.GetDamage(Random.Range((int)totalStats[(int)CharStat.AttackMin], (int)totalStats[(int)CharStat.AttackMax] + 1), true, assignIdx);

            currentTarget.Knockback(basicKnockbackRate);

            //죽었는 지 체크
            if (currentTarget.DeathCheck())
            {
                targets.Remove(currentTarget);
                currentTarget.ItemDrop();
                currentTarget = null;
            }
        }
    }

    //공격 후딜레이 - 애니메이션에서 직접 호출
    public IEnumerator AttackReady()
    {
        if (isControl)
            yield break;
        //공격 딜레이의 1 / 4는 이동 불가
        StartCoroutine(AttackCooldown());

        for (float i = 0; i < 1 / (4 * totalStats[(int)CharStat.AttackSpeed]); i += 0.03f)
        {
            yield return new WaitForSeconds(0.03f);
        }

        //애니메이션 설정
        AnimationSet(Anim.Idle);

        //딜레이의 나머지 3 / 4는 카이팅 또는 추격
        while (basicCooldown > 0)
        {
            //타겟이 있을 때만 이동함
            if (currentTarget != null)
            {
                //적이 사거리보다 멀리 있는 경우 -> 접근
                if (targetDistance - basicAttackRange > 0.01f)
                {
                    //이동 애니메이션 설정
                    AnimationSet(Anim.Move);

                    if (transform.position.y - currentTarget.transform.position.y > 0.05f)
                        Move(Vector3.right);
                    else
                        Move(targetVector);
                }
                //적이 사거리보다 가까이 있는 경우 -> 후퇴
                else if (targetDistance - basicAttackRange < -0.01f)
                {
                    //이동 애니메이션 설정
                    AnimationSet(Anim.Move);

                    if (transform.position.y - currentTarget.transform.position.y > 0.05f)
                        Move(Vector3.left);
                    else
                        Move(-targetVector);
                }
                //적이 사거리 정도에 있는 경우 -> 가만히 있음
                else
                {
                    AnimationSet(Anim.Idle);
                }
            }
            //타겟이 없는 경우 -> 가만히 있음
            else
            {
                AnimationSet(Anim.Idle);
            }

            //새로 타겟 설정
            SetTargetByDistance();
            yield return new WaitForSeconds(0.03f);
        }

        //새로 공격 이어감
        isAttack = false;
        AttackMain();
    }
    //****************************************


    //연금술사 현재 스텟을 고려한 스텟 로드
    public void SpiritStatLoad()
    {
        totalStats[(int)CharStat.HPMax] = alchmist.totalStats[(int)CharStat.HPMax];
        currentHP = (int)totalStats[(int)CharStat.HPMax];

        totalStats[(int)CharStat.AttackMin] = 0.8f * alchmist.totalStats[(int)CharStat.AttackMin];
        totalStats[(int)CharStat.AttackMax] = 0.8f * alchmist.totalStats[(int)CharStat.AttackMax];

        totalStats[(int)CharStat.PhysicalDefense] = 0.9f * alchmist.totalStats[(int)CharStat.PhysicalDefense];
        totalStats[(int)CharStat.MagicDefense] = 0.9f * alchmist.totalStats[(int)CharStat.MagicDefense];

        totalStats[(int)CharStat.AttackSpeed] = 1.2f;
        totalStats[(int)CharStat.MovementSpeed] = 9;

        basicAttackRange = 0.8f;
        basicKnockbackRate = 1;
    }

    //생존 제한 시간 - 10초
    IEnumerator LifeTime()
    {
        float time = 10f;

        while(time > 0)
        {
            time -= 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        gameObject.SetActive(false);
    }
}

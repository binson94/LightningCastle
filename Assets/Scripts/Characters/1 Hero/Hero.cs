using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***********************************************************************************
    용사 클래스

    <변수 내용>
    * 패시브 카운트, 지속시간, 패시브 켜 있는지 여부
    * 부메랑 프리팹, 생성한 부메랑, 부메랑 타겟 리스트
    * 부메랑 맨 이미지, 부메랑 매지 않은 이미지, 부메랑 메지 않은 애니메이터

    <함수 내용>
    * AttackMain -> AttackStart -> ChaseAndAttack -> 애니메이션에서 처리(AttackReady, AttackDamage) -> AttackMain
    * 일정 시간 후 Passive 초기화
    * Strike, Boomerang 스킬 함수
    * 스텟 json 파일 로드
    
************************************************************************************/

public class Hero : Character {

    //Passive 관련
    //*****************************
    public int burningFireCount;        //패시브 중첩 수
    public float burningFireTime;       //남은 패시브 지속 시간, 공격 시마다 초기화됨
    public bool passiveOn;              //패시브 켜 있는 여부
    //*****************************

    //Skill 2 관련
    //*****************************
    public GameObject boomerang_Prefab;                   //부메랑 프리팹
    public Boomerang boomerang;                           //부메랑 인스턴스
    public List<Enemy> BoomerangList = new List<Enemy>(); //부메랑에 맞은 적 리스트에 저장, 부메랑이 돌아올 때 리스트 한 번 리셋
    //*****************************

    //애니메이션 관련
    //*****************************
    public SpriteRenderer withBoomerang;           //부메랑을 들고있는 이미지
    public SpriteRenderer withoutBoomerang;        //부메랑을 들지 않은 이미지

    Animator addAnim;                       //부메랑 없는 에니메이션
    public Animator[] addanimArray;         //모든 색깔 애니메이터들(부메랑 없음)
    //*****************************

    //부메랑 가진 이미지만 보이게 설정
    private void Start()
    {
        withBoomerang.enabled = true;       //기본 상태는 아직 부매랑을 사용하지 않았으므로 부매랑을 가지고 있음
        withoutBoomerang.enabled = false;
    }

    //공격 메인 함수
    public override void AttackMain()
    {
        //직접 컨트롤 중이 아닐 때에만 실행
        if (!isControl && !isGather)
        {
            //공격 중복 방지
            if (!isAttack)
            {
                AnimationSet("Now State", (int)Anim.Idle);

                //스킬 우선 순서 : 스킬 1 > 스킬 2 > 기본 공격
                if (cooldowns[0] <= 0)
                {
                    SetTargetByDistance();
                    if ((currentTarget != null) && targetDistance < skill1Range)
                    {
                        Skill1();
                    }
                    else
                        AttackStart();
                }
                else if (cooldowns[1] <= 0)
                {
                    Skill2();
                }
                else
                    AttackStart();
            }
        }
    }

    //타겟 설정 및 Chase 코루틴 호출
    private void AttackStart()
    {
        //직접 컨트롤 중이 아닐 때만 실행
        if (!isControl && !isAttack)
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
                if (CanMove())
                {
                    //나는 2층에 있고, 적은 1층에 있는 경우, 발코니 쪽으로 이동하여 내려가려 함
                    if ((int)nowArea > 3 && currentTarget && (int)currentTarget.nowArea < 4)
                        Move(Vector3.right);
                    else
                        Move(targetVector);
                }

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

            //용사 패시브 - 기본 공격 시 Strike 쿨타임 감소
            if (cooldowns[0] > 0)
                cooldowns[0] -= 0.5f;

            //패시브 꺼 있으면 켜기
            if (!passiveOn)
            {
                burningFireCount++;
                StartCoroutine(Passive());
            }
            //이미 패시브 켜 있으면 스택 증가, 패시브 지속시간 초기화
            else
            {
                if (burningFireCount < 5)
                    burningFireCount++;
                burningFireTime = 2;
            }

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
        
        for(float i = 0; i < 1 / (4 * totalStats[(int)CharStat.AttackSpeed]); i += 0.03f)
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
                //홀드 중에는 이동하지 않음
                if (CanMove())
                {
                    //적이 사거리보다 멀리 있는 경우 -> 접근
                    if (targetDistance - basicAttackRange > 0.01f)
                    {
                        //이동 애니메이션 설정
                        AnimationSet(Anim.Move);

                        if (transform.position.y - currentTarget.transform.position.y > 0.05f)
                            Move(-targetVector);
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

    //버튼을 통한 공격 실행
    public override void AttackBtn()
    {
        //중복 호출 방지
        if (isAttack)
            return;

        if (!isBtnAttack && basicCooldown <= 0)
        {
            //타겟 설정
            SetTargetByDistance();

            //타겟이 있고, 사거리 내에 있는 경우 공격 실행
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

    //버튼을 통한 공격 후딜레이 - 애니메이션에서 직접 호출
    public IEnumerator BtnAttackReady()
    {
        StartCoroutine(AttackCooldown());

        yield return new WaitForSeconds(1 / (4 * totalStats[(int)CharStat.AttackSpeed]));
        AnimationSet(Anim.Idle);

        isAttack = false;
        yield return new WaitForSeconds(3 / (4 * totalStats[(int)CharStat.AttackSpeed]));
        isBtnAttack = false;
    }

    //시간이 지난 후 Passive Count 리셋
    private IEnumerator Passive()
    {
        //패시브 꺼 있는 경우 켜기
        if(!passiveOn)
        {
            passiveOn = true;

            //지속시간 계속 감소, 공격속도 증가 유지
            for (burningFireTime = 2; burningFireTime > 0; burningFireTime -= 0.03f)
            {
                totalStats[(int)CharStat.AttackSpeed] = stats[(int)CharStat.AttackSpeed] + (0.15f * burningFireCount);
                yield return new WaitForSeconds(0.03f);
            }

            //지속시간 만료 -> 패시브 끄기
            burningFireTime = 0;
            burningFireCount = 0;
            totalStats[(int)CharStat.AttackSpeed] = stats[(int)CharStat.AttackSpeed];
            passiveOn = false;
        }
    }


    //직접 컨트롤 시 Skill 버튼과 연동
    //****************************************
    public override void Skill1()
    {
        if (isAttack)
            return;

        if (isActiveAndEnabled)
            StartCoroutine(Strike());
    }

    //Skill 1 - Strike
    private IEnumerator Strike()
    {
        SetTargetByDistance();

        if ((currentTarget != null) && (targetDistance <= skill1Range) && cooldowns[0] <= 0)
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

    //Skill 1 피해 - 애니메이션에서 직접 호출
    public void StrikeDamage()
    {
        if (currentTarget)
        {
            currentTarget.GetDamage((int)(2.5f * Random.Range(totalStats[(int)CharStat.AttackMin], totalStats[(int)CharStat.AttackMax] + 1)), true, assignIdx);

            currentTarget.Knockback(skill1KnockbackRate);
            
            if (currentTarget.DeathCheck())
            {
                targets.Remove(currentTarget);
                currentTarget.ItemDrop();
                currentTarget.gameObject.SetActive(false);
                currentTarget = null;
            }
        }
    }
    //****************************************


    //직접 컨트롤 시 Skill 버튼과 연동
    //****************************************
    public override void Skill2()
    {
        if (isAttack)
            return;

        if (!boomerang.isActiveAndEnabled && isActiveAndEnabled)
                StartCoroutine(Boomerang());
    }

    //Skill 2 - Boomerang
    private IEnumerator Boomerang()
    {
        if (cooldowns[1] <= 0 && !isAttack)
        {
            isAttack = true;

            SetTargetByDistance();

            //타겟 있으면 타겟 방향으로 발사
            if (currentTarget != null)
                boomerang.TargetVector = targetVector;
            //타겟 없으면 앞으로 발사
            else
                boomerang.TargetVector.Set(transform.localScale.x, 0);
            
            BoomerangList.Clear();
            boomerang.transform.position = transform.position;

            SeeTarget();
            charAnim.SetInteger("Now State", (int)Anim.Skill2);
            
            for (cooldowns[1] = cooldownStart[1]; cooldowns[1] > 0; cooldowns[1] -= 0.05f)
                yield return new WaitForSeconds(0.05f);

            cooldowns[1] = 0;
        }
        else if (!isControl)
            AttackStart();
    }

    //부메랑 던지기 - 애니메이션에서 직접 호출
    public void BoomerangGo()
    {
        boomerang.gameObject.SetActive(true);
        boomerang.BoomerangStart(skill2Range);
    }

    //부메랑 던진 후 공격 이어가기 - 애니메이션에서 직접 호출
    public void BoomerangAttackMain()
    {
        withBoomerang.enabled = false;
        withoutBoomerang.enabled = true;
        isAttack = false;
        AnimationSet("Now State", (int)Anim.Idle);
        AttackMain();
    }
    //****************************************


    //총 스텟 로드
    public override void TotalStatLoad()
    {
        for (int i = 0; i < 10; i++)
            totalStats[i] = stats[i];

        currentHP = (int)totalStats[(int)CharStat.HPMax];

        basicAttackRange = 0.7f;
        skill1Range = 1;
        skill2Range = 4;

        basicKnockbackRate = 1.25f;
        skill1KnockbackRate = 1.75f;

        basicCooldown = 0;
        cooldowns[0] = cooldowns[1] = 0;
        cooldownStart[0] = 8f;
        cooldownStart[1] = 10f;
    }

    //캐릭터와 관련된 오브젝트 생성 - 부메랑
    public override void CharInstantiate(int idx)
    {
        if (!PortalPos)
            PortalPos = GameObject.FindWithTag("Portal2").transform;

        boomerang = Instantiate(boomerang_Prefab).GetComponent<Boomerang>();
        boomerang.hero = this;
        boomerang.AnimationColorSet();

        boomerang.gameObject.SetActive(false);

        assignIdx = idx;
    }

    //애니메이션 설정
    public override void AnimationSet(Anim anim)
    {
        charAnim.SetInteger("Now State", (int)anim);
        addAnim.SetInteger("Now State", (int)anim);
    }

    public override void AnimationColorSet()
    {
        for (int i = 0; i < 8; i++)
            if ((int)charColor / 5 == i)
            {
                animArray[i].gameObject.SetActive(true);
                charAnim = animArray[i];
                addAnim = addanimArray[i];

                withBoomerang = charAnim.GetComponent<SpriteRenderer>();
                withoutBoomerang = addAnim.GetComponent<SpriteRenderer>();
            }
            else
            {
                animArray[i].gameObject.SetActive(false);
                addanimArray[i].gameObject.SetActive(false);
            }
    }

    public override void BoolInit()
    {
        burningFireTime = burningFireCount = 0;
        passiveOn = false;
        base.BoolInit();
    }
}

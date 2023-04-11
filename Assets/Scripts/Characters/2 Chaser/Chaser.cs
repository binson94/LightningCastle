using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***********************************************************************************
    추적자 클래스

    <변수 내용>
    * 현재 무기(false : Crossbow, true : Claw)
    * Crossbow 사격 벡터, 총알 프리팹, 총알,   #Character의 기본 스텟은 Clossbow의 스텟임
    * Claw 공격력, 공격속도, 사거리

    <함수 내용>
    * 총알 생성
    * Crossbow : AttackMain -> ChaseAndShot -> SetShotVector -> Shot -> AttackReady -> AttackMain : 공격 알고리즘
    * Claw : AttackMain -> ChaseAndAttack -> AttackDamage + AreaDamage -> AttackReady -> AttackMain : 공격 알고리즘
    * Rush, WeaponChange 스킬 함수
    
************************************************************************************/

public class Chaser : Character {

    //현재 무기 관련
    //*******************************
    public bool nowWeapon; //false -> Crossbow, true -> Claw
    //*******************************

    //Crossbow 관련 - 기본 스텟이 Crossbow 스텟으로 연관
    //*******************************
    public Vector2 shotVector;                  //사격할 방향 벡터
    public float shotSpeed = 5;                 //투사체 속도
    public GameObject bulletPrefab;             //총알 프리팹
    GameObject[] bullets = new GameObject[13];  //총알 인스턴스
    public int nowbullet;                       //현재 사용 중인 총알 index
    public Transform ShotPos;                   //총알 발사 시 출발 위치

    public float bowKnockBackRate;
    //*******************************

    //Claw 관련
    //*******************************
    public float ClawAttackRange;     //클로 공격 사거리
    public float ClawKnockBackRate;   //클로 넉백
    public float ClawBeforeDelay;     //클로 선딜레이
    //*******************************

    //Passsive 관련
    //*******************************
    public int ElementStack;          //원소 중첩 수
    //*******************************

    //Skill 1 관련
    //*******************************
    bool isRush = false;              //쇄도 중 여부
    Vector2 rushVector;
    //*******************************


    //공통 - 공격 메인 함수
    public override void AttackMain()
    {
        //중복 호출 방지
        if (!isControl && !isAttack && !isGather)
        {
            //애니메이션 설정
            AnimationSet(Anim.Idle);
            SetTargetByDistance();

            //타겟이 존재, 적이 적 사거리보다 가까이 있음, 적 사거리가 나보다 더 짧음 -> 앞 쇄도
            if (currentTarget != null &&
                targetDistance < currentTarget.stats[(int)EnemyStat.Range])
            {
                if (cooldowns[0] <= 0)
                {
                    Skill1();
                    return;
                }
            }

            //Claw인 경우
            if (nowWeapon)
                ClawAttackStart();
            //CrossBow인 경우
            else
                CrossbowAttackStart();
        }
    }

    //Claw - 타겟 설정 및 Chase 코루틴 호출
    private void ClawAttackStart()
    {
        //중복 호출 방지
        if(!isAttack && !isControl)
        {
            SetTargetByDistance();

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
                return;
            }
        }
    }

    //Crossbow - 타겟 설정 및 Chase 코루틴 호출
    private void CrossbowAttackStart()
    {
        //중복 호출 방지
        if(!isControl && !isAttack)
        {
            //타겟 설정
            SetTargetByDistance();
            
            if (currentTarget != null)
            {
                isAttack = true;
                if (isActiveAndEnabled)
                    chaseCoroutine = StartCoroutine(ChaseAndShot());
            }
            //타겟이 없음 -> 리스트 비어있음 -> 함수 끝
            else
            {
                isAttack = false;
            }
        }
    }

    //Claw - 접근 후 공격
    private IEnumerator ChaseAndAttack()
    {
        //중복 호출 방지
        if (!isChase)
        {
            isChase = true;

            //애니메이션 설정
            AnimationSet(Anim.Move);

            //타겟이 사정거리보다 멀리 있음 -> 접근
            while (targetDistance > ClawAttackRange)
            {
                //홀드 중 아님
                if (!isHold && !isRush && !isSelfHold)
                {
                    //나는 2층에 있고 적은 1층에 있는 경우 -> 발코니쪽으로 가서 내려가려 함
                    if ((int)nowArea > 3 && currentTarget && (int)currentTarget.nowArea < 4)
                        Move(Vector3.right);
                    else
                        Move(targetVector);
                }

                //타겟 설정
                SetTargetDistance();
                
                //타겟이 없음 -> 리스트 비어있음 -> 함수 끝
                if(currentTarget == null)
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

    //Claw - 실제 피해 - 애니메이션에서 직접 호출
    public void AttackDamage()
    {
        if (currentTarget)
        {
            //패시브 스택 존재 -> 피해 강화
            if (ElementStack > 0)
            {
                currentTarget.GetDamage((int)(1.5f * Random.Range((int)totalStats[(int)CharStat.ClawAttackMin], (int)totalStats[(int)CharStat.ClawAttackMax] + 1)), true, assignIdx);

                //패시브 스택 감소
                ElementStack -= 2;
                if (ElementStack <= 0)
                    ElementStack = 0;
            }
            //패시브 스택 없음 -> 그냥 피해
            else
                currentTarget.GetDamage(Random.Range((int)totalStats[(int)CharStat.ClawAttackMin], (int)totalStats[(int)CharStat.ClawAttackMax] + 1), true, assignIdx);

            currentTarget.Knockback(ClawKnockBackRate);

            //범위 피해 입히기
            AreaDamage(0.7f, ClawKnockBackRate);

            //적 처치
            if (currentTarget.DeathCheck())
            {
                targets.Remove(currentTarget);

                //Passive : 적 처치 시 쿨타임 감소
                cooldowns[0] -= 2;
                cooldowns[1] -= 2;
                for (int i = 0; i < 2; i++)
                    if (cooldowns[i] < 0)
                        cooldowns[i] = 0;

                currentTarget.ItemDrop();
                currentTarget = null;
            }
        }
    }

    //Claw - 광역 피해
    private void AreaDamage(float range, float knockBack)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            //currentTarget이 아니면서 범위 내에 존재
            if (targets[i] != currentTarget
                && Vector2.Distance(targets[i].transform.position, currentTarget.transform.position) < range)
            {
                //데미지 계산 : min ~ max 사이의 랜덤 값 - 방어력
                targets[i].GetDamage(Random.Range((int)totalStats[(int)CharStat.ClawAttackMin], (int)totalStats[(int)CharStat.ClawAttackMax] + 1), true, assignIdx);

                targets[i].Knockback(knockBack);

                //적 처치
                if (targets[i].DeathCheck())
                {
                    targets[i].ItemDrop();

                    targets.RemoveAt(i);

                    //Passive : 적 처치 시 쿨타임 감소
                    cooldowns[0] -= 2;
                    cooldowns[1] -= 2;
                    for (int j = 0; i < 2; i++)
                        if (cooldowns[j] < 0)
                            cooldowns[j] = 0;
                }
            }
        }
    }

    //CrossBow - 접근 후 공격
    private IEnumerator ChaseAndShot()
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
                if (!isHold && !isRush && !isSelfHold)
                {
                    Move(targetVector);
                }

                SetTargetByDistance();

                //타겟이 없음 -> 리스트 비어있음 -> 함수 끝
                if(currentTarget == null)
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

    //Crossbow - 사격 벡터 결정
    public void setShotVector()
    {
        if (currentTarget != null)
        {
            if(currentTarget.isChase)
            {
                Vector2 tempVector;
                float i;
                for (i = 0; i < 12; i += 0.03f)
                {
                    tempVector = currentTarget.transform.position - ShotPos.position;
                    
                    //적의 이동 방향으로 목적 Vector 보정
                    tempVector += i * currentTarget.stats[(int)EnemyStat.MoveSpeed] * 0.1f * currentTarget.targetVector;

                    //보정 성공 - 오차가 +- 0.07 이하
                    if (((shotSpeed * i + 0.07f) > Vector2.Distance(tempVector, new Vector2(0, 0)))
                        && ((shotSpeed * i - 0.07f) < Vector2.Distance(tempVector, new Vector2(0, 0))))
                    {
                        shotVector = new Vector2(currentTarget.transform.position.x, currentTarget.transform.position.y)
                                    + i * 0.1f * currentTarget.stats[(int)EnemyStat.MoveSpeed] * currentTarget.targetVector 
                                    - new Vector2(ShotPos.position.x, ShotPos.position.y);
                        break;
                    }
                }
                
                //보정 실패 -> 그냥 발사
                if (i >= 12)
                {
                    shotVector.Set(currentTarget.transform.position.x - ShotPos.position.x, currentTarget.transform.position.y - ShotPos.position.y);
                }
            }
            //적이 이동 중이 아님 -> 그냥 발사
            else
            {
                shotVector.Set(currentTarget.transform.position.x - ShotPos.position.x, currentTarget.transform.position.y - ShotPos.position.y);
            }
        }
        //타겟이 없음 -> 앞으로 발사
        else
        {
            shotVector.x = transform.localScale.x;
            shotVector.y = 0;
        }
    }

    //Crossbow - 사격
    public void Shot()
    {
        //총알 위치 설정
        bullets[nowbullet].transform.position = ShotPos.position;

        //총알 활성화
        bullets[nowbullet].SetActive(true);
        bullets[nowbullet].GetComponent<bullet>().moveVector = shotVector;
        bullets[nowbullet].GetComponent<bullet>().BulletStart();

        //다음 총알 설정
        nowbullet = (nowbullet + 1) % 13;
    }
    
    //공통 - 공격 후딜레이
    public IEnumerator AttackReady()
    {
        if (isControl)
            yield break;
        
        StartCoroutine(AttackCooldown());
        AnimationSet(Anim.Idle);

        //Claw인 경우
        if (nowWeapon)
        {
            //딜레이의 3 / 4는 카이팅 또는 추격
            while (basicCooldown > 0)
            {
                if (currentTarget != null)
                {
                    if (!isHold && !isSelfHold)
                    {
                        //타겟이 사거리보다 멀리 있음 -> 추격
                        if (targetDistance - ClawAttackRange > 0.01f)
                        {
                            AnimationSet(Anim.Move);
                            Move(targetVector);
                        }
                        //타겟이 사거리보다 가까이 있음 -> 후퇴
                        else if (targetDistance - ClawAttackRange < -0.01f)
                        {
                            AnimationSet(Anim.Move);
                            Move(-targetVector);
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
                }
                //타겟이 없음 -> 가만히 있음
                else
                    AnimationSet(Anim.Idle);

                SetTargetByDistance();

                yield return new WaitForSeconds(0.03f);
            }
        }
        //Crossbow인 경우
        else
        {
            //딜레이의 3 / 4는 추적 또는 카이팅
            while(basicCooldown > 0)
            {
                if (currentTarget != null)
                {
                    if (!isHold && !isSelfHold)
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
        }

        isAttack = false;
        AttackMain();
    }

    //버튼을 통한 공격 실행
    public override void AttackBtn()
    {
        if (isAttack)
            return;

        if (!isBtnAttack && basicCooldown <= 0)
        {
            SetTargetByDistance();

            //타겟 존재
            if (currentTarget)
            {
                //Claw인 경우
                if (nowWeapon)
                {
                    //사거리 내에 타겟 존재
                    if (targetDistance < ClawAttackRange)
                    {
                        isAttack = true;
                        isBtnAttack = true;

                        SeeTarget();
                        AnimationSet(Anim.Attack);
                    }
                }
                //Crossbow인 경우
                else
                {
                    //사격
                    isAttack = true;
                    isBtnAttack = true;

                    SeeTarget();
                    AnimationSet(Anim.Attack);
                }
            }
            //타겟이 없어도 Crossbow는 쏨
            else if (!nowWeapon)
            {
                isAttack = true;
                AnimationSet(Anim.Attack);
                isBtnAttack = true;
            }
        }
    }

    //버튼을 통한 공격 후딜레이
    public IEnumerator BtnAttackReady()
    {
        //Claw인 경우
        if (nowWeapon)
        {
            StartCoroutine(AttackCooldown());
            AnimationSet("Now State", (int)Anim.Idle);
            isAttack = false;
            yield return new WaitForSeconds(3 / (4 * totalStats[(int)CharStat.ClawAttackSpeed]));
            isBtnAttack = false;
        }
        //Crossbow인 경우
        else
        {
            StartCoroutine(AttackCooldown());
            AnimationSet("Now State", (int)Anim.Idle);
            isAttack = false;
            yield return new WaitForSeconds(3 / (4 * totalStats[(int)CharStat.AttackSpeed]));
            isBtnAttack = false;
        }
    }

    protected override IEnumerator AttackCooldown()
    {
        if (nowWeapon)
            basicCooldown = 0.8f / totalStats[(int)CharStat.ClawAttackSpeed];
        else
            basicCooldown = 0.8f / totalStats[(int)CharStat.AttackSpeed];

        while (basicCooldown > 0)
        {
            basicCooldown -= 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    //직접 컨트롤 시 Skill 버튼과 연동
    public override void Skill1()
    {
        //홀드 중 아님
        if (!isAttack && !isHold && !isSelfHold && isActiveAndEnabled)
            StartCoroutine(Rush());
        else if (!isControl)
            if (nowWeapon)
                ClawAttackStart();
            else
                CrossbowAttackStart();
    }

    //Skill 1 - Rush
    private IEnumerator Rush()
    {
        //타겟 설정
        SetTargetByDistance();
        
        //자동 전투 시 타겟 반대 방향으로 쇄도
        if (!isControl)
        {
            if (currentTarget != null)
                rushVector = -targetVector;
            else
                rushVector = Vector2.right;
        }
        //컨트롤 시 조이스틱 방향으로 쇄도
        else
        {
            if (moveVector != Vector2.zero)
            {
                rushVector = moveVector;
            }
            else
            {
                yield break;
            }
        }

        if (!isRush && cooldowns[0] <= 0)
        {
            isAttack = true;
            isRush = true;

            transform.localScale = new Vector3(rushVector.x, 1, 1);
            AnimationSet(Anim.Skill1);

            //패시브 - 원소 스택 채움
            ElementStack = 5;

            for (cooldowns[0] = cooldownStart[0]; cooldowns[0] > 0; cooldowns[0] -= 0.05f)
            {
                yield return new WaitForSeconds(0.05f);
            }

            cooldowns[0] = 0;
        }
    }

    //쇄도 실제 움직임 - 애니메이션 직접 호출
    public IEnumerator RushMove()
    {
        //이동
        for (int i = 0; i < 10; i++)
        {
            transform.Translate(rushVector * 0.15f);
            yield return new WaitForSeconds(0.03f);
        }

        isRush = false;
        AnimationSet(Anim.Idle);

        isAttack = false;
        AttackMain();
    }

    //직접 컨트롤 시 Skill 버튼과 연동
    public override void Skill2()
    {
        if (isAttack)
            return;

        StartCoroutine(WeaponChange());
    }

    IEnumerator WeaponChange()
    {
        if (cooldowns[1] <= 0 && !isAttack)
        {
            isAttack = true;
            ElementStack = 5;
            StartCoroutine(Skill2Cooldown());

            AnimationSet(Anim.Skill2);

            yield return new WaitForSeconds(0.5f);
            nowWeapon = !nowWeapon;
            AnimationSet("isClaw", nowWeapon);
        }
    }

    public void WeaponChangeEnd()
    {
        isAttack = false;
        AnimationSet(Anim.Idle);

        AttackMain();
    }

    private IEnumerator Skill2Cooldown()
    {
        for (cooldowns[1] = cooldownStart[1]; cooldowns[1] > 0; cooldowns[1] -= 0.05f)
            yield return new WaitForSeconds(0.05f);

        cooldowns[1] = 0;
    }

    //총 스텟 로드
    public override void TotalStatLoad()
    {
        for (int i = 0; i < 10; i++)
            totalStats[i] = stats[i];

        currentHP = (int)totalStats[(int)CharStat.HPMax];
        
        ClawBeforeDelay = 0.4f;

        basicAttackRange = 3.5f;
        basicKnockbackRate = 0.5f;

        ClawAttackRange = 0.85f;
        ClawKnockBackRate = 1f;

        bowKnockBackRate = 0.5f;

        skill1Range = 1.5f;

        basicCooldown = 0;
        cooldowns[0] = cooldowns[1] = 0;
        cooldownStart[0] = 8f;
        cooldownStart[1] = 6f;
    }

    //캐릭터와 관련된 오브젝트 생성 - 총알
    public override void CharInstantiate(int idx)
    {
        if (!PortalPos)
            PortalPos = GameObject.FindWithTag("Portal2").transform;

        for (nowbullet = 0; nowbullet < 13; nowbullet++)
        {
            bullets[nowbullet] = Instantiate(bulletPrefab);
            bullets[nowbullet].GetComponent<bullet>().chaser = this;
            bullets[nowbullet].SetActive(false);
        }

        nowbullet = 0;
        assignIdx = idx;
    }

    public override void BoolInit()
    {
        isRush = false;
        ElementStack = 0;
        base.BoolInit();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*Image 클래스는 모두 애니메이션 트리거 함수를 위한 클래스 - Animator가 있는 오브젝트에 할당되어 있음

public class ChaserImage : MonoBehaviour {

    public Chaser chaser;

    //클로 - 피해 입히기
    public void AttackDamage()
    {
        chaser.AttackDamage();
    }

    //크로스보우 - 사격
    public void Shot()
    {
        chaser.setShotVector();
        chaser.Shot();
    }

    //공격 동작 완료 - 후딜레이 시작
    public void AttackReady()
    {
        if (chaser.isControl)
            StartCoroutine(chaser.BtnAttackReady());
        else
            StartCoroutine(chaser.AttackReady());                     //후딜레이
    }

    //쇄도 - 움직임
    public void Skill1()
    {
        if (chaser.isActiveAndEnabled)
            StartCoroutine(chaser.RushMove());
    }

    public void Skill2()
    {
        chaser.WeaponChangeEnd();
    }

    //공격 이어가기
    public void AttackMain()
    {
        chaser.isAttack = false;
        chaser.AttackMain();
    }
}

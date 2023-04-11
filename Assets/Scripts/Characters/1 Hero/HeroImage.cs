using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroImage : MonoBehaviour {

    public Hero hero;

    public void AttackDamage()
    {
        hero.AttackDamage();
    }

    public void AttackReady()
    {
        if (hero.isControl)
            StartCoroutine(hero.BtnAttackReady());
        else
            StartCoroutine(hero.AttackReady());                      //후딜레이
    }

    public void Skill1Damage()
    {
        hero.StrikeDamage();
    }

    public void Skill2Damage()
    {
        hero.BoomerangGo();
    }

    public void Skill2AttackMain()
    {
        hero.BoomerangAttackMain();
    }

    public void AttackMain()
    {
        hero.isAttack = false;
        hero.AttackMain();
    }
}

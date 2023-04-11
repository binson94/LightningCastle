using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrusherImage : MonoBehaviour {

    public Crusher crusher;

    public void AttackDamage()
    {
        crusher.AttackDamage();
    }

    public void AttackReady()
    {
        if (crusher.isControl)
            StartCoroutine(crusher.BtnAttackReady());
        else
            StartCoroutine(crusher.AttackReady());                       //후딜레이
    }

    public void Skill1Damage()
    {
        crusher.SmiteDamage();
    }

    public void AttackMain()
    {
        crusher.isAttack = false;
        crusher.AttackMain();
    }
}

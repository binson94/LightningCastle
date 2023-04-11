using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianImage : MonoBehaviour {

    public Guardian guardian;

    public void ShieldOn()
    {
        guardian.ShieldOn();
    }

    public void ShieldOff()
    {
        guardian.ShieldOff();
    }

    public void Skill1Damage()
    {
        guardian.ScourgeDamage();
    }

    public void Skill2Move()
    {
        guardian.Skill2Move();
    }

    public void AttackMain()
    {
        if (guardian.isShieldOn)
            guardian.AnimationSet(Anim.Attack);
        else
            guardian.AnimationSet(Anim.Idle);

        guardian.isAttack = false;
        guardian.AttackMain();
    }
}

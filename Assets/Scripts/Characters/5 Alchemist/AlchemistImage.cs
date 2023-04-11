using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemistImage : MonoBehaviour {

    public Alchemist alchemist;

    public void Shot()
    {
        alchemist.SetShotVector();
        alchemist.Shot();
    }

    public void AttackReady()
    {
        if (alchemist.isControl)
            StartCoroutine(alchemist.BtnAttackReady());
        else
            StartCoroutine(alchemist.AttackReady());
    }

    public void Skiil1Throw()
    {
        alchemist.DustThrow();
    }

    public void Skill1End()
    {
        alchemist.DustEnd();
    }

    public void Skill2Sommon()
    {
        alchemist.SommonSpirit();
    }

    public void Skill2End()
    {
        alchemist.SommonEnd();
    }

    public void AttackMain()
    {
        alchemist.AttackMain();
    }
}

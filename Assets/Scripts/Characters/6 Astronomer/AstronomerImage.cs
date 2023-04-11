using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstronomerImage : MonoBehaviour
{
    public Astronomer astronomer;

    public void Charge()
    {
        astronomer.ChargeStart();

        if (!astronomer.isControl)
            astronomer.ChargeEnd();
    }

    public void BasicShot()
    {
        astronomer.SetShotVector();
        astronomer.Shot();
    }

    public void AttackReady()
    {
        if (astronomer.isControl)
            StartCoroutine(astronomer.BtnAttackReady());
        else
            StartCoroutine(astronomer.AttackReady());
    }

    public void Skill1()
    {
        astronomer.MeteorShot();
    }

    public void Skill1End()
    {
        astronomer.MeteorEnd();

        if (!astronomer.isControl)
            astronomer.AttackMain();
    }

    public void Skill2()
    {
        astronomer.MilkyWayStart();
    }

    public void AttackMain()
    {
        astronomer.AttackMain();
    }
}

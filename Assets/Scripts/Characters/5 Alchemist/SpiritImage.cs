using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritImage : MonoBehaviour {

    public Spirit spirit;

    public void AttackDamage()
    {
        spirit.AttackDamage();
    }

    public void AttackReady()
    {
        StartCoroutine(spirit.AttackReady());
    }

    public void AttackMain()
    {
        spirit.AttackMain();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyImage : MonoBehaviour
{
    public Enemy enemy;

    public void AttackDamage()
    {
        enemy.AttackDamage();
    }

    public void AttackReady()
    {
        if (enemy.isActiveAndEnabled)
            StartCoroutine(enemy.AttackReady());
    }
}

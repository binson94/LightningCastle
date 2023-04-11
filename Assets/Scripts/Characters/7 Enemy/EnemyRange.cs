using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRange : MonoBehaviour {

    public Enemy enemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.gameObject.tag == "Player")&& !enemy.targets.Contains(other.GetComponent<Character>()))
        {
            if (enemy.currentTarget == null)
            {
                enemy.currentTarget = other.gameObject.GetComponent<Character>();
                enemy.targets.Add(enemy.currentTarget);
                enemy.AttackMain();
            }
            else
            {
                enemy.targets.Add(other.gameObject.GetComponent<Character>());
            }
        }
    }
}

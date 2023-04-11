using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlWindRange : MonoBehaviour {

    public Crusher crusher;

    public List<Enemy> WhirlWindList;

    public bool skillOn;
    public bool isOverheat;

	// Use this for initialization
	void Start () {
        gameObject.SetActive(false);
	}

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(skillOn)
        {
            Enemy tmpEnemy;
            if (collision.tag == "Enemy")
            {
                tmpEnemy = collision.GetComponent<Enemy>();

                if (!WhirlWindList.Contains(tmpEnemy))
                {
                    WhirlWindList.Add(tmpEnemy);
                    StartCoroutine(WhirlWindDamage(tmpEnemy));
                }

                tmpEnemy.Knockback(1.25f);
            }
        }
    }

    IEnumerator WhirlWindDamage(Enemy enemy)
    {
        enemy.GetDamage((int)(0.6f * Random.Range((int)crusher.totalStats[(int)CharStat.AttackMin], (int)crusher.totalStats[(int)CharStat.AttackMax] + 1)), true, crusher.assignIdx);
        
        if (enemy.DeathCheck())
        {
            crusher.targets.Remove(enemy);
            enemy.ItemDrop();

            if (enemy == crusher.currentTarget)
                crusher.currentTarget = null;
        }

        if (isOverheat)
        {
            yield return new WaitForSeconds(0.4f);
            WhirlWindList.Remove(enemy);
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
            WhirlWindList.Remove(enemy);
        }
    }
}

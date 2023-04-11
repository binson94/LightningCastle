using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour
{
    public Vector2 targetVector;
    float distance;                 //이동 거리
    float range;                    //사거리

    public Astronomer astronomer;   //시전자
    float charge;                   //충전 게이지
    float coeff;                    //충전에 따른 데미지 계수

    bool isAttacked;

    public void MoonStart(float c, float r)
    {
        charge = c;

        if (charge < 0.5f)
            coeff = 1;
        else if (charge < 1f)
            coeff = 1.25f;
        else if (charge < 1.5f)
            coeff = 1.5f;
        else if (charge < 2f)
            coeff = 1.75f;
        else
            coeff = 2;

        Debug.Log(coeff);

        isAttacked = false;
        range = r;

        distance = 0;
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        while(distance < range && !isAttacked)
        {
            transform.Translate(targetVector * 0.15f);
            distance += 0.15f;
            yield return new WaitForSeconds(0.03f);
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" && !isAttacked)
        {
            isAttacked = true;
            AttackDamage(collision.GetComponent<Enemy>());
        }
    }

    //실제 피해를 입히는 함수
    private void AttackDamage(Enemy target)
    {
        target.GetDamage((int)(coeff * Random.Range(astronomer.totalStats[(int)CharStat.AttackMin], astronomer.totalStats[(int)CharStat.AttackMax] + 1)),
            false, astronomer.assignIdx);

        target.Knockback(astronomer.basicKnockbackRate);

        if (target.DeathCheck())
        {
            astronomer.targets.Remove(target);
            target.ItemDrop();

            if (target == astronomer.currentTarget)
                astronomer.currentTarget = null;
        }

        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public Vector2 targetPos;

    public Astronomer astronomer;   //시전자
    public Vector2 moveVector;      //이동 방향 벡터

    float charge;                   //충전 게이지
    float coeff;                    //충전에 따른 범위
    
    bool isAttacked;

    public void MeteorStart(float c)
    {
        charge = c;
        if (charge < 0.5f)
            coeff = 0.4f;
        else if (charge < 1f)
            coeff = 0.55f;
        else if (charge < 1.5f)
            coeff = 0.7f;
        else if (charge < 2f)
            coeff = 0.85f;
        else
            coeff = 1;

        Debug.Log(coeff);

        isAttacked = false;
        
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        float inc = Mathf.Abs(moveVector.y * 0.27f);

        for (float i = 0; i < 4.32f; i += inc)
        {
            transform.Translate(moveVector * 0.27f);

            yield return new WaitForSeconds(0.03f);
        }

        AreaDamage();
        gameObject.SetActive(false);
    }

    void AreaDamage()
    {
        for (int i = 0; i < astronomer.targets.Count; i++)
        {
            if (!astronomer.targets[i].DeathCheck())
                if (Vector3.Distance(astronomer.targets[i].transform.position, targetPos) <= coeff)
                    AttackDamage(astronomer.targets[i]);
        }
    }

    //실제 피해를 입히는 함수
    private void AttackDamage(Enemy target)
    {
        target.GetDamage((int)(2 * Random.Range(astronomer.totalStats[(int)CharStat.AttackMin], astronomer.totalStats[(int)CharStat.AttackMax] + 1)),
            false, astronomer.assignIdx);

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

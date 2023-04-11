using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour {

    public Alchemist alchemist;

    public Vector2 moveVector;
    private bool isAttacked;
    public float range;     //사거리
    float distance;         //이동한 거리

    public Animator[] animArray;
    
    public void PotionStart(float r)
    {
        distance = 0;
        range = r;
        isAttacked = false;
        moveVector = moveVector.normalized;

        StartCoroutine(Move());
    }

    public void AnimationColorSet()
    {
        for (int i = 0; i < 8; i++)
            if ((int)alchemist.charColor / 5 == i)
                animArray[i].gameObject.SetActive(true);
            else
                animArray[i].gameObject.SetActive(false);

    }

    IEnumerator Move()
    {
        while (distance < range)
        {
            transform.Translate(moveVector * 0.105f);
            distance += 0.105f;

            yield return new WaitForSeconds(0.03f);
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((!isAttacked) && other.gameObject.tag == "Enemy")
        {
            isAttacked = true;
            AttackDamage(other.gameObject.GetComponent<Enemy>());
            alchemist.PassiveUpdate();
        }
    }

    private void AttackDamage(Enemy target)        //실제 피해를 입히는 함수
    {
        target.GetDamage(Random.Range((int)alchemist.totalStats[(int)CharStat.AttackMin], (int)alchemist.totalStats[(int)CharStat.AttackMax] + 1), false, alchemist.assignIdx);
        
        //넉백

        if (target.DeathCheck())
        {
            alchemist.targets.Remove(target);
            target.ItemDrop();
            
            if (target == alchemist.currentTarget)
                alchemist.currentTarget = null;
        }

        gameObject.SetActive(false);
    }
}

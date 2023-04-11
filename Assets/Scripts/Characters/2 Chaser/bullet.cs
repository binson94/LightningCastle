using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour {
    
    public Vector2 moveVector;
    public Chaser chaser;
    private bool isAttacked;
    
    public void BulletStart()
    {
        isAttacked = false;
        moveVector = moveVector.normalized;
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        while(true)
        {
            transform.Translate(moveVector * 0.18f);
            yield return new WaitForSeconds(0.03f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "ShoterRange")
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((!isAttacked) && other.gameObject.tag == "Enemy")
        {
            isAttacked = true;
            AttackDamage(other.gameObject.GetComponent<Enemy>());
        }
    }

    private void AttackDamage(Enemy target)        //실제 피해를 입히는 함수
    {
        if (chaser.ElementStack > 0)
        {
            chaser.ElementStack--;
            target.GetDamage((int)(1.5f * Random.Range((int)chaser.totalStats[(int)CharStat.AttackMin], (int)chaser.totalStats[(int)CharStat.AttackMax] + 1)), true, chaser.assignIdx);
        }
        else
        {
            target.GetDamage(Random.Range((int)chaser.totalStats[(int)CharStat.AttackMin], (int)chaser.totalStats[(int)CharStat.AttackMax] + 1), true, chaser.assignIdx);
        }

        target.Knockback(chaser.bowKnockBackRate);

        if (target.DeathCheck())
        {
            chaser.targets.Remove(target);
            target.ItemDrop();

            //Passive : 적 처치 시 쿨타임 감소
            chaser.cooldowns[0] -= 2;
            chaser.cooldowns[1] -= 2;
            for (int i = 0; i < 2; i++)
                if (chaser.cooldowns[i] < 0)
                    chaser.cooldowns[i] = 0;
            
            if (target == chaser.currentTarget)
                chaser.currentTarget = null;
        }

        gameObject.SetActive(false);
    }
}

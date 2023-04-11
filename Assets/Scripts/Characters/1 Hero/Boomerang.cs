using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour {

    public Vector2 TargetVector;
    public Hero hero;
    private float Range;

    BoomerangImage image;
    Animator anim;
    public Animator[] animArray;

    private int nowCondition;

    public void AnimationColorSet()
    {
        for (int i = 0; i < 8; i++)
        {
            if ((int)hero.charColor / 5 == i)
            {
                anim = animArray[i];
                anim.gameObject.SetActive(true);
                image = anim.GetComponent<BoomerangImage>();
            }
            else
                animArray[i].gameObject.SetActive(false);
        }
    }

    public void BoomerangStart(float r)
    {
        Range = r;
        StartCoroutine(Move());
        image.ImageStart();
    }

    private IEnumerator Move()
    {
        nowCondition = 0;
        for (float i = 0; i < Range; i += 0.12f)
        {
            transform.Translate(TargetVector * 0.12f);
            yield return new WaitForSeconds(0.03f);
        }

        hero.BoomerangList.Clear();
        nowCondition = 1;

        while (true)
        {
            TargetVector = (hero.transform.position - transform.position).normalized;
            transform.Translate(TargetVector * 0.135f);
            yield return new WaitForSeconds(0.03f);

            if (Vector3.Distance(transform.position, hero.transform.position) < 0.3f)
            {
                hero.withBoomerang.enabled = true;
                hero.withoutBoomerang.enabled = false;
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Enemy nowEnemy = other.GetComponent<Enemy>();
            if (!hero.BoomerangList.Contains(nowEnemy))
            {
                hero.BoomerangList.Add(nowEnemy);
                AttackDamage(nowEnemy);
            }
        }
        else if (other.GetComponent<Hero>() == hero)
            if (nowCondition == 1)
            {
                hero.withBoomerang.enabled = true;
                hero.withoutBoomerang.enabled = false;
                gameObject.SetActive(false);
            }
    }

    private void AttackDamage(Enemy target)        //실제 피해를 입히는 함수
    {
        target.GetDamage(Random.Range((int)hero.totalStats[(int)CharStat.AttackMin], (int)hero.totalStats[(int)CharStat.AttackMax] + 1), true, hero.assignIdx);
    
        if (target.DeathCheck())
        {
            if(hero.targets.Contains(target))
                hero.targets.Remove(target);

            target.ItemDrop();
            target.gameObject.SetActive(false);

            if (target == hero.currentTarget)
                hero.currentTarget = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//적이 성으로 침입하는 것을 감지하는 클래스
//적이 2층 왼쪽 끝까지 침투하였을 때, 성 체력 감소
public class LastGate : MonoBehaviour
{
    public NightManager nightM;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            nightM.EnemyInvasion((int)collision.GetComponent<Enemy>().stats[(int)EnemyStat.Level]);
            collision.GetComponent<Enemy>().currentHP = 0;
            collision.gameObject.SetActive(false);
        }
    }
}

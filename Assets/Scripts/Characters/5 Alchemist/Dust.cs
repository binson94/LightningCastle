using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dust : MonoBehaviour {
    
    public Vector2 moveVector;
    public Alchemist alchemist;
    public float range;
    float distance;
    List<Collider2D> attachedList = new List<Collider2D>();  //이미 버프 받은 캐릭터들
    
    public void DustStart(float r)
    {
        attachedList.Clear();
        distance = 0;
        range = r;
        moveVector = moveVector.normalized;
        StartCoroutine(Move());
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
        if (!attachedList.Contains(other) && other.gameObject.tag == "Player")
        {
            attachedList.Add(other);
            other.GetComponent<Character>().DamageBuff(alchemist);
            alchemist.PassiveUpdate();
        }
    }
}

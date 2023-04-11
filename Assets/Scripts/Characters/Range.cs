using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour
{
    public Character character;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.gameObject.tag == "Enemy") && (!character.targets.Contains(other.gameObject.GetComponent<Enemy>())))
        {
            if (character.currentTarget == null)
            {
                character.currentTarget = other.gameObject.GetComponent<Enemy>();
                character.targets.Add(character.currentTarget);
                character.AttackMain();
            }
            else
            {
                character.targets.Add(other.gameObject.GetComponent<Enemy>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy" && other.gameObject.GetComponent<Enemy>().currentHP > 0)
        {
            character.targets.Remove(other.gameObject.GetComponent<Enemy>());
            if (other.gameObject.GetComponent<Enemy>() == character.currentTarget)
            {
                character.currentTarget = null;
                if (character.isChase)
                {
                    StopCoroutine(character.chaseCoroutine);
                    character.isChase = false;
                }
                character.AttackMain();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//1층에서 2층으로 올라가는 포탈 클래스
public class UpStair : MonoBehaviour {

    public GameObject TeleportPos;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Character tmpChar = collision.GetComponent<Character>();

            if(tmpChar.isCanTeleportLeft)
            {
                collision.gameObject.transform.position = TeleportPos.transform.position;

                if (!tmpChar.isControl && !tmpChar.isGather)
                    tmpChar.TeleportHold();
            }
        }
        else if(collision.tag == "Enemy")
        {
            collision.gameObject.transform.position = TeleportPos.transform.position;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<Character>().isCanTeleportLeft = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//2층에서 1층으로 내려가는 포탈 클래스
public class DownStair : MonoBehaviour {

    public Transform teleportPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Character tmpChar = collision.GetComponent<Character>();
            tmpChar.isCanTeleport = true;
            tmpChar.teleportPos = teleportPoint.position;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Character tmpChar = collision.GetComponent<Character>();
            tmpChar.isCanTeleport = false;
        }
    }
}

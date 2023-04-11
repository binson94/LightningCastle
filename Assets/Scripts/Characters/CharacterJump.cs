using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//발코니 통과를 위한 클래스
public class CharacterJump : MonoBehaviour
{
    public BoxCollider2D charCol;

    //점프했는데 발코니에 닿았을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Balcony")
        {
            charCol.isTrigger = true;
        }
        else if(collision.tag == "Platform")
        {
            charCol.isTrigger = false;
        }
    }
}

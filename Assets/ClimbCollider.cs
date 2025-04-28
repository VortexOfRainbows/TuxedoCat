using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClimbCollider : MonoBehaviour
{
    public CharacterAnimator anim;
    public void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("World") || collision.CompareTag("Standable")))
        {
            if (collision.gameObject.layer == 10)
            {
                anim.Climbing = true;
                if (collision.gameObject.transform.position.x < transform.position.x)
                    anim.ClimbDir = -1;
                else
                    anim.ClimbDir = 1;
            }
        }
    }
}

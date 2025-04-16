using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goon : MonoBehaviour
{
    public CharacterAnimator anim;
    public void FixedUpdate()
    {
        anim.Animate();
    }
}

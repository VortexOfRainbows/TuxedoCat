using UnityEngine;

public class BiggieCheese : MonoBehaviour
{
    public CharacterAnimator anim;
    public void FixedUpdate()
    {
        anim.Animate();
    }
}

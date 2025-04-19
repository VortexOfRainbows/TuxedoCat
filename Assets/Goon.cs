using UnityEngine;

public class Goon : MonoBehaviour
{
    public GameObject target = null;
    public void TryFindTarget()
    {
        float dist = 10;
        GameObject player = Player.Instance.gameObject;
        bool hasLineOfSight = true;
        //do a raytrace here to check for LOS
        if(hasLineOfSight)
        {
            target = player;
        }
        else
        {
            target = null;
        }
    }
    public Rigidbody2D RB => anim.RB;
    public CharacterAnimator anim;
    private float Dir
    {
        get => anim.Dir;
        set => anim.Dir = value;
    }
    public bool TouchingGround
    {
        get => anim.TouchingGround;
        set => anim.TouchingGround = value;
    }
    public void FixedUpdate()
    {
        TryFindTarget();
        if(target != null)
        {
            anim.eyeTargetPosition = target.transform.position;
            anim.TargetFront = false;
        }
        else
        {
            anim.TargetFront = true;
        }
        if (Mathf.Abs(RB.velocity.x) > 0.1f)
            Dir = Mathf.Sign(RB.velocity.x);
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 5 : 10;
        float inertia = TouchingGround ? 0.05f : 0.0225f;
        //float jumpForce = 10f;

        //if (Control.Left)
        //    targetVelocity.x -= topSpeed;
        //if (Control.Right)
        //    targetVelocity.x += topSpeed;

        if (TouchingGround)
            velo.x = Mathf.Lerp(velo.x, targetVelocity.x, inertia);
        else
        {
            velo += targetVelocity * inertia;
            velo.x *= 1 - inertia;
        }
        RB.velocity = velo;
        anim.Animate();
    }
}

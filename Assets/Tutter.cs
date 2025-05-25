
using UnityEngine;

public class Tutter : Goon
{
    bool TiedUp = true;
    public new void FixedUpdate()
    {
        if (startVelo != Vector2.zero)
        {
            RB.velocity = startVelo * 0.5f;
            startVelo = Vector2.zero;
        }
        Vector3 targetPatrolPoint = transform.position;
        TryFindTarget();
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 2 : 4;
        float inertia = TouchingGround ? 0.04f : 0.02f;

        if (transform.position != targetPatrolPoint && !TiedUp)
        {
            Vector3 toPatrol = targetPatrolPoint - transform.position;
            float distToTargetX = Mathf.Abs(toPatrol.x);
            if (distToTargetX > 3)
                targetVelocity.x += Mathf.Sign(toPatrol.x) * topSpeed;
            if (distToTargetX < 1)
                targetVelocity.x -= Mathf.Sign(toPatrol.x) * topSpeed;
        }

        if (TouchingGround)
            velo.x = Mathf.Lerp(velo.x, targetVelocity.x, inertia);
        else
        {
            velo += targetVelocity * inertia;
            velo.x *= 1 - inertia;
        }
        RB.velocity = velo;
        anim.Animate();
        anim.ArmLeft.GetComponent<SpriteRenderer>().sortingOrder = -1;
    }
}

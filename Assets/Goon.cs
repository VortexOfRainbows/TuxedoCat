using System.Net.Mail;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Goon : MonoBehaviour
{
    private bool dead = false;
    public void Die()
    {
        dead = true;
        foreach (SpriteRenderer sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr != anim.ItemSprite)
                sr.color = Color.white;
        }
        Debug.Log("I DIE");
        anim.LegLeft.AddComponent<Gore>();
        anim.ArmRight.AddComponent<Gore>();
        anim.ArmLeft.AddComponent<Gore>();
        anim.LegRight.AddComponent<Gore>();
        anim.Body.AddComponent<Gore>();
        anim.Head.AddComponent<Gore>();
        Destroy(gameObject);
    }
    public Color color = Color.white;
    public GameObject target = null;
    public float Alertness = 0;
    public int life = 5;
    public float hurtTimer = 0;
    public void Hurt(int damage)
    {
        life -= damage;
        Alertness += 0.5f;
        hurtTimer = 10;
        if(life <= 0 && !dead)
        {
            Die();
        }
    }
    public void TryFindTarget()
    {
        float dist = 10;
        GameObject player = Player.Instance.gameObject;
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer = toPlayer.normalized;
        bool hasLineOfSight = false;
        RaycastHit2D ray = Physics2D.Raycast(transform.position + toPlayer * 1.0f, toPlayer, dist, LayerMask.GetMask("Player", "World"));
        if(ray.distance < dist && ray.collider != null && ray.collider.CompareTag("Player"))
        {
            hasLineOfSight = true;
        }
        //do a raytrace here to check for LOS
        if(hasLineOfSight)
        {
            target = player;
            if(Alertness < 2)
            {
                Alertness += Time.fixedDeltaTime;
            }
            else
            {
                Alertness = 0;
            }
        }
        else
        {
            if (Alertness > 0)
            {
                Alertness -= Time.fixedDeltaTime;
            }
            else
            {
                target = null;
                Alertness = 0;
            }
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
        if (dead)
            return;
        Vector3 targetPatrolPoint = transform.position;
        TryFindTarget();
        if(target != null)
        {
            Vector3 toPlayer = target.transform.position - transform.position;
            anim.eyeTargetPosition = target.transform.position;
            anim.TargetFront = false;
            anim.armTargetPos = target.transform.position;
            targetPatrolPoint = target.transform.position;
            Dir = Mathf.Sign(toPlayer.x);
        }
        else
        {
            anim.TargetFront = true;
            anim.armTargetPos = Vector2.zero;
            if (Mathf.Abs(RB.velocity.x) > 0.1f)
                Dir = Mathf.Sign(RB.velocity.x);
        }
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 2 : 4;
        float inertia = TouchingGround ? 0.04f : 0.02f;
        //float jumpForce = 10f;

        if(transform.position != targetPatrolPoint)
        {
            Vector3 toPatrol = targetPatrolPoint - transform.position;
            float distToTargetX = Mathf.Abs(toPatrol.x);
            if(distToTargetX > 7)
                targetVelocity.x += Mathf.Sign(toPatrol.x) * topSpeed;
            if (distToTargetX < 4)
                targetVelocity.x -= Mathf.Sign(toPatrol.x) * topSpeed;
        }

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

        if(--hurtTimer >= 0)
            color = Color.Lerp(color, Color.red, 0.12f);
        else
            color = Color.Lerp(color, Color.white, 0.15f);

        foreach (SpriteRenderer sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if(sr != anim.ItemSprite)
                sr.color = color;
        }
    }
}

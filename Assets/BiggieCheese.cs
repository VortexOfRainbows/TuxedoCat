using UnityEngine;
using UnityEngine.EventSystems;

public class BiggieCheese : MonoBehaviour
{
    public Player target => Player.Instance;
    public CharacterAnimator anim;
    public Rigidbody2D rb => anim.RB;
    public bool FightInitiated = false;
    public void FixedUpdate()
    {
        if(Vector2.Distance(target.transform.position, transform.position) < 10)
        {
            FightInitiated = true;
        }
        if (FightInitiated)
            BossAI();
        anim.Animate();
        anim.prevTouchingGround = true;
        anim.TouchingGround = false;
    }
    public float AiCounter = 0;
    public void BossAI()
    {
        anim.oldVelo = rb.velocity;
        if(Mathf.Abs(anim.oldVelo.x) > 0.1f)
            anim.Dir = Mathf.Sign(anim.oldVelo.x);
        Vector2 velo = rb.velocity;
        Vector2 toPlayer = Player.Position - transform.position;
        AiCounter++;
        if(AiCounter > 200)
        {
            if(anim.TouchingGround)
            {
                velo.y += 10;
            }
            AiCounter = 0;
        }
        else
        {
           velo.x *= 0.94f;
           velo.x += Mathf.Sign(toPlayer.x) * 0.2f;
        }
        rb.velocity = velo;
    }
}

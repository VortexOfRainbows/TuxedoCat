using UnityEngine;
using UnityEngine.EventSystems;

public class BiggieCheese : Goon
{
    public override void Die()
    {
        CheeseWheel.SetActive(false);
        base.Die();
        Main.Instance.CheeseBarImage.color = color;
        Main.Instance.CheeseBarImage.fillAmount = 0;
        FightInitiated = false;
    }
    public Rigidbody2D rb => anim.RB;
    public static bool FightInitiated = false;
    public Vector3 startPos = Vector3.zero;
    public new void FixedUpdate()
    {
        target = Player.Instance.gameObject;
        if (startPos == Vector3.zero)
            startPos = transform.position;
        if (Player.Instance.Dead)
        {
            transform.position = startPos;
            AiCounter = 0;
            AiPhase = 0;
            FightInitiated = false;
            anim.TPose = false;
            rb.velocity *= 0.9f;
            transform.localScale = Vector3.one;
            transform.localEulerAngles = Vector3.zero;
            CheeseWheel.SetActive(false);
        }
        if (Vector2.Distance(target.transform.position, transform.position) < 10)
        {
            FightInitiated = true;
        }
        if (FightInitiated)
        {
            BossAI();
        }
        else
        {
            Main.Instance.CheeseBarImage.color = Color.white;
            Main.Instance.CheeseBarImage.fillAmount = 1;
            life = 100;
        }
        anim.Animate();
        CheeseWheelAnim();
        anim.prevTouchingGround = true;
        anim.TouchingGround = false;

        if (--hurtTimer >= 0)
            color = Color.Lerp(color, Color.red, 0.12f);
        else
            color = Color.Lerp(color, Color.white, 0.15f);

        foreach (SpriteRenderer sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr != anim.ItemSprite)
                sr.color = color;
        }
        Main.Instance.CheeseBarImage.color = color;
        Main.Instance.CheeseBarImage.fillAmount = life / 100f;
    }
    public float AiCounter = 0;
    public int AiPhase = 0;
    public GameObject CheeseWheel;
    public void BossAI()
    {
        anim.oldVelo = rb.velocity;
        if(Mathf.Abs(anim.oldVelo.x) > 0.1f)
            anim.Dir = Mathf.Sign(anim.oldVelo.x);
        Vector2 velo = rb.velocity;
        Vector2 toPlayer = Player.Position - transform.position;

        if(AiPhase == 0)
        {
            ++AiCounter;
            if (AiCounter > 200)
            {
                if (anim.TouchingGround)
                {
                    velo.y += 14.5f;
                    anim.TouchingGround = false;
                }
                if (AiCounter > 440)
                {
                    AiPhase++;
                    AiCounter = 0;
                    velo.y += 15.0f;
                }
                else
                {
                    velo.x += toPlayer.x * 0.01f;
                    velo.x *= 0.97f;
                    if (cheeseWheelTimer < 40)
                        cheeseWheelTimer++;
                    if (AiCounter % 60 == 0)
                    {
                        Projectile.NewProjectile<CheeseWheel>(transform.position + new Vector3(0, 2), toPlayer.normalized * 5, 0, 0);
                    }
                    if (velo.y < 0)
                        velo.y *= 0.5f;
                }
            }
        }
        else
        {
            if (cheeseWheelTimer > 0)
                cheeseWheelTimer--;
        }
        if(AiPhase == 1)
        {
            if(AiCounter > 5)
                ++AiCounter;
            else
            {
                velo.y += -0.12f;
            }
            if (anim.TouchingGround)
            {
                if(AiCounter < 6)
                {
                    AiPhase = 2;
                    AiCounter = -50;
                    for (int i = -1; i <= 1; i += 2)
                        Projectile.NewProjectile<CheeseWheel>(transform.position + new Vector3(0, -1.5f), new Vector2(i * 6, 0.6f), 1, 0);
                    for (int i = -3; i <= 3; ++i)
                    {
                        Projectile.NewProjectile<Cheese>(transform.position, new Vector2(0, 16).RotatedBy(i * 3.5f * Mathf.Deg2Rad), -0.5f);
                    }
                }
            }
        }
        if(AiPhase == 2 || AiPhase == 3)
        {
            anim.TPose = true;
            if(AiPhase == 2)
                AiCounter++;
            if(AiPhase == 3)
                AiCounter--;
            if(AiCounter >= 0)
            {
                if (AiCounter > 280)
                {
                    AiPhase = 3;
                }
                if (AiCounter <= 0 && AiPhase == 3)
                {
                    AiPhase = 0;
                }
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + AiCounter * 0.05f, transform.localEulerAngles.z);
                velo.x += toPlayer.x * 0.02f;
                if (AiCounter > 80)
                {
                    velo.y += rb.gravityScale * Time.fixedDeltaTime + 0.178f;
                }
                if (AiCounter % 6 == 0 && AiCounter > 100)
                {
                    Projectile.NewProjectile<Cheese>(transform.position + new Vector3(0, 1.5f), new Vector2(0, 8f) + Utils.RandCircle(2) + rb.velocity * 1.0f, -0.5f);
                }
            }
            velo.x *= 0.995f;
        }
        else
        {
            anim.TPose = false;
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.LerpAngle(transform.localEulerAngles.y, 0, 0.1f), transform.localEulerAngles.z);
        }
        if (anim.TouchingGround)
        {
           velo.x *= 0.94f;
           velo.x += Mathf.Sign(toPlayer.x) * 0.2f;
        }
        rb.velocity = velo;

    }
    public float cheeseWheelTimer = 0;
    public void CheeseWheelAnim()
    {
        CheeseWheel.SetActive(true);
        Vector2 toPlayer = Player.Position - transform.position;
        float percent = cheeseWheelTimer / 40f;
        if (percent > 1) percent = 1;
        float cheeseScale = percent + Mathf.Sin(percent * Mathf.PI) * 0.4f;
        CheeseWheel.transform.localScale = new Vector3(Mathf.Sign(toPlayer.x) * anim.Dir, 1, 1) * cheeseScale * 1.25f;
        toPlayer.x = Mathf.Abs(toPlayer.x);
        toPlayer.y *= 0.25f;
        CheeseWheel.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(CheeseWheel.transform.localEulerAngles.z, toPlayer.ToRotation() * Mathf.Rad2Deg, 0.05f);
    }
}

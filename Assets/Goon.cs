using UnityEngine;

public class Goon : MonoBehaviour
{
    private bool dead = false;
    public Vector2 startVelo = Vector2.zero;
    public virtual void Die()
    {
        dead = true;
        foreach (SpriteRenderer sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr != anim.ItemSprite && sr.gameObject.activeSelf) {
                sr.color = Color.white;
                sr.gameObject.transform.eulerAngles = new Vector3(0, 0, sr.gameObject.transform.localEulerAngles.z);
                sr.gameObject.transform.localScale = new Vector3(1, 1, 1);
                sr.gameObject.transform.position = new Vector3(sr.gameObject.transform.position.x, sr.gameObject.transform.position.y, 0);
            }
        }
        anim.LegLeft.AddComponent<Gore>();
        anim.ArmRight.AddComponent<Gore>();
        anim.ArmLeft.AddComponent<Gore>();
        anim.LegRight.AddComponent<Gore>();
        anim.Body.AddComponent<Gore>();
        anim.Head.AddComponent<Gore>();
        anim.Eyes.SetActive(false);
        for(int i = 0; i < 30; ++i)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(1f, 2f), RB.velocity * Utils.RandFloat(0.5f, 2f), 5f, Utils.RandFloat(4f, 5f), 1);
        }
        for (int i = 0; i < 7; ++i)
        {
            GameObject g = Projectile.NewProjectile<Cheese>(anim.ItemSprite.transform.position, Utils.RandCircle(3) + Vector2.up * 3);
            g.GetComponent<ProjComponents>().rb.gravityScale = Utils.RandFloat(0.3f, 0.5f);
            g.GetComponent<Projectile>().Hostile = false;
            g.transform.localScale *= Utils.RandFloat(0.7f, 1.05f);
        }
        AudioManager.PlaySound(SoundID.Explosion, transform.position, 1, 1);
        Destroy(gameObject);
    }
    public Color color = Color.white;
    public GameObject target = null;
    public float Alertness = 0;
    public int life = 7;
    public float hurtTimer = 0;
    public void Hurt(int damage)
    {
        life -= damage;
        Alertness += 0.5f;
        hurtTimer = 10;
        for (int i = 0; i < 3; ++i)
            ParticleManager.NewParticle(transform.position, 1, RB.velocity * Utils.RandFloat(0.5f, 1f), 4f, Utils.RandFloat(2f, 3f), 1);
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
    public int shootCounter = 0;
    public void FixedUpdate()
    {
        if(startVelo != Vector2.zero)
        {
            RB.velocity = startVelo * 0.5f;
            startVelo = Vector2.zero;
        }
        if (dead)
            return;
        Vector3 targetPatrolPoint = transform.position;
        TryFindTarget();
        if(target != null)
        {
            Vector3 toPlayer = (target.transform.position - transform.position).normalized;
            anim.eyeTargetPosition = target.transform.position;
            anim.TargetFront = false;
            anim.armTargetPos = target.transform.position;
            targetPatrolPoint = target.transform.position;
            Dir = Mathf.Sign(toPlayer.x);
            shootCounter++;
            if(shootCounter > 120)
            {
                shootCounter = -Utils.RandInt(41);
                toPlayer.y += Mathf.Abs(toPlayer.magnitude) * 0.125f;
                Projectile.NewProjectile<Cheese>(anim.ItemSprite.transform.position + toPlayer * 0.5f, toPlayer * 8);
                anim.useRecoil += 30;
            }
        }
        else
        {
            shootCounter = 0;
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

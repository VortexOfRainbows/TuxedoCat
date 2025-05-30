using System.Linq;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public static GameObject ProjPrefab => Resources.Load<GameObject>("Proj/ProjPrefab");
    public ProjComponents cmp;
    public SpriteRenderer SR => cmp.spriteRenderer;
    public Rigidbody2D RB => cmp.rb;
    public CircleCollider2D c2D => cmp.c2D;
    public float timer = 0f;
    public float timer2 = 0f;
    public float[] Data;
    public int Damage = 1;
    public int Penetrate = 1;
    public bool Friendly = false;
    private bool Dead = false;
    public bool Hostile = false;
    public bool TileCollide = false;
    public Vector2 startPos = Vector2.zero;
    public static GameObject NewProjectile<T>(Vector2 pos, Vector2 velo, params float[] data) where T : Projectile
    {
        GameObject Proj = Instantiate(ProjPrefab, pos, Quaternion.identity);
        Projectile proj = Proj.AddComponent<T>();
        proj.cmp = Proj.GetComponent<ProjComponents>();
        proj.RB.velocity = velo;
        proj.Data = data;
        proj.Init();
        return Proj;
    }
    public void Kill()
    {
        if (!Dead)
            Dead = true;
        else
            return;
        OnKill();
        Destroy(gameObject);
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 7 && Friendly) //if it is an enemy
        {
            Goon goon = collision.gameObject.GetComponent<Goon>();
            if(goon != null && Penetrate > 0 )
            {
                if(goon is Tutter tutter)
                {
                    tutter.FreeMe();
                }
                else
                {
                    --Penetrate;
                    goon.Hurt(1);
                    OnHit(goon);
                }
            }
            else if(collision.TryGetComponent<BiggieCheese>(out BiggieCheese c) && Penetrate > 0 && BiggieCheese.FightInitiated)
            {
                --Penetrate;
                c.Hurt(1);
                OnHit(c);
            }
        }
        if (collision.gameObject.layer == 6 && Hostile) //if it is a player
        {
            Player p = collision.gameObject.GetComponent<Player>();
            if (p != null && Penetrate > 0)
            {
                p.Hurt(1);
            }
            Kill();
        }
        if(TileCollide && collision.gameObject.layer == 3) //collide with world
        {
            Kill();
        }
    }
    public virtual void OnHit(Goon target)
    {

    }
    public virtual void Init()
    {
        FixedUpdate();
    }
    public void FixedUpdate()
    {
        AI();
    }
    public virtual void AI()
    {

    }
    public virtual void OnKill()
    {

    }
}

public class Laser : Projectile
{
    public override void Init()
    {
        cmp.c2D.offset = new Vector2(1, 0);
        cmp.spriteRenderer.material = Resources.Load<Material>("Additive");
        Friendly = true;
        Hostile = false;
        AudioManager.PlaySound(SoundID.Laser, transform.position, 0.6f, Utils.RandFloat(1.2f, 1.4f));
    }
    public void SpawnParticles()
    {
        Color c = cmp.spriteRenderer.color * 1.5f;
        Vector2 destination = new Vector2(Data[0], Data[1]);
        Vector2 toDest = destination - (Vector2)transform.position;
        float scaleX = toDest.magnitude;
        for (int j = 0; j < 12; ++j)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.5f, 1f), RB.velocity * Utils.RandFloat(1f, 10f), 1.5f, Utils.RandFloat(0.45f, 0.6f), 0, c);
        }
        for (float i = 0; i < scaleX; i += 0.2f)
        {
            Vector2 inBetween = Vector2.Lerp(transform.position, destination, i / scaleX);
            ParticleManager.NewParticle(inBetween, Utils.RandFloat(0.5f, 0.1f), RB.velocity * Utils.RandFloat(0.1f, 0.7f), 3f, Utils.RandFloat(0.4f, 0.5f), 0, c);
        }
        for (int j = 0; j < 20; ++j)
        {
            ParticleManager.NewParticle(destination, Utils.RandFloat(0.5f, 1f), RB.velocity * Utils.RandFloat(0.5f, 2f), 3f, Utils.RandFloat(0.45f, 0.6f), 0, c);
        }
    }
    public override void AI()
    {
        if(timer == 0)
        {
            SpawnParticles();
        }
        ++timer;
        if (timer <= 11)
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.y, 0.335f, 0.16f), 1);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.y, 0f, 0.078f), 1);
            cmp.spriteRenderer.color *= 0.96f;
            if (transform.lossyScale.y < 0.01f)
                timer = 101;
            if (timer > 20)
                Friendly = false; //turn off hitbox after a bit
        }
        timer++;
        if (timer > 100)
            Kill();
    }
    public override void OnKill()
    {

    }
}
public class Cheese : Projectile
{
    public override void Init()
    {
        transform.localScale *= 0.7f;
        cmp.c2D.radius *= 0.8f;
        cmp.c2D.offset = new Vector2(0, 0);
        cmp.spriteRenderer.sprite = Resources.Load<Sprite>("Proj/Cheese");
        Friendly = false;
        Hostile = true;
        TileCollide = true;
        RB.gravityScale = 0.1f;
        if (Data.Length > 0)
        {
            RB.gravityScale = -Data[0];
        }
        AI();
        for (int i = 0; i < 15; ++i)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.6f, 1.2f), RB.velocity * Utils.RandFloat(0.5f, 1.5f) + Vector2.up, 2f, Utils.RandFloat(0.7f, 1f), 1);
        }
    }
    public override void AI()
    {
        RB.rotation = Mathf.Rad2Deg * RB.velocity.ToRotation();
        cmp.spriteRenderer.flipY = RB.velocity.x < 0;
        if(Utils.RandFloat() < 0.2f)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.5f, 1.1f), (RB.velocity + Vector2.up) * Utils.RandFloat(0.5f), 1f, Utils.RandFloat(0.6f, 0.7f), 1);
        }
    }
    public override void OnKill()
    {
        for (int i = 0; i < 25; ++i)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.6f, 1.2f), RB.velocity * Utils.RandFloat(-0.25f, 0.9f) + Vector2.up, 1.5f, Utils.RandFloat(0.7f, 1f), 1);
        }
    }
}
public class CatClawSlash : Projectile
{
    public override void Init()
    {
        transform.localScale *= 2f;
        cmp.c2D.offset = new Vector2(-0.3f, 0);
        cmp.c2D.radius = .6f;
        cmp.spriteRenderer.sprite = null;
        cmp.spriteRenderer.color = new Color(1, 1, 1, 0.1f);
        Friendly = true;
        Hostile = false;
        TileCollide = false;
        RB.gravityScale = 0.0f;
        Penetrate = 3;
        Vector2 tilt = RB.velocity.normalized * Data[0];
        Vector2 dir = tilt.RotatedBy(Mathf.Deg2Rad * 90);
        for (int i = -1; i <= 1; ++i)
        {
            float size = i == 0 ? 2 : 1.7f;
            float speed = i == 0 ? 21f : 17f;
            float lifeTime = i == 0 ? 0.21f : 0.19f;
            ParticleManager.NewParticle((Vector2)transform.position - (dir * speed * lifeTime / 2.4f) + tilt * i * 0.29f, size, dir * speed + RB.velocity, 0, lifeTime, 2);
        }
        AudioManager.PlaySound(SoundID.Blade, transform.position, 0.7f, Utils.RandFloat(0.6f, 0.9f));
        AI();
    }
    public override void AI()
    {
        RB.rotation = Mathf.Rad2Deg * RB.velocity.ToRotation();
        cmp.spriteRenderer.flipY = RB.velocity.x < 0;
        timer++;
        if(timer > 3 && timer < 10)
        {
            Friendly = true;
        }
        else
        {
            Friendly = false;
        }
        if (timer > 20)
            Kill();
    }
    public override void OnKill()
    {
    }
    public override void OnHit(Goon target)
    {
        for(int i = 0; i < 3; ++i)
        {
            Vector2 circular = Utils.RandCircle(1.5f);
            ParticleManager.NewParticle((Vector2)target.transform.position + circular, 0.8f, -circular * 10f, 0.25f, 0.25f, 2);
        }
    }
}
public class CheeseWheel : Projectile
{
    public override void Init()
    {
        transform.localScale *= 0.1f;
        cmp.c2D.radius *= 1.5f;
        cmp.c2D.offset = new Vector2(0, 0);
        cmp.spriteRenderer.sprite = Resources.Load<Sprite>("Proj/CheeseWheel");
        Friendly = false;
        Hostile = true;
        TileCollide = true;
        RB.gravityScale = 0.05f;
        AI();
        for (int i = 0; i < 15; ++i)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.6f, 1.2f), RB.velocity * Utils.RandFloat(0.5f, 1.5f) + Vector2.up, 2f, Utils.RandFloat(0.7f, 1f), 1);
        }
    }
    public override void AI()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.9f, 0.05f);
        RB.velocity *= 1.0065f;
        RB.rotation = Mathf.Rad2Deg * RB.velocity.ToRotation();
        cmp.spriteRenderer.flipY = RB.velocity.x < 0;
        if (Utils.RandFloat() < 0.4f)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.7f, 2f), (RB.velocity + Vector2.up) * Utils.RandFloat(0.5f), 1f, Utils.RandFloat(0.6f, 0.7f), 1);
        }
    }
    public override void OnKill()
    {
        for (int i = -2; i <= 2; ++i)
        {
            Projectile.NewProjectile<Cheese>(transform.position + new Vector3(0f, 0.5f, 0f), new Vector2(0, 16 - Mathf.Abs(i) * 2).RotatedBy(i * 1.5f * Mathf.Deg2Rad) + RB.velocity * 0.1f, -1);
        }
        for (int i = 0; i < 25; ++i)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.7f, 2f), RB.velocity * Utils.RandFloat(-0.25f, 0.9f) + Vector2.up, 3.5f, Utils.RandFloat(0.7f, 1.1f), 1);
        }
    }
}

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
        if(collision.gameObject.layer == 7) //if it is an enemy
        {
            Goon goon = collision.gameObject.GetComponent<Goon>();
            if(goon != null && Penetrate >= 0)
            {
                --Penetrate;
                goon.Hurt(1);
            }
        }
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

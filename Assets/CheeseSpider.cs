using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;

public class CheeseSpider : MonoBehaviour
{
    public static CheeseSpider Instance;
    public static bool Active => Instance != null && Instance.IsActive;
    public bool IsActive = false;
    public GameObject Visual;
    public GameObject Body;
    public GameObject[] legs;
    public GameObject[] joints;
    public Vector3 oldVelo;
    public float AnimCounter = 0;
    public Rigidbody2D RB;
    private float Dir = 1;
    public bool TouchingGround = false;
    public Collider2D c2D;
    public float AliveTimer = 0;
    public void Start()
    {
        TouchingGround = false;
        Instance = this;
    }
    public void MovementUpdate()
    {
        AliveTimer += Time.fixedDeltaTime;
        Instance = this;
        if (Mathf.Abs(RB.velocity.x) > 0.1f)
            Dir = Mathf.Sign(RB.velocity.x);
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 5 : 6;
        float inertia = TouchingGround ? 0.2f : 0.02f;
        float jumpForce = 9f;

        if (Player.Control.Left)
            targetVelocity.x -= topSpeed;
        if (Player.Control.Right)
            targetVelocity.x += topSpeed;

        if (TouchingGround)
            velo.x = Mathf.Lerp(velo.x, targetVelocity.x, inertia);
        else
        {
            velo += targetVelocity * inertia;
            velo.x *= 1 - inertia;
        }
        if (Player.Control.Up && TouchingGround)
        {
            velo.y *= 0.1f;
            velo.y += jumpForce;
            TouchingGround = false;
        }
        Animate();
        TouchingGround = false;
        RB.velocity = velo;
    }
    private void FixedUpdate()
    {
        Instance = this;
        if(IsActive)
        {
            MovementUpdate();
            Vector2 lerp = Vector2.Lerp(Camera.main.transform.position, transform.position, 0.1f);
            Camera.main.transform.position = new Vector3(lerp.x, lerp.y, -10);
            c2D.enabled = true;
            RB.gravityScale = 1;
            transform.localScale = Vector3.one;
            Body.transform.localEulerAngles = Vector3.forward * 0;
            for (int i = 0; i < legs.Length; ++i)
            {
                GameObject leg = legs[i];
                GameObject joint = joints[i];
                leg.SetActive(true);
                joint.SetActive(true);
            }
        }
        else
        {
            for(int i = 0; i < legs.Length; ++i)
            {
                GameObject leg = legs[i];
                GameObject joint = joints[i];
                leg.transform.localScale = joint.transform.localScale = Vector3.zero;
            }
            c2D.enabled = false;
            RB.gravityScale = 0;
            RB.velocity *= 0f;
        }
        if(Utils.RandFloat(1) < 0.05f)
            ParticleManager.NewParticle(Body.transform.position + Vector3.up * 0.4f, Utils.RandFloat(0.75f, 1.25f), CheeseSpider.Instance.RB.velocity * Utils.RandFloat(0.4f, 0.8f), 1.8f, Utils.RandFloat(0.7f, 1f) , 1);
    }
    public void Animate()
    {
        float veloDiff = RB.velocity.y - oldVelo.y;
        oldVelo = RB.velocity;
        float squashImpact = 1 - veloDiff;
        float yScale = Mathf.Lerp(Visual.transform.localScale.y, Mathf.Clamp(squashImpact, RB.velocity.y <= 0.5f && TouchingGround ? 0.3f : 1.1f, 1), squashImpact < 0.9f ? 0.3f : 0.05f);
        Visual.transform.localScale = new Vector3(1, yScale, 1);
        Body.GetComponent<SpriteRenderer>().flipX = Dir == -1;
        float speed = Mathf.Abs(RB.velocity.x) * 1.25f;
        float walkSpeedMultiplier = Mathf.Clamp(Mathf.Abs(RB.velocity.x / 2f), 0, 1f);
        bool airborne = false;
        if (!TouchingGround)
        {
            walkSpeedMultiplier *= 0.2f;
            airborne = true;
        }
        AnimCounter += Mathf.Deg2Rad * speed * -1 * walkSpeedMultiplier * Dir;
        AnimCounter = AnimCounter.WrapAngle() * Mathf.Rad2Deg;
        AnimCounter = Mathf.LerpAngle(AnimCounter, 0, (1 - walkSpeedMultiplier) * (airborne ? 0.03f : 0.05f)) * Mathf.Deg2Rad;
        for (int i = 0; i < legs.Length; ++i)
        {
            GameObject g = legs[i];
            float r = AnimCounter;
            if (i == 0 || i == 3)
            {
                r += Mathf.PI;
            }
            int side = i == 1 || i == 3 ? 1 : -1;
            Vector2 circular = new Vector2(0.3f, 0).RotatedBy(r);
            circular.x += side * 0.3f;
            circular.y *= 1f;
            if (circular.y < 0)
                circular.y *= 0.1f;
            g.transform.localPosition = circular;
            float angle = Mathf.Sin(r * Dir) * 20 + (airborne ? side * 45 : 0);
            g.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(g.transform.localEulerAngles.z, angle, 0.1f);
            g.transform.localScale = Vector3.one * Mathf.Lerp(g.transform.localScale.x, 1, 0.1f);
        }
        for (int i = 0; i < joints.Length; ++i)
        {
            GameObject g = joints[i];
            int j = i;
            Vector2 toLeg = legs[j].transform.position - g.transform.position + new Vector3(0, -0.125f);
            g.transform.eulerAngles = Vector3.forward * (Mathf.Rad2Deg * toLeg.ToRotation() + 90);
            g.transform.localScale = new Vector3(Mathf.Lerp(g.transform.localScale.x, 1, 0.1f), (toLeg.magnitude * 2 - 0.125f), 1);
        }
        transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(transform.localEulerAngles.z, 0, 0.1f);

        Body.transform.localPosition = new Vector3(0, -0.25f + Mathf.Sin(AnimCounter * 2 + Mathf.PI) * 1f / 32f, 0);
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("World") || collision.CompareTag("Standable"))
        {
            TouchingGround = true;
        }
    }
    public void Kill()
    {
        Destroy(gameObject);
        if(Active)
        {
            for (int i = 0; i < 20; ++i)
            {
                ParticleManager.NewParticle(transform.position, Utils.RandFloat(1f, 2f), RB.velocity * Utils.RandFloat(0.25f, 0.75f), 2.5f, Utils.RandFloat(2f, 3f), 1);
            }
            for (int i = 0; i < 3; ++i)
            {
                GameObject g = Projectile.NewProjectile<Cheese>(transform.position, Utils.RandCircle(2) + Vector2.up * 2);
                g.GetComponent<ProjComponents>().rb.gravityScale = Utils.RandFloat(0.3f, 0.5f);
                g.GetComponent<Projectile>().Hostile = false;
                g.transform.localScale *= Utils.RandFloat(0.6f, 0.95f);
            }
        }
    }
    public void OnDestroy()
    {
        CheeseSpider.Instance = null;
    }
}

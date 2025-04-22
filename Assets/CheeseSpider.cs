using UnityEngine;

public class CheeseSpider : MonoBehaviour
{
    public static CheeseSpider Instance;
    public static bool Active => Instance != null;
    public GameObject Visual;
    public GameObject Body;
    public GameObject[] legs;
    public GameObject[] joints;
    public Vector3 oldVelo;
    public float AnimCounter = 0;
    public Rigidbody2D RB;
    private float Dir = 1;
    public bool TouchingGround = true;
    public void Start()
    {
        Instance = this;
    }
    public void MovementUpdate()
    {
        Instance = this;
        if (Mathf.Abs(RB.velocity.x) > 0.1f)
            Dir = Mathf.Sign(RB.velocity.x);
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 5 : 6;
        float inertia = TouchingGround ? 0.2f : 0.1f;
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
        MovementUpdate();
        Vector2 lerp = Vector2.Lerp(Camera.main.transform.position, transform.position, 0.1f);
        Camera.main.transform.position = new Vector3(lerp.x, lerp.y, -10);
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
            float angle = Mathf.Sin(r) * 15 + (airborne ? side * 45 : 0);
            g.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(g.transform.localEulerAngles.z, angle, 0.1f);
        }
        for (int i = 0; i < joints.Length; ++i)
        {
            GameObject g = joints[i];
            int j = i;
            Vector2 toLeg = legs[j].transform.position - g.transform.position + new Vector3(0, -0.125f);
            g.transform.eulerAngles = Vector3.forward * (Mathf.Rad2Deg * toLeg.ToRotation() + 90);
            g.transform.localScale = new Vector3(1, (toLeg.magnitude * 2 - 0.125f), 1);
        }


        Body.transform.localPosition = new Vector3(0, -0.25f + Mathf.Sin(AnimCounter * 2 + Mathf.PI) * 1f / 32f, 0);
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("World") || collision.CompareTag("Standable"))
        {
            TouchingGround = true;
        }
    }
}

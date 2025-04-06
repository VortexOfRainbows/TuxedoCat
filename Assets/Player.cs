using System;
using UnityEngine;
using UnityEngine.Android;

public class Control
{
    public bool Ability => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.Space);
    public bool MouseLeft, MouseRight;
    public bool Up, Down, Left, Right;
    public void ControlUpdate()
    {
        UpdateKey(ref Up, Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space), Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        UpdateKey(ref Down, Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow), 
            Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
        UpdateKey(ref Left, Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow), 
            Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
        UpdateKey(ref Right, Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow), 
            Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow));
        UpdateKey(ref MouseLeft, Input.GetMouseButton(0));
        UpdateKey(ref MouseRight, Input.GetMouseButton(1));
    }
    public void UpdateKey(ref bool KeyName, bool Key = false, bool AntiKey = false)
    {
        KeyName = Key && !AntiKey;
    }
    public void CopyFrom(Control other)
    {
        Up = other.Up;
        Down = other.Down;
        Left = other.Left;
        Right = other.Right;
        MouseLeft = other.MouseLeft;
        MouseRight = other.MouseRight;
    }
}
public class Player : MonoBehaviour
{
    public static Player Instance;
    public static Control Control { get; private set; } = new Control();
    public static Control PrevControl { get; private set; } = new Control();
    public static Vector3 Position => Instance == null ? (Instance = FindFirstObjectByType<Player>()).transform.position : Instance.transform.position;
    public GameObject GrappleHookPrefab;
    private GameObject Grapple;
    public Rigidbody2D RB;
    public bool TouchingGround = false;
    public int TimeSpentNotColliding = 0;
    public int UseAnimation = 0;
    public float AnimSpeed => 40f;
    public bool isUsingItem => Control.MouseLeft || (UseAnimation > 0 && UseAnimation <= AnimSpeed * 2);
    public Vector2 armTargetPos = Vector2.zero;
    public void Start()
    {
        Instance = this;
    }
    public void Update()
    {
        Instance = this;
    }
    public void MovementUpdate()
    {
        if (Mathf.Abs(RB.velocity.x) > 0.1f)
            Dir = Mathf.Sign(RB.velocity.x);
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 5 : 8;
        float inertia = TouchingGround ? 0.05f : 0.01f;
        float jumpForce = 8.1f;

        if (Control.Left)
            targetVelocity.x -= topSpeed;
        if (Control.Right)
            targetVelocity.x += topSpeed;

        if (TouchingGround)
            velo.x = Mathf.Lerp(velo.x, targetVelocity.x, inertia);
        else
        {
            velo += targetVelocity * inertia;
            velo.x *= 1 - inertia;
        }


        if (Control.Up && TouchingGround)
        {
            velo.y += jumpForce;
            TouchingGround = true;
        }

        if (isUsingItem)
        {
            Vector2 toMouse = Utils.MouseWorld - (Vector2)transform.position;
            UseAnimation++;
            float percent = UseAnimation / AnimSpeed;
            if (percent < 1)
                Dir = Mathf.Sign(toMouse.x);
            if (percent < 0.1f)
            {
                armTargetPos = (Vector2)transform.position + new Vector2(Dir, 1);
            }
            else if (percent < 0.2f)
            {
                armTargetPos = (Vector2)transform.position + new Vector2(Dir, 3);
            }
            else if (percent < 0.6f)
            {
                armTargetPos = (Vector2)transform.position + new Vector2(0, 2);
            }
            else if (percent < 0.7f)
            {
                armTargetPos = (Vector2)transform.position + new Vector2(Dir, 0);
            }
            else
            {
                if (Grapple == null)
                {
                    Grapple = Instantiate(GrappleHookPrefab, transform.position, Quaternion.identity);
                    Grapple.GetComponent<Rigidbody2D>().velocity = toMouse.normalized * 24 + RB.velocity * 0.2f;
                }
                armTargetPos = (Vector2)transform.position + new Vector2(-Dir * 0.5f, -2);
            }
        }
        else
        {
            armTargetPos = Vector2.zero;
            UseAnimation = 0;
        }
        if(Grapple != null)
        {
            ItemSprite.sprite = Resources.Load<Sprite>("JustYarn");
        }
        else
        {
            ItemSprite.sprite = Resources.Load<Sprite>("YarnWithHook");
        }
        RB.velocity = velo;
        PlayerAnimation();
    }
    public void FixedUpdate()
    {
        PrevControl.CopyFrom(Control);
        Control.ControlUpdate();
        MovementUpdate();

        TouchingGround = false;
        TimeSpentNotColliding++;
        Vector2 lerp = Vector2.Lerp(Camera.main.transform.position, Player.Position, 0.1f);
        Camera.main.transform.position = new Vector3(lerp.x, lerp.y, -10);
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("World"))
        {
            TouchingGround = true;
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        Collide2D(collision);
    }
    public void OnCollisionStay2D(Collision2D collision)
    {
        Collide2D(collision);
    }
    public void Collide2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("World"))
        {
            TimeSpentNotColliding = 0;
        }
    }

    public static readonly Vector2 ArmLeftPos = new Vector3(0.3125f, -0.125f, 0);
    public static readonly Vector2 ArmRightPos = new Vector3(-0.3125f, -0.125f, 0); 
    public static readonly Vector2 LegLeftPos = new Vector3(-0.15625f, -0.65625f, 0);
    public static readonly Vector2 LegRightPos = new Vector3(0.15625f, -0.65625f, 0);
    private float Dir = 1;
    private float prevDir = 1;
    public GameObject Visual;
    public GameObject Head;
    public GameObject Eyes;
    public GameObject Body;
    public GameObject LegLeft;
    public GameObject LegRight;
    public GameObject ArmLeft;
    public GameObject ArmRight;
    public SpriteRenderer ItemSprite;
    public Vector2 oldItemPos;
    public float walkCounter = 0;
    public Vector2 oldVelo = Vector2.zero;
    public void PlayerAnimation()
    {
        oldItemPos = ItemSprite.transform.position;
        float veloDiff = RB.velocity.y - oldVelo.y;
        oldVelo = RB.velocity;
        float squashImpact = 1 - veloDiff;
        float yScale = Mathf.Lerp(Visual.transform.localScale.y, Mathf.Clamp(squashImpact, RB.velocity.y <= 0.5f && TouchingGround ? 0.6f : 1, 1), squashImpact < 0.9f ? 0.3f : 0.05f);
        Visual.transform.localScale = new Vector3(Dir * (2 - yScale), yScale, 1);
        Visual.transform.localPosition = new Vector3(0, Visual.transform.localScale.y - 1, 0);
        float targetR = (Grapple != null && Grapple.GetComponent<GrapplingHook>().Attached) ? RB.velocity.x * 2.25f : RB.velocity.x * -0.75f * Mathf.Sqrt(1 + 0.5f * MathF.Abs(RB.velocity.y)) * Mathf.Sign(RB.velocity.y + 0.1f);
        Visual.transform.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(Visual.transform.localEulerAngles.z, targetR, 0.04f));
        float walkMotion = 4f / 32f;
        float walkDirection = 1f;
        //if (Entity.Velocity.y < -0.0 && MathF.Abs(Entity.Velocity.y) > 0.001f && MathF.Abs(Entity.Velocity.x) < 0.001f)
        //    walkDirection = -1;
        float velocity = Mathf.Abs(RB.velocity.x);
        float verticalVelo = Mathf.Abs(RB.velocity.y);
        float walkSpeedMultiplier = Mathf.Clamp(Math.Abs(velocity / 2f), 0, 1f);
        float jumpSpeedMultiplier = Mathf.Clamp(Math.Abs(verticalVelo / 3f), 0, 1f);
        walkCounter += walkDirection * velocity * Mathf.Deg2Rad * Mathf.Clamp(walkSpeedMultiplier * 9, 0, 1);
        walkCounter = walkCounter.WrapAngle();

        Vector2 circularMotion = new Vector2(walkMotion, 0).RotatedBy(-walkCounter) * walkSpeedMultiplier;
        circularMotion.x *= 0.5f;
        Vector2 inverse = -circularMotion;
        if (circularMotion.y < 0)
            circularMotion.y *= 0.1f;
        if (inverse.y < 0)
            inverse.y *= 0.1f;

        if(TouchingGround)
        {
            LegLeft.transform.localPosition = LegLeftPos + inverse + new Vector2(2 / 32f, 0) * walkSpeedMultiplier;
            LegRight.transform.localPosition = LegRightPos + circularMotion + new Vector2(-2 / 32f, 0) * walkSpeedMultiplier;
            Head.transform.localPosition = new Vector3(0, Mathf.Sin(walkCounter * 2 + Mathf.PI) * 1f / 32f, 0);
            Body.transform.localPosition = new Vector3(0, Mathf.Sin(walkCounter * 2 + Mathf.PI / 2f) * 1f / 32f, 0);
            float leftAngle = Mathf.Sin(walkCounter) * -15 * walkSpeedMultiplier;
            float rightAngle = -leftAngle;
            if (Grapple == null && armTargetPos == Vector2.zero)
                ArmLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z, leftAngle, 0.07f);
            ArmRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmRight.transform.localEulerAngles.z, rightAngle, 0.07f);
            
            leftAngle = Mathf.Cos(walkCounter) * -12 * walkSpeedMultiplier;
            rightAngle = -leftAngle;
            LegLeft.transform.localEulerAngles = new Vector3(0, 0, leftAngle);
            LegRight.transform.localEulerAngles = new Vector3(0, 0, rightAngle);
        }
        else
        {
            LegLeft.transform.localPosition = LegLeft.transform.localPosition.Lerp(LegLeftPos + new Vector2(1, 0) / 32f * jumpSpeedMultiplier, 0.05f);
            LegRight.transform.localPosition = LegRight.transform.localPosition.Lerp(LegRightPos + new Vector2(-1, 0) / 32f * jumpSpeedMultiplier, 0.05f);
            LegLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(LegLeft.transform.localEulerAngles.z, 12f * jumpSpeedMultiplier, 0.05f);
            LegRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(LegRight.transform.localEulerAngles.z, -12f * jumpSpeedMultiplier, 0.05f);
            if (Grapple == null && armTargetPos == Vector2.zero)
                ArmLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z, -30 * jumpSpeedMultiplier, 0.05f);
            ArmRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmRight.transform.localEulerAngles.z, 30 * jumpSpeedMultiplier, 0.05f);
        }
        if(Grapple != null && UseAnimation > AnimSpeed)
        {
            GrapplingHook hook = Grapple.GetComponent<GrapplingHook>();
            if(hook.points != null && hook.points.Count > 0)
                {
                armTargetPos = hook.points[0].transform.position * 0.95f + hook.transform.position * 0.05f;
            }
        }
        if(armTargetPos != Vector2.zero)
        {
            Vector2 toHook = armTargetPos - (Vector2)ArmLeft.transform.position;
            toHook.x *= Dir;
            ArmLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z, toHook.ToRotation() * Mathf.Rad2Deg + 90f, prevDir != Dir ? 1 : 0.1f);
        }
        Vector2 toMouse = Utils.MouseWorld - (Vector2)transform.position;
        Eyes.transform.localPosition = new Vector3(toMouse.normalized.x / 32f * Dir, 0, 0);
        toMouse.x = Mathf.Abs(toMouse.x);
        Head.transform.localEulerAngles = Vector3.forward * (toMouse.ToRotation() * Mathf.Rad2Deg * 0.125f);
        prevDir = Dir;
    }
}

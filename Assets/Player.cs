using System;
using UnityEngine;
using UnityEngine.Android;

public class Control
{
    public bool SwapItem;
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
        UpdateKey(ref SwapItem, Input.GetKey(KeyCode.Tab));
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
        SwapItem = other.SwapItem;
    }
}
public class Player : MonoBehaviour
{
    public CharacterAnimator anim;
    public bool TouchingGround {
        get => anim.TouchingGround;
        set => anim.TouchingGround = value;
    }
    private float Dir
    {
        get => anim.Dir;
        set => anim.Dir = value;
    }
    private float prevDir
    {
        get => anim.prevDir;
        set => anim.prevDir = value;
    }
    public Vector2 armTargetPos
    {
        get => anim.armTargetPos;
        set => anim.armTargetPos = value;
    }
    public GameObject Grapple
    {
        get => anim.Grapple;
        set => anim.Grapple = value;
    }
    public int UseAnimation
    {
        get => anim.UseAnimation;
        set => anim.UseAnimation = value;
    }
    public bool StartUsingItem => Control.MouseLeft ;
    public bool IsUsingItem => UseAnimation > 0;
    public static Player Instance;
    public static Control Control { get; private set; } = new Control();
    public static Control PrevControl { get; private set; } = new Control();
    public static Vector3 Position => Instance == null ? (Instance = FindFirstObjectByType<Player>()).transform.position : Instance.transform.position;
    public GameObject GrappleHookPrefab;
    public Rigidbody2D RB;
    public int TimeSpentNotColliding = 0;
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
        float topSpeed = TouchingGround ? 5 : 10;
        float inertia = TouchingGround ? 0.05f : 0.0225f;
        float jumpForce = 11f;

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
            velo.y *= 0.1f;
            velo.y += jumpForce;
            TouchingGround = true;
        }

        RB.velocity = velo;
        anim.Animate();
    }
    public void FixedUpdate()
    {
        PrevControl.CopyFrom(Control);
        Control.ControlUpdate();
        MovementUpdate();
        ItemUpdate();
        anim.prevTouchingGround = TouchingGround;
        TouchingGround = false;
        TimeSpentNotColliding++;
        Vector2 lerp = Vector2.Lerp(Camera.main.transform.position, Player.Position, 0.1f);
        Camera.main.transform.position = new Vector3(lerp.x, lerp.y, -10);
    }
    public int ItemType = 0;
    public void ItemUpdate()
    {
        if(Control.SwapItem && !PrevControl.SwapItem && !IsUsingItem)
        {
            ItemType++;
            ItemType %= 2;
        }
        if (ItemType == 0)
        {
            if(StartUsingItem && UseAnimation <= 0)
                UseAnimation++;
            else if(IsUsingItem)
                UseAnimation++;
            if (IsUsingItem && StartUsingItem)
            {
                Vector2 toMouse = Utils.MouseWorld - (Vector2)transform.position;
                float percent = UseAnimation / anim.AnimSpeed;
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
                        Grapple.GetComponent<Rigidbody2D>().velocity = toMouse.normalized * 32 + RB.velocity * 0.2f;
                        GrapplingHook hook = Grapple.GetComponent<GrapplingHook>();
                        hook.Owner = gameObject;
                        hook.OwnerBody = RB;
                        hook.Player = this;
                    }
                    if (UseAnimation <= anim.AnimSpeed * 2)
                        armTargetPos = (Vector2)transform.position + new Vector2(-Dir * 0.5f, -2);
                    else
                        armTargetPos = Vector2.zero;
                }
            }
            else
            {
                armTargetPos = Vector2.zero;
                UseAnimation = 0;
            }
            if (Grapple != null)
            {
                anim.ItemSprite.sprite = Resources.Load<Sprite>("Items/JustYarn");
            }
            else
            {
                anim.ItemSprite.sprite = Resources.Load<Sprite>("Items/YarnWithHook");
            }
        }
        if(ItemType == 1)
        {
            anim.ItemSprite.sprite = null;
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
        if (collision.gameObject.CompareTag("World") || collision.gameObject.CompareTag("Standable"))
        {
            TimeSpentNotColliding = 0;
        }
    }
}

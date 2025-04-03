using System;
using Unity.VisualScripting;
using UnityEngine;

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
    public static Vector3 Position => Instance.transform.position;
    public GameObject GrappleHookPrefab;
    private GameObject Grapple;
    public Rigidbody2D RB;
    public bool TouchingGround = false;
    public int TimeSpentNotColliding = 0;
    public void Start()
    {
        Instance = this;
    }
    public void Update()
    {
        Instance = this;
    }
    public void FixedUpdate()
    {
        PrevControl.CopyFrom(Control);
        Control.ControlUpdate();

        Vector2 velo = RB.velocity;

        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 5 : 6;
        float inertia = TouchingGround ? 0.05f : 0.01f;

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
            velo.y += 5;
            TouchingGround = true;
        }

        if (Control.MouseLeft)
        {
            Vector2 toMouse = Utils.MouseWorld -(Vector2)transform.position;
            if (Grapple == null)
            {
                Grapple = Instantiate(GrappleHookPrefab, transform.position, Quaternion.identity);
                Grapple.GetComponent<Rigidbody2D>().velocity = toMouse.normalized * 16;
            }
        }
        else
        {
            if (Grapple != null)
            {
                Destroy(Grapple);
            }

        }

        RB.velocity = velo;
        TouchingGround = false;
        TimeSpentNotColliding++;
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
}

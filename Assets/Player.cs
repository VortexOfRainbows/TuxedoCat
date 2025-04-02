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
    public static Control Control { get; private set; } = new Control();
    public static Control PrevControl { get; private set; } = new Control();
    public Rigidbody2D RB;
    public void Start()
    {
        
    }
    public void Update()
    {
        Control.ControlUpdate();
    }
    public void FixedUpdate()
    {
        Vector2 velo = RB.velocity;

        Vector2 moveDir = Vector2.zero;
        if (Control.Left)
            moveDir.x -= 2;
        if (Control.Right)
            moveDir.x += 2;
        Debug.Log(moveDir);

        velo.x = moveDir.x;

        if(Control.Up)
        {

        }

        RB.velocity = velo;

        PrevControl.CopyFrom(Control);
    }
}

using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public float hurtTimer = 0;
    public float DelayCameraMovement = 0f;
    private Color color = Color.white;
    public void Hurt(int damage = 1)
    {
        hurtTimer = 10;
        regen = 0;
        life -= damage;
    }
    public int life = 9;
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
    public Tilemap world;
    public bool StartUsingItem => Control.MouseLeft ;
    public bool IsUsingItem => UseAnimation > 0;
    public static Player Instance;
    public static Control Control { get; private set; } = new Control();
    public static Control PrevControl { get; private set; } = new Control();
    public static Vector3 Position => Instance == null ? (Instance = FindFirstObjectByType<Player>()).transform.position : Instance.transform.position;
    public GameObject GrappleHookPrefab;
    public GameObject LaserPtr;
    public Rigidbody2D RB;
    public int TimeSpentNotColliding = 0;
    public GameObject CheeseSpiderPrefab;
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

        if(!CheeseSpider.Active)
        {
            if (Control.Left)
                targetVelocity.x -= topSpeed;
            if (Control.Right)
                targetVelocity.x += topSpeed;
        }
        else
        {
            inertia *= 2.5f;
        }

        if (TouchingGround)
            velo.x = Mathf.Lerp(velo.x, targetVelocity.x, inertia);
        else
        {
            velo += targetVelocity * inertia;
            velo.x *= 1 - inertia;
        }


        if (!CheeseSpider.Active)
        {
            if (Control.Up && TouchingGround)
            {
                velo.y *= 0.1f;
                velo.y += jumpForce;
                TouchingGround = false;
            }
        }

        RB.velocity = velo;
    }
    public void FixedUpdate()
    {
        PrevControl.CopyFrom(Control);
        Control.ControlUpdate();
        MovementUpdate();
        ItemUpdate();
        if(life < 9)
        {
            regen += Time.fixedDeltaTime;
            if(regen > 5)
            {
                life++;
                regen = 0;
            }
        }
        if(life < 1)
        {
            life = 1; //For now, stay at one life since we dont have death support
        }
        anim.Animate();
        anim.prevTouchingGround = TouchingGround;
        TouchingGround = false;
        TimeSpentNotColliding++;
        if(!CheeseSpider.Active)
        {
            if (DelayCameraMovement <= 0)
            {
                Vector2 lerp = Vector2.Lerp(Camera.main.transform.position, Player.Position, 0.1f);
                Camera.main.transform.position = new Vector3(lerp.x, lerp.y, -10);
            }
            else
            {
                DelayCameraMovement -= Time.fixedDeltaTime;
            }
        }
        else
        {
            DelayCameraMovement = 0.5f;
        }

        if (--hurtTimer >= 0)
            color = Color.Lerp(color, Color.red, 0.12f);
        else
            color = Color.Lerp(color, Color.white, 0.15f);
        foreach (SpriteRenderer sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr != anim.ItemSprite && sr.gameObject != LaserPtr)
                sr.color = color;
        }
    }
    public int ItemType = 0;
    public float regen = 0;
    public void ItemUpdate()
    {
        if (Control.SwapItem && !PrevControl.SwapItem && !IsUsingItem)
        {
            ItemType++;
            ItemType %= 3;
        }
        if (ItemType == 0)
        {
            if (StartUsingItem && UseAnimation <= 0)
                UseAnimation++;
            else if (IsUsingItem && UseAnimation < anim.AnimSpeed * 2)
                UseAnimation++;
            if (IsUsingItem && (StartUsingItem || UseAnimation < anim.AnimSpeed * 2))
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
            anim.ItemSprite.transform.localScale = Vector3.one * 0.7f;
            anim.ItemSprite.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        if (ItemType == 1)
        {
            anim.ItemSprite.sprite = Resources.Load<Sprite>("Items/LaserPointerGun");
            anim.ItemSprite.transform.localScale = Vector3.one;
            anim.ItemSprite.transform.localEulerAngles = new Vector3(0, 0, -90);
            anim.armTargetPos = Utils.MouseWorld;
            Vector2 toMouse = Utils.MouseWorld - (Vector2)transform.position;
            Dir = Mathf.Sign(toMouse.x);
            LaserPtr.SetActive(true);
            Vector2 dir = Vector2.down.RotatedBy(anim.ArmLeft.transform.eulerAngles.z * Mathf.Deg2Rad);
            RaycastHit2D ray = Physics2D.Raycast((Vector2)LaserPtr.transform.position, dir, 20, LayerMask.GetMask("World", "Goon"));
            RaycastHit2D ray2 = Physics2D.Raycast((Vector2)LaserPtr.transform.position - dir * 0.5f, dir, 20, LayerMask.GetMask("World", "Goon"));
            float newDist = 20;
            if (ray.distance > 0)
            {
                newDist = ray.distance;
            }
            else if (ray2.distance > 0)
            {
                newDist = ray2.distance - 0.5f;
            }
            LaserPtr.transform.localScale = new Vector3(newDist, LaserPtr.transform.localScale.y);
            //Gizmos.DrawLine((Vector2)LaserPtr.transform.position, (Vector2)LaserPtr.transform.position + toMouse.normalized * newDist);
            if (StartUsingItem && UseAnimation <= 0)
            {
                if(world.HasTile(world.WorldToCell(LaserPtr.transform.position)))
                {

                }
                else
                {
                    Vector2 end = ray.distance > 0 ? ray.point : ray2.distance > 0 ? ray2.point : (Vector2)LaserPtr.transform.position + dir * newDist;
                    UseAnimation = 25;
                    var p = Projectile.NewProjectile<Laser>(LaserPtr.transform.position, dir, end.x, end.y);
                    p.transform.localScale = new Vector3(newDist, LaserPtr.transform.localScale.y);
                    p.transform.localEulerAngles = new Vector3(0, 0, anim.ArmLeft.transform.eulerAngles.z - 90);
                    p.GetComponent<ProjComponents>().spriteRenderer.color = LaserPtr.GetComponent<SpriteRenderer>().color * 3;
                    Debug.Log(newDist);
                    p.GetComponent<ProjComponents>().c2D.radius = Mathf.Abs(0.35f / newDist);
                    anim.useRecoil += 10 + Utils.RandFloat(20);
                    if (anim.useRecoil > 10)
                        anim.useRecoil *= 1.1f;
                    if (anim.useRecoil > 20)
                        anim.useRecoil *= 1.1f;
                    if (anim.useRecoil > 120)
                        anim.useRecoil = 120;
                }
                //LaserPtr.transform.localScale = new Vector3(LaserPtr.transform.localScale.x, LaserPtr.transform.localScale.y + 0.5f);
            }
            else
            {
                UseAnimation--;
                if(!StartUsingItem)
                {
                    UseAnimation = 0;
                }
                //LaserPtr.transform.localScale = new Vector3(LaserPtr.transform.localScale.x, Mathf.Lerp(LaserPtr.transform.lossyScale.y, 0.03125f, 0.1f));
            }
        }
        else
        {
            LaserPtr.SetActive(false);
        }
        if (ItemType == 2)
        {
            if (CheeseSpider.Instance == null)
            {
                CheeseSpider.Instance = Instantiate(CheeseSpiderPrefab, transform.position, Quaternion.identity).GetComponent<CheeseSpider>();
                CheeseSpider.Instance.transform.localScale = Vector3.zero;
                UseAnimation = 30;
            }
            else 
            {
                //CheeseSpider.Instance.transform.position = anim.ItemSprite.transform.position;
                if (!CheeseSpider.Active)
                {
                    anim.ItemSprite.sprite = null;
                    anim.ItemSprite.transform.localScale = Vector3.one;
                    anim.ItemSprite.transform.localEulerAngles = new Vector3(0, 0, -90);
                    Vector2 toMouse = Utils.MouseWorld - (Vector2)transform.position;
                    Dir = Mathf.Sign(toMouse.x);
                    toMouse.y *= 0.4f;
                    toMouse.y -= 2;
                    anim.armTargetPos = (Vector2)transform.position + toMouse;
                    if (Control.MouseLeft && UseAnimation <= 0)
                    {
                        //cast cheese spider
                        CheeseSpider.Instance.transform.SetParent(null);
                        CheeseSpider.Instance.IsActive = true;
                        CheeseSpider.Instance.RB.velocity = toMouse.normalized * 10f + Vector2.up * 10;
                        for(int i = 0; i < 15; ++i)
                            ParticleManager.NewParticle(CheeseSpider.Instance.transform.position, Utils.RandFloat(0.75f, 1.25f), CheeseSpider.Instance.RB.velocity * Utils.RandFloat(0.4f, 0.8f), 1.8f, Utils.RandFloat(0.7f, 1f) , 1);
                        UseAnimation = 0;
                    }
                    else
                    {
                        float percent = Mathf.Min(1, 1 - UseAnimation / 30f);
                        CheeseSpider.Instance.transform.SetParent(anim.ItemSprite.transform);
                        CheeseSpider.Instance.transform.localPosition = new Vector3(-0.2f, 0.24f, 0);
                        CheeseSpider.Instance.transform.localEulerAngles = new Vector3(0, 0, 60);
                        float sin = Mathf.Sin(percent * percent * Mathf.PI) * 0.4f;
                        CheeseSpider.Instance.transform.localScale = new Vector3(1, 1, 1) * (percent + sin);
                        UseAnimation--;
                    }
                }
                else
                {
                    anim.armTargetPos = Vector2.zero;
                    if (CheeseSpider.Instance.AliveTimer < 0.7f)
                    {
                        UseAnimation = 1;
                    }
                    else
                        UseAnimation = 0;
                }
                if (Control.MouseRight && CheeseSpider.Active && CheeseSpider.Instance.AliveTimer > 0.7f)
                {
                    if (CheeseSpider.Instance != null)
                        CheeseSpider.Instance.Kill();
                }
            }
        }
        else
        {
            if (CheeseSpider.Instance != null)
                CheeseSpider.Instance.Kill();
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

using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Tilemaps;
public struct Control
{
    public bool SwapItem;
    public bool MouseLeft, MouseRight;
    public bool Jump;
    public bool Up, Down, Left, Right;
    public bool Num1, Num2, Num3, Num4;
    public bool E;
    public float MouseWheel;
    public Control(bool defaultState = false)
    {
        SwapItem = MouseLeft = MouseRight = Up = Down = Left = Right = defaultState;
        Num1 = Num2 = Num3 = Num4 = Jump = false;
        E = false;
        MouseWheel = 0;
    }
    public void Update()
    {
        E = UpdateKey(Input.GetKey(KeyCode.E));
        Up = UpdateKey(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space), Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        Down = UpdateKey(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow), 
          Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space));
        Left = UpdateKey(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow), 
          Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
        Right = UpdateKey(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow), 
          Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow));
        MouseLeft = UpdateKey(Input.GetMouseButton(0));
        MouseRight = UpdateKey(Input.GetMouseButton(1));
        SwapItem = UpdateKey(Input.GetKey(KeyCode.Tab));
        Jump = UpdateKey(Input.GetKey(KeyCode.Space));

        Num1 = UpdateKey(Input.GetKey(KeyCode.Alpha1));
        Num2 = UpdateKey(Input.GetKey(KeyCode.Alpha2));
        Num3 = UpdateKey(Input.GetKey(KeyCode.Alpha3));
        Num4 = UpdateKey(Input.GetKey(KeyCode.Alpha4));
        MouseWheel = Input.mouseScrollDelta.y;
    }
    public static bool UpdateKey(bool Key = false, bool AntiKey = false)
    {
        return Key && !AntiKey;
    }
}
public class Player : MonoBehaviour
{
    public static bool HasHook = false;
    public static bool HasGun = false;
    public static bool HasDrone = false;
    public static bool HasClaw = false;
    public SaveBox LastUsedSaveBox;
    public static bool InSaveAnimation = false;
    public float hurtTimer = 0;
    public float DelayCameraMovement = 0f;
    private Color color = Color.white;
    public float DeathAnimation = 0;
    public bool Dead = false;
    public void Hurt(int damage = 1)
    {
        if (InSaveAnimation)
            return;
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
    public static Control Control = new();
    public static Control PrevControl = new();
    public static Vector3 Position => Instance == null ? (Instance = FindFirstObjectByType<Player>()).transform.position : Instance.transform.position;
    public GameObject GrappleHookPrefab;
    public GameObject LaserPtr;
    public Rigidbody2D RB;
    public int TimeSpentNotColliding = 0;
    public GameObject CheeseSpiderPrefab;
    public bool wasClimbingUpward = false;
    public GameObject ClimbCollider;
    public int wallJumpTimer = 0;
    public int wallJumpGraceFrames = 0;
    public Collider2D SolidCollider;
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
        SolidCollider.enabled = !InSaveAnimation;
        if (!InSaveAnimation)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.14f);
            if (transform.localScale.x > 0.99f)
                transform.localScale = Vector3.one;
        }
        else
            LaserPtr.SetActive(false);
        ClimbCollider.SetActive(ItemType == 2);
        if (IsUsingItem) {
            if (anim.Climbing)
                anim.TouchingGround = false;
            anim.Climbing = false;
        }
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 5 : (wallJumpTimer > 0 ? 5 : 10);
        float inertia = TouchingGround ? 0.05f : (wallJumpTimer > 0 ? 0.0025f : 0.0225f);
        bool midWallJump = wallJumpTimer > 0;
        if (midWallJump || anim.Climbing || wallJumpGraceFrames >= 0)
        {
            if(midWallJump)
            {
                float percent = wallJumpTimer / 50f;
                percent *= percent;
                velo.x *= 0.99f;
                velo.x -= anim.ClimbDir * 0.3f * percent;
                velo.y += 0.05f * percent;
            }
            wallJumpTimer--;
        }
        else if(wallJumpTimer < 0)
        {
            wallJumpTimer++;
        }
        float jumpForce = 11f;

        if (anim.Climbing)
        {
            inertia = 0.1f;
            Dir = anim.ClimbDir;
            RB.gravityScale = 0.1f;
            velo.y *= 0.95f;
        }
        else
        {
            if (Mathf.Abs(RB.velocity.x) > 0.1f && !midWallJump)
                Dir = Mathf.Sign(RB.velocity.x);
            if(Grapple == null && !InSaveAnimation)
                RB.gravityScale = 1f;
        }
        if (!CheeseSpider.Active && !InSaveAnimation && !Dead)
        {
            if (Control.Left) {
                targetVelocity.x -= topSpeed;
                if (anim.Climbing)
                {
                    if(anim.ClimbDir == -1)
                    {
                        targetVelocity.y += topSpeed * 0.5f;
                        wasClimbingUpward = true;
                    }
                    else
                    {
                        wasClimbingUpward = false;
                        wallJumpGraceFrames = 15;
                    }
                }
            }
            if (Control.Right) {
                targetVelocity.x += topSpeed;
                if (anim.Climbing)
                {
                    if(anim.ClimbDir == 1)
                    {
                        targetVelocity.y += topSpeed * 0.5f;
                        wasClimbingUpward = true;
                    }
                    else
                    {
                        wasClimbingUpward = false;
                        wallJumpGraceFrames = 15;
                    }
                }
            }
            if(Control.Up && targetVelocity.y == 0 && anim.Climbing)
                targetVelocity.y += topSpeed * 0.5f;
            if (Control.Down && anim.Climbing)
            {
                targetVelocity.y -= topSpeed * 0.5f;
            }
        }
        else
        {
            inertia *= 2.5f;
        }

        if (TouchingGround || anim.Climbing) {
            velo.x = Mathf.Lerp(velo.x, targetVelocity.x, inertia);
            if(anim.Climbing)
            {
                velo.y = Mathf.Lerp(velo.y, targetVelocity.y, inertia);
            }
        }
        else
        {
            velo += targetVelocity * inertia;
            velo.x *= 1 - inertia;
        }


        if (!CheeseSpider.Active && !InSaveAnimation && !Dead)
        {
            bool fakeJump = false;
            float jumpMult = 1.0f;
            if(anim.prevClimbing && !anim.Climbing && wasClimbingUpward && UseAnimation <= 0)
            {
                jumpMult = 0.55f;
                fakeJump = !Control.Down;
            }
            if (anim.Climbing)
            {
                velo.x *= 0.95f;
                TouchingGround = false;
            }
            if ((anim.Climbing || wallJumpGraceFrames >= 0) && Control.Jump && wallJumpTimer <= -12)
            {
                TouchingGround = true;
                fakeJump = true;
                jumpMult = 0.875f;
            }
            if (((Control.Up || Control.Jump) && TouchingGround) || fakeJump)
            {
                velo.y *= 0.1f;
                velo.y += jumpMult * jumpForce;
                if(anim.Climbing)
                {
                    wallJumpTimer = 50;
                    velo.x -= anim.ClimbDir * 3.75f;
                    RB.position += velo * 0.01f;
                    anim.Climbing = false;
                }
                TouchingGround = false;
            }
        }
        wallJumpGraceFrames--;
        if (!anim.prevClimbing && !anim.Climbing)
        {
            wasClimbingUpward = false;
        }
        RB.velocity = velo;
    }
    public void LifeUpdate()
    {
        if (life < 9)
        {
            if (InSaveAnimation)
            {
                regen += Time.fixedDeltaTime;
                if (regen >= 0.5f)
                {
                    life++;
                    regen -= 0.5f;
                    PopupText.NewPopupText(transform.position, new Vector2(0, 2) + Utils.RandCircle(1), new Color(0f, 1f, 0f), "+");
                }
            }
            else
                regen = -1;
        }
        if (life <= 0)
        {
            Dead = true;
        }
        if(Dead)
        {
            DeathAnimation++;
            float percent = Mathf.Min(DeathAnimation / 200f, 1);
            anim.Visual.transform.localPosition = Utils.RandCircle(percent * 0.1f);
            Main.SetVignetteIntensity(percent);
            if(percent >= 1)
            {
                Dead = false;
                life = 1;
                transform.position = LastUsedSaveBox.transform.position;
                Control.E = true;
                LastUsedSaveBox.EnterSaveBox();
                LastUsedSaveBox.Anim = 5;
            }
        }
        else
        {
            if(DeathAnimation > 0)
            {
                float percent = Mathf.Max(DeathAnimation / 200f, 0);
                DeathAnimation--;
                Main.SetVignetteIntensity(percent);
            }
        }
        DeathAnimation = Mathf.Clamp(DeathAnimation, 0, 200);
    }
    public void FixedUpdate()
    {
        PrevControl = Control;
        Control.Update();
        MovementUpdate();
        ItemUpdate();
        LifeUpdate();
        if (wallJumpTimer > 0)
        {
            Dir = -anim.ClimbDir;
        }
        anim.Animate();
        anim.prevTouchingGround = TouchingGround;
        TouchingGround = anim.Climbing = false;
        TimeSpentNotColliding++;
        if(!CheeseSpider.Active)
        {
            if (DelayCameraMovement <= 0)
            {
                if(!Dialogue.InDialogue)
                {
                    Vector2 lerp = Vector2.Lerp(Camera.main.transform.position, Player.Position, 0.1f);
                    Camera.main.transform.position = new Vector3(lerp.x, lerp.y, -10);
                }
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
    public int UseCounter = 0;
    public float ItemTypeCounter = 0;
    public float ItemSwapDelay = 0;
    public void AdjustItemType(int i)
    {
        ItemType += i;
        if (ItemType < 0)
            ItemType += 4;
        ItemType %= 4;
    }
    public bool HasCorrectItem()
    {
        if (HasHook && ItemType == 0)
            return true;
        if (HasGun && ItemType == 1)
            return true;
        if (HasClaw && ItemType == 2)
            return true;
        if (HasDrone && ItemType == 3)
            return true;
        return !HasHook && !HasGun && !HasClaw && !HasDrone;
    }
    public void FindAppropriateItem(int dir)
    {
        while (!HasCorrectItem())
            AdjustItemType(dir);
    }
    public void ItemUpdate()
    {
        if ((!IsUsingItem || (ItemType == 3 && !CheeseSpider.Active && !Dead)))
        {
            int prevType = ItemType;
            if(ItemSwapDelay <= 0)
            {
                ItemTypeCounter += Control.MouseWheel;
                if (ItemTypeCounter >= 1 || ItemTypeCounter <= -1)
                {
                    int dir = -(int)Mathf.Sign(ItemTypeCounter);
                    AdjustItemType(dir);

                    if(HasHook || HasGun || HasDrone || HasClaw)
                        FindAppropriateItem(dir);

                    ItemTypeCounter = 0;
                    ItemSwapDelay = 5;
                }
            }
            else
            {
                ItemSwapDelay--;
            }
            if (Control.Num1 && HasHook)
                ItemType = 0;
            else if (Control.Num2 && HasGun)
                ItemType = 1;
            else if (Control.Num3 && HasClaw)
                ItemType = 2;
            else if (Control.Num4 && !InSaveAnimation && HasDrone)
                ItemType = 3;
            if(prevType != ItemType)
                UseAnimation = 0;
        }
        if (!IsUsingItem && (InSaveAnimation || Dead))
        {
            if (ItemType == 3)
            {
                UseAnimation = 0;
                ItemType = 2;
            }
            else
                return;
        }
        if (ItemType == 0 && HasHook)
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
        if (ItemType == 1 && HasGun)
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
        if (ItemType == 3 && HasDrone)
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
        if(ItemType == 2 && HasClaw)
        {
            anim.ItemSprite.sprite = anim.BackItemSprite.sprite = Resources.Load<Sprite>("Items/CatClaw");
            anim.ItemSprite.transform.localScale = anim.BackItemSprite.transform.localScale = Vector3.one;
            anim.ItemSprite.transform.localEulerAngles = anim.BackItemSprite.transform.localEulerAngles = new Vector3(0, 0, -90);
            anim.ItemSprite.sortingOrder = 7;
            anim.BackItemSprite.sortingOrder = -6;
            anim.BackItemSprite.flipY = true;
            anim.armTargetPos = anim.RightArmTargetPos = Vector2.zero;
            Vector2 toMouse = Utils.MouseWorld - (Vector2)transform.position;
            Dir = Mathf.Sign(toMouse.x);
            int speed = 16;
            if (UseAnimation <= 0)
            {
                if(UseAnimation >= 0)
                {
                    if(StartUsingItem)
                    {
                        UseAnimation = speed * 2;
                    }
                    else
                    { 
                        UseCounter = 0; 
                    }
                }
                else 
                {
                    UseAnimation++;
                }
                anim.ArmLerpSpeed = 0.1f;
                anim.ArmLeft.transform.localScale = anim.ArmRight.transform.localScale = Vector3.one;
            }
            else
            {
                float percent = 1 - ((UseAnimation - 1) % speed) / (float)speed;
                float sin = Mathf.Sin(percent * Mathf.PI) * 0.4f;
                toMouse *= Dir;
                float r = toMouse.ToRotation();
                if (UseAnimation > speed * 1)
                {
                    anim.ForceArmDir = 1;
                    armTargetPos = (Vector2)transform.position + new Vector2(Dir, 2f);
                    anim.RightArmTargetPos = (Vector2)transform.position + new Vector2(Dir, -2f);
                    if(UseCounter > 0)
                        anim.ArmRight.transform.localScale = anim.ArmLeft.transform.localScale = Vector3.one * (1 + sin);
                }
                else
                {
                    armTargetPos = (Vector2)transform.position + new Vector2(Dir, -2f);
                    anim.RightArmTargetPos = (Vector2)transform.position + new Vector2(Dir, 2f);
                    anim.ArmRight.transform.localScale = anim.ArmLeft.transform.localScale = Vector3.one * (1 + sin);
                    anim.ForceArmDir = 0;
                }
                if (UseAnimation == speed + 10 || UseAnimation == 10)
                {
                    float changeDir = UseCounter % 2 * 2 - 1;
                    Vector2 launchDir = new Vector2(Dir, Utils.RandFloat(-1, 1) * 0.6f) * 0.7f + RB.velocity * 1.1f;
                    toMouse *= Dir;
                    Vector2 n = toMouse.normalized * 1f;
                    launchDir += n;
                    Projectile.NewProjectile<CatClawSlash>((Vector2)transform.position + new Vector2(Dir * .8f, 0) + n * 1.2f, launchDir, changeDir * -Dir);
                    UseCounter++;
                }
                UseAnimation--;
                anim.ArmLerpSpeed = 0.4f * percent * percent;
                if(UseAnimation <= 0)
                {
                    anim.ForceArmDir = 0;
                }
            }
        }
        else
        {
            UseCounter = 0;
            anim.BackItemSprite.sprite = null;
            anim.ItemSprite.sortingOrder = 5;
            anim.BackItemSprite.sortingOrder = -4;
            anim.ArmLerpSpeed = 0.1f;
            anim.ForceArmDir = 0;
            anim.RightArmTargetPos = Vector2.zero;
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
        if ((collision.gameObject.CompareTag("World") || collision.gameObject.CompareTag("Standable")))
        {
            if(collision.gameObject.layer != 10)
                TimeSpentNotColliding = 0;
        }
    }
}

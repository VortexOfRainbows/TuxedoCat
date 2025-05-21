using System;
using UnityEngine;
public class CharacterAnimator : MonoBehaviour
{
    public void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("World") || collision.CompareTag("Standable")))
        {
            if (collision.gameObject.layer != 10 && collision.gameObject.layer != 12)
            {
                TouchingGround = true;
            }
        }
    }
    public bool IsBiggieCheese = false;
    public bool TPose = false;
    public Vector2 ArmLeftPos => IsBiggieCheese ? new Vector3(-1.1875f, 0.125f, 0) : new Vector3(-0.3125f, -0.125f, 0);
    public Vector2 ArmRightPos => IsBiggieCheese ? new Vector3(1.1875f, 0.0625f, 0) : new Vector3(0.3125f, -0.125f, 0);
    public Vector2 LegLeftPos => IsBiggieCheese ? new Vector3(-0.40625f, -1.6875f, 0) : new Vector3(-0.15625f, -0.65625f, 0);
    public Vector2 LegRightPos => IsBiggieCheese ? new Vector3(0.4675f, -1.6875f, 0) : new Vector3(0.15625f, -0.65625f, 0);
    public bool Climbing = false;
    public bool prevClimbing = false;
    public float ClimbDir = 1;
    public float Dir = 1;
    public float prevDir = 1;
    public GameObject Grapple;
    public Rigidbody2D RB;
    public GameObject Visual;
    public GameObject Head;
    public GameObject Eyes;
    public GameObject Body;
    public GameObject LegLeft;
    public GameObject LegRight;
    public GameObject ArmLeft;
    public GameObject ArmRight;
    public GameObject HandLeft;
    public GameObject HandRight;
    public SpriteRenderer ItemSprite;
    public SpriteRenderer BackItemSprite;
    public Vector2 oldItemPos;
    public float walkCounter = 0;
    public Vector2 oldVelo = Vector2.zero;
    public bool prevTouchingGround = false;
    public bool TouchingGround = false;
    public Vector2 armTargetPos = Vector2.zero;
    public Vector2 RightArmTargetPos = Vector2.zero;
    public int UseAnimation = 0;
    public float AnimSpeed => 40f;
    public bool TargetCursor =  false;
    public bool TargetFront = true;
    public Vector2 eyeTargetPosition = Vector2.zero;
    public float useRecoil = 0;
    public float ArmLerpSpeed = 0.1f;
    public int ForceArmDir = 0;
    public void Animate()
    {
        if (Climbing)
            Dir = ClimbDir;


        oldItemPos = ItemSprite.transform.position;
        float veloDiff = RB.velocity.y - oldVelo.y;
        oldVelo = RB.velocity;
        float squashImpact = 1 - (IsBiggieCheese ? veloDiff * 0.5f : veloDiff);
        float yScale = Mathf.Lerp(Visual.transform.localScale.y, Mathf.Clamp(squashImpact, RB.velocity.y <= 0.5f && TouchingGround ? 0.6f : 1, 1), squashImpact < 0.9f ? 0.3f : 0.05f);
        Visual.transform.localScale = new Vector3(Dir * (2 - yScale), yScale, 1);
        if(!Player.Instance.Dead || Player.Instance.gameObject != gameObject)
            Visual.transform.localPosition = new Vector3(0, Visual.transform.localScale.y - 1, 0);
        float targetR = (Grapple != null && Grapple.GetComponent<GrapplingHook>().Attached) ? RB.velocity.x * 2.25f : RB.velocity.x * -0.75f * Mathf.Sqrt(1 + 0.5f * MathF.Abs(RB.velocity.y)) * Mathf.Sign(RB.velocity.y + 0.1f);
        Visual.transform.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(Visual.transform.localEulerAngles.z, targetR, 0.04f));
        float walkMotion = IsBiggieCheese ? 0.25f : 0.125f;
        float walkDirection = 1f;
            //if (Entity.Velocity.y < -0.0 && MathF.Abs(Entity.Velocity.y) > 0.001f && MathF.Abs(Entity.Velocity.x) < 0.001f)
            //    walkDirection = -1;
        float velocity = Mathf.Sqrt(Mathf.Abs(RB.velocity.x)) * 3f * Mathf.Sign(RB.velocity.x * Dir);
        if (Climbing)
            velocity += Mathf.Sqrt(Mathf.Abs(RB.velocity.y)) * 3f;
        float verticalVelo = Mathf.Abs(RB.velocity.y);
        if (verticalVelo < 0.2f)
            verticalVelo = 0;
        float walkSpeedMultiplier = Mathf.Clamp(Math.Abs(velocity / 2f), 0, 1f);
        float jumpSpeedMultiplier = Mathf.Clamp(Math.Abs(verticalVelo / 3f), 0, 1f);
        walkCounter += walkDirection * velocity * Mathf.Deg2Rad * Mathf.Clamp(walkSpeedMultiplier * 9, 0, 1);
        walkCounter = walkCounter.WrapAngle();

        if (Climbing)
        {
            float sin = MathF.Sin(walkCounter);
            float mult = MathF.Min(1, verticalVelo * 3.5f);
            armTargetPos = (Vector2)transform.position + new Vector2(ClimbDir, 0.65f * sin * mult);
            RightArmTargetPos = (Vector2)transform.position + new Vector2(ClimbDir, -0.65f * sin * mult);
        }

        Vector2 circularMotion = new Vector2(walkMotion, 0).RotatedBy(-walkCounter) * walkSpeedMultiplier;
        circularMotion.x *= 0.5f;
        Vector2 inverse = -circularMotion;
        if (circularMotion.y < 0)
            circularMotion.y *= 0.1f;
        if (inverse.y < 0)
            inverse.y *= 0.1f;

        if (TouchingGround || TPose)
        {
            float biggieCheeseModifier = IsBiggieCheese ? TPose ? 0 : 55 : 0;
            LegLeft.transform.localPosition = LegLeftPos + inverse + new Vector2(2 / 32f, 0) * walkSpeedMultiplier;
            LegRight.transform.localPosition = LegRightPos + circularMotion + new Vector2(-2 / 32f, 0) * walkSpeedMultiplier;
            Head.transform.localPosition = new Vector3(0, Mathf.Sin(walkCounter * 2 + Mathf.PI) * 1f / 32f, 0);
            Body.transform.localPosition = new Vector3(0, Mathf.Sin(walkCounter * 2 + Mathf.PI / 2f) * 1f / 32f, 0);
            float angle = IsBiggieCheese ? 5f : 15f;
            float leftAngle = biggieCheeseModifier + Mathf.Sin(walkCounter) * -angle * walkSpeedMultiplier;
            float rightAngle = -leftAngle;
            if (Grapple == null && armTargetPos == Vector2.zero)
                ArmLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z, leftAngle, 0.07f);
            if (RightArmTargetPos == Vector2.zero)
                ArmRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmRight.transform.localEulerAngles.z, rightAngle, 0.07f);

            leftAngle = Mathf.Cos(walkCounter) * -12 * walkSpeedMultiplier;
            rightAngle = -leftAngle;
            LegLeft.transform.localEulerAngles = new Vector3(0, 0, leftAngle);
            LegRight.transform.localEulerAngles = new Vector3(0, 0, rightAngle);
            if(IsBiggieCheese)
            {
                if(TPose)
                {
                    HandLeft.transform.localEulerAngles = Mathf.LerpAngle(HandLeft.transform.localEulerAngles.z, 15, 0.05f) * Vector3.forward;
                    HandRight.transform.localEulerAngles = Mathf.LerpAngle(HandRight.transform.localEulerAngles.z, -15, 0.05f) * Vector3.forward;
                }
                else
                {
                    HandLeft.transform.localEulerAngles = Mathf.LerpAngle(HandLeft.transform.localEulerAngles.z, 65, 0.05f) * Vector3.forward;
                    HandRight.transform.localEulerAngles = Mathf.LerpAngle(HandRight.transform.localEulerAngles.z, -65, 0.05f) * Vector3.forward;
                }
            }
        }
        else
        {
            LegLeft.transform.localPosition = LegLeft.transform.localPosition.Lerp(LegLeftPos + new Vector2(1, 0) / 32f * jumpSpeedMultiplier, 0.05f);
            LegRight.transform.localPosition = LegRight.transform.localPosition.Lerp(LegRightPos + new Vector2(-1, 0) / 32f * jumpSpeedMultiplier, 0.05f);
            LegLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(LegLeft.transform.localEulerAngles.z, 12f * jumpSpeedMultiplier, 0.05f);
            LegRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(LegRight.transform.localEulerAngles.z, -12f * jumpSpeedMultiplier, 0.05f);
            if(IsBiggieCheese)
            {
                HandLeft.transform.localEulerAngles = Mathf.LerpAngle(HandLeft.transform.localEulerAngles.z, -55, 0.15f) * Vector3.forward;
                HandRight.transform.localEulerAngles = Mathf.LerpAngle(HandRight.transform.localEulerAngles.z, 55, 0.15f) * Vector3.forward;
                ArmLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z, -65, 0.05f);
                ArmRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmRight.transform.localEulerAngles.z, 65, 0.05f);
            }
            else
            {
                if (Grapple == null && armTargetPos == Vector2.zero)
                    ArmLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z, -30 * jumpSpeedMultiplier, 0.05f);
                if (RightArmTargetPos == Vector2.zero)
                    ArmRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmRight.transform.localEulerAngles.z, 30 * jumpSpeedMultiplier, 0.05f);
            }
        }
        if (Grapple != null && UseAnimation > AnimSpeed)
        {
            GrapplingHook hook = Grapple.GetComponent<GrapplingHook>();
            if (hook.points != null && hook.points.Count > 0)
            {
                armTargetPos = hook.points[0].transform.position * 0.95f + hook.transform.position * 0.05f;
            }
        }
        if (armTargetPos != Vector2.zero)
        {
            Vector2 toHook = armTargetPos - (Vector2)ArmLeft.transform.position;
            toHook.x *= Dir;
            ArmLeft.transform.localEulerAngles = Vector3.forward *
                (Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z,
                toHook.ToRotation() * Mathf.Rad2Deg + 90f + useRecoil,
                prevDir != Dir ? 1 : ArmLerpSpeed));
        }
        if (RightArmTargetPos != Vector2.zero)
        {
            Vector2 toHook = RightArmTargetPos - (Vector2)ArmRight.transform.position;
            toHook.x *= Dir;
            ArmRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmRight.transform.localEulerAngles.z, toHook.ToRotation() * Mathf.Rad2Deg + 90f + useRecoil, prevDir != Dir ? 1 : ArmLerpSpeed);
        }
        useRecoil *= 0.9f;
        if(TargetCursor)
            eyeTargetPosition = Utils.MouseWorld;
        else if(TargetFront)
        {
            eyeTargetPosition = transform.position + new Vector3(Dir, 0, 0);
        }
        Vector2 toMouse = eyeTargetPosition - (Vector2)transform.position;
        Eyes.transform.localPosition = new Vector3(toMouse.normalized.x / 32f * Dir, 0, 0);
        toMouse.x = Mathf.Abs(toMouse.x);
        Head.transform.localEulerAngles = Vector3.forward * (toMouse.ToRotation() * Mathf.Rad2Deg * 0.125f);
        prevDir = Dir;
        prevClimbing = Climbing;
    }
}

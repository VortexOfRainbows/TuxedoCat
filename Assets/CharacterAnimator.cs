using System;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("World") || collision.CompareTag("Standable")) && collision.gameObject.layer != 10)
        {
            TouchingGround = true;
        }
    }
    public static readonly Vector2 ArmLeftPos = new Vector3(0.3125f, -0.125f, 0);
    public static readonly Vector2 ArmRightPos = new Vector3(-0.3125f, -0.125f, 0);
    public static readonly Vector2 LegLeftPos = new Vector3(-0.15625f, -0.65625f, 0);
    public static readonly Vector2 LegRightPos = new Vector3(0.15625f, -0.65625f, 0);
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
        float velocity = Mathf.Sqrt(Mathf.Abs(RB.velocity.x)) * 3f * Mathf.Sign(RB.velocity.x * Dir);
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

        if (TouchingGround)
        {
            LegLeft.transform.localPosition = LegLeftPos + inverse + new Vector2(2 / 32f, 0) * walkSpeedMultiplier;
            LegRight.transform.localPosition = LegRightPos + circularMotion + new Vector2(-2 / 32f, 0) * walkSpeedMultiplier;
            Head.transform.localPosition = new Vector3(0, Mathf.Sin(walkCounter * 2 + Mathf.PI) * 1f / 32f, 0);
            Body.transform.localPosition = new Vector3(0, Mathf.Sin(walkCounter * 2 + Mathf.PI / 2f) * 1f / 32f, 0);
            float leftAngle = Mathf.Sin(walkCounter) * -15 * walkSpeedMultiplier;
            float rightAngle = -leftAngle;
            if (Grapple == null && armTargetPos == Vector2.zero)
                ArmLeft.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z, leftAngle, 0.07f);
            if (RightArmTargetPos == Vector2.zero)
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
            if (RightArmTargetPos == Vector2.zero)
                ArmRight.transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(ArmRight.transform.localEulerAngles.z, 30 * jumpSpeedMultiplier, 0.05f);
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
            //if(ForceArmDir != 0)
            //{
            //    float z = ArmLeft.transform.localEulerAngles.z;
            //    float r = toHook.ToRotation() * Mathf.Rad2Deg + 90f + useRecoil;
            //    if (r < z && ForceArmDir == 1)
            //        r += 360;
            //    ArmLeft.transform.localEulerAngles = Vector3.forward *
            //        (Mathf.Lerp(z,
            //        r,
            //        prevDir != Dir ? 1 : ArmLerpSpeed));
            //}
            //else
                ArmLeft.transform.localEulerAngles = Vector3.forward *
                    (Mathf.LerpAngle(ArmLeft.transform.localEulerAngles.z,
                    toHook.ToRotation() * Mathf.Rad2Deg + 90f + useRecoil,
                    prevDir != Dir ? 1 : ArmLerpSpeed));
        }
        if (RightArmTargetPos != Vector2.zero)
        {
            Vector2 toHook = RightArmTargetPos - (Vector2)ArmRight.transform.position;
            toHook.x *= Dir;
            //if (ForceArmDir == 0)
            //{
            //    float z = ArmRight.transform.localEulerAngles.z;
            //    float r = toHook.ToRotation() * Mathf.Rad2Deg + 90f + useRecoil;
            //    if (r < z && ForceArmDir == 0)
            //        r += 360;
            //    ArmRight.transform.localEulerAngles = Vector3.forward *
            //        (Mathf.Lerp(z,
            //        r,
            //        prevDir != Dir ? 1 : ArmLerpSpeed));
            //}
            //else
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
    }
}

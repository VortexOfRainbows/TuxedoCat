using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public Player Player;
    public GameObject Owner;
    public Rigidbody2D OwnerBody;
    public GameObject GrapplePointPrefab;
    public Rigidbody2D RB;
    public List<GameObject> points;
    public bool Attached = false;
    public bool AttachedPrev = false;
    public bool Retracting = false;
    public float targetLength = 0;
    public float minDist = 0.65f;
    public float maxDist = 10;
    public float pixelsInRope = 6f;
    private int PointCount = 25;
    private float timeAttached = 0;
    private float RetractDelay = 0;
    public Collider2D SolidCollider;
    public void FixedUpdate()
    {
        transform.SetParent(null);
        if (targetLength != 0)
            UpdatePoints();
        bool isLantern = Player == null;
        bool isPlayer = Player != null;
        if(isLantern && maxDist == 10)
        {
            maxDist = ((Vector2)transform.position - (Vector2)Owner.transform.position + new Vector2(0, 0.2f).RotatedBy(OwnerBody.rotation * Mathf.Deg2Rad)).magnitude - 0.2f;
        }
        bool goUp = isPlayer && Player.Control.Up;
        bool goDown = !isPlayer || Player.Control.Down;
        bool onWall = !isPlayer || Player.TimeSpentNotColliding < 4;
        float pullSpeed = 4.7f * Mathf.Min(1, timeAttached * 2);
        float pushSpeed = (isLantern ? 1.0f : 1.5f) * Mathf.Min(1, timeAttached * 2);
        if (Attached)
        {
            SolidCollider.enabled = false;
            if(!isLantern)
                OwnerBody.gravityScale = 0.675f;
            timeAttached += Time.fixedDeltaTime;
            if (!AttachedPrev)
            {
                targetLength = (transform.position - Owner.transform.position).magnitude;
                OwnerBody.velocity *= 1.1f;
                RB.velocity *= 0.0f;
                RB.gravityScale = 0;
                if(Player.Instance.UseAnimation < Player.Instance.anim.AnimSpeed * 2)
                {
                    Player.Instance.UseAnimation = (int)(Player.Instance.anim.AnimSpeed) * 2;
                }
            }
            if(onWall)
            {
                if(isPlayer)
                    OwnerBody.velocity = new Vector2(Mathf.Abs(OwnerBody.velocity.x) > 3 ? OwnerBody.velocity.x * 0.7f : OwnerBody.velocity.x * 0.985f, OwnerBody.velocity.y > 0 ? OwnerBody.velocity.y * 0.5f : OwnerBody.velocity.y * 0.94f);
                else
                    OwnerBody.velocity = new Vector2(OwnerBody.velocity.x, OwnerBody.velocity.y * 0.9875f);
            }
            else
            {
                OwnerBody.velocity = new Vector2(OwnerBody.velocity.x, OwnerBody.velocity.y * 0.9875f);
            }
            Vector2 toGrapple = transform.position - Owner.transform.position;
            float origDistance = toGrapple.magnitude;
            if (targetLength == 0)
            {
                targetLength = origDistance;
                PointCount = (int)(targetLength / pixelsInRope * 32);
                UpdatePoints();
            }
            toGrapple -= OwnerBody.velocity * Time.fixedDeltaTime;
            float distance = toGrapple.magnitude;
            if (Player != null && Player.anim.prevTouchingGround)
                targetLength = distance;
            if (goUp)
            {
                if (origDistance < targetLength)
                    targetLength = origDistance;
                if(Mathf.Abs(targetLength - distance) < pullSpeed * Time.fixedDeltaTime)
                    targetLength -= pullSpeed * Time.fixedDeltaTime;
            }
            else if (goDown)
            {
                if (onWall)
                    targetLength = distance;
                else
                {
                    if (origDistance > targetLength)
                        targetLength = origDistance;
                    if (Mathf.Abs(targetLength - distance) < pushSpeed * Time.fixedDeltaTime)
                        targetLength += pushSpeed * Time.fixedDeltaTime;
                }
            }
            if(toGrapple.y < 0) //Player is above the grapple
            {
                targetLength = Mathf.Min(origDistance, distance);
            }
            targetLength = Mathf.Clamp(targetLength, minDist, maxDist);
            float distDiff = distance - targetLength;
            bool snapToDistance = true;

            if (goUp && distDiff < 0)
                snapToDistance = false;
            if (goDown && distDiff > 0)
                snapToDistance = false;
            if (targetLength == maxDist || targetLength == minDist)
                snapToDistance = true;

            if (snapToDistance)
                OwnerBody.velocity += toGrapple.normalized * distDiff / Time.fixedDeltaTime;
        }
        Vector2 toGrapple2 = transform.position + (Vector3)RB.velocity * Time.fixedDeltaTime - Owner.transform.position - (Vector3)OwnerBody.velocity * Time.fixedDeltaTime;
        targetLength = toGrapple2.magnitude;
        if (!Attached)
        {
            if(targetLength > maxDist)
            {
                Retracting = true;
            }
        }
        if (Player != null && !Player.IsUsingItem)
            Retracting = true;
        if (Retracting)
        {
            float lerp = Mathf.Clamp((1 - RetractDelay / 25f) * 0.2f, 0, 0.2f);
            if (--RetractDelay <= 0)
            {
                SolidCollider.enabled = false;
                lerp = 1;
            }
            else
            {
                RB.rotation = toGrapple2.ToRotation() * Mathf.Rad2Deg;
            }
            if (!isLantern)
                OwnerBody.gravityScale = 1f;
            float speed = RB.velocity.magnitude + 1f;
            if (Attached)
            {
                speed = 10;
            }
            Attached = false;
            RB.velocity = Vector2.Lerp(RB.velocity, - toGrapple2.normalized * speed, lerp);
            if (targetLength < minDist)
            {
                Destroy(gameObject);
            }
        }
        if(RB.velocity.sqrMagnitude > 0.2f && !Retracting)
        {
            RB.rotation = RB.velocity.ToRotation() * Mathf.Rad2Deg;
        }
        if(Player == null)
        {
            OwnerBody.rotation = toGrapple2.ToRotation() * Mathf.Rad2Deg - 90;
            OwnerBody.velocity *= 0.995f;
        }
        //else if (toNearLastPoint.sqrMagnitude > 0.01f)
        //    targetRotation = (-toNearLastPoint).ToRotation() * Mathf.Rad2Deg;
        //RB.rotation = Mathf.LerpAngle(RB.rotation, targetRotation, 0.2f);
        AttachedPrev = Attached;
    }
    private bool RunOnce = true;
    public void UpdatePoints()
    {
        if(points == null)
            points = new List<GameObject>();
        while(points.Count < PointCount)
        {
            points.Add(Instantiate(GrapplePointPrefab, transform.position, Quaternion.identity));
        }
        Vector3 itemPos = Player != null ? Player.anim.ItemSprite.transform.position : (Vector2)Owner.transform.position + new Vector2(0, 0.2f).RotatedBy(OwnerBody.rotation * Mathf.Deg2Rad);
        float distance = (transform.position - itemPos).magnitude;
        float distancePercent = distance / maxDist;
        float dp = distancePercent;
        if (Attached)
            dp = 1;
        if (Retracting)
        {
            distancePercent = 2.2f;
            dp = Mathf.Min(dp * 2f, 1);
        }
        Vector2 prevPoint = (Vector2)itemPos;
        Vector2 itemVelocity = Player != null ? (Vector2)Player.anim.ItemSprite.transform.position - Player.anim.oldItemPos : Vector3.zero;
        Vector2 ownerVelo = itemVelocity / Time.fixedDeltaTime + OwnerBody.velocity;
        float added = RunOnce ? 1 : 0f;
        float rotationV = RB.velocity.ToRotation();
        for (int i = 0; i < points.Count; ++i)
        {
            float percent = i / ((float)points.Count - 1);
            float iPercent = 1 - percent;
            Vector3 targetPos = Vector3.Lerp(itemPos, transform.position, percent);
            points[i].transform.position = Vector3.Lerp(points[i].transform.position, targetPos, 0.05f + 0.09f * distancePercent);
            points[i].transform.position += (Vector3)RB.velocity * Time.fixedDeltaTime * percent * percent;
            points[i].transform.position -= (Vector3)ownerVelo * Time.fixedDeltaTime * percent * iPercent;
            points[i].transform.position += (Vector3)ownerVelo * Time.fixedDeltaTime * iPercent * iPercent;
            Vector2 toPrev = prevPoint - (Vector2)points[i].transform.position;
            if (Player != null)
            {
                float sin = Mathf.Sin(Mathf.PI * i / 3f);
                points[i].transform.position += (0.5f + dp * 0.5f) * (Vector3)(new Vector2(0, sin * 0.0675f * (1 - dp) * (0.1f + 0.9f * iPercent)).RotatedBy(Mathf.LerpAngle(rotationV * Mathf.Rad2Deg, (-toPrev).ToRotation() * Mathf.Rad2Deg, 0.5f) * Mathf.Deg2Rad));
            }
            toPrev = prevPoint - (Vector2)points[i].transform.position;
            points[i].transform.GetChild(0).transform.eulerAngles = new Vector3(0, 0, toPrev.ToRotation() * Mathf.Rad2Deg);
            float horizontalScale = toPrev.magnitude * 32f / pixelsInRope; //32 pixels per unit, 6 pixel width sprite
            points[i].transform.GetChild(0).transform.localScale = new Vector3(horizontalScale, Mathf.Clamp(0.8f - horizontalScale * 0.14f, 0, 1), 1f);
            prevPoint = points[i].transform.position;
        }
        RunOnce = false;
    }
    public void OnDestroy()
    {
        for (int i = 0; i < points.Count; ++i)
            Destroy(points[i]);
        if(Player != null)
        {
            Player.UseAnimation = 0;
        }
        if(OwnerBody != null)
            OwnerBody.gravityScale = 1f;
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("World"))
        {
            if(collision.gameObject.layer == 10) //Special Layer for specifically climable objects
                Attached = !Retracting;
        }
    }
    public void OnCollisionEnter2D(Collision2D collision) => OnCollision2D(collision);
    public void OnCollisionStay2D(Collision2D collision) => OnCollision2D(collision);
    public void OnCollision2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("World") || collision.gameObject.CompareTag("Standable")) && collision.gameObject.layer != 10)
        {
            if (!Attached)
            {
                if (!Retracting)
                {
                    Retracting = true;
                    RetractDelay = 25;
                }
            }
        }
    }
}

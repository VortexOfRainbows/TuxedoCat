using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public GameObject GrapplePointPrefab;
    public Rigidbody2D RB;
    public List<GameObject> points;
    public bool Attached = false;
    public bool AttachedPrev = false;
    public bool Retracting = false;
    public float targetLength = 0;
    public float minDist = 0.65f;
    public float maxDist = 10;
    /// <summary>
    /// ToDo: Improve visuals
    /// </summary>
    public void FixedUpdate()
    {
        UpdatePoints();
        bool goUp = Player.Control.Up;
        bool goDown = Player.Control.Down;
        bool onWall = Player.Instance.TimeSpentNotColliding < 4;
        float pullSpeed = 4f;
        float pushSpeed = 1f;
        if (Attached)
        {
            if(!AttachedPrev)
            {
                Player.Instance.RB.velocity *= 0.4f;
                RB.velocity *= 0.0f;
                RB.gravityScale = 0;
            }
            if(onWall)
            {
                Player.Instance.RB.velocity = new Vector2(Mathf.Abs(Player.Instance.RB.velocity.x) > 3 ? Player.Instance.RB.velocity.x * 0.5f : Player.Instance.RB.velocity.x * 0.95f, Player.Instance.RB.velocity.y > 0 ? Player.Instance.RB.velocity.y * 0.5f : Player.Instance.RB.velocity.y * 0.94f);
            }
            else
            {
                Player.Instance.RB.velocity = new Vector2(Player.Instance.RB.velocity.x, Player.Instance.RB.velocity.y * 0.99f);
            }
            Vector2 toGrapple = transform.position - Player.Position;
            float origDistance = toGrapple.magnitude;
            toGrapple -= Player.Instance.RB.velocity * Time.fixedDeltaTime;
            float distance = toGrapple.magnitude;
            if (goUp)
            {
                if (origDistance < targetLength)
                    targetLength = origDistance;
                if(Mathf.Abs(targetLength - distance) < pullSpeed * Time.fixedDeltaTime)
                    targetLength -= pullSpeed * Time.fixedDeltaTime;
            }
            if (goDown)
            {
                if(onWall)
                {
                    targetLength = distance;
                }
                else
                {
                    if (origDistance > targetLength)
                        targetLength = origDistance;
                    if(Mathf.Abs(targetLength - distance) < pushSpeed * Time.fixedDeltaTime)
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
                Player.Instance.RB.velocity += toGrapple.normalized * distDiff / Time.fixedDeltaTime;
        }
        Vector2 toGrapple2 = transform.position + (Vector3)RB.velocity * Time.fixedDeltaTime - Player.Position - (Vector3)Player.Instance.RB.velocity * Time.fixedDeltaTime;
        targetLength = toGrapple2.magnitude;
        if (!Attached)
        {
            if(targetLength > maxDist)
            {
                Retracting = true;
            }
        }
        if (!Player.Control.MouseLeft)
            Retracting = true;
        if (Retracting)
        {
            float speed = RB.velocity.magnitude + 1f;
            if(Attached)
            {
                speed = 10;
            }
            Attached = false;
            RB.velocity = -toGrapple2.normalized * speed;
            if(targetLength < minDist)
            {
                Destroy(gameObject);
            }
        }
        Vector2 toNearLastPoint = points[points.Count - 6].transform.position - transform.position;
        float targetRotation = RB.rotation;
        if(RB.velocity.sqrMagnitude > 0.2f && !Retracting)
            RB.rotation = RB.velocity.ToRotation() * Mathf.Rad2Deg;
        else if (toNearLastPoint.sqrMagnitude > 0.01f)
            targetRotation = (-toNearLastPoint).ToRotation() * Mathf.Rad2Deg;
        RB.rotation = Mathf.LerpAngle(RB.rotation, targetRotation, 0.2f);
        AttachedPrev = Attached;
    }
    public void UpdatePoints()
    {
        if(points == null)
            points = new List<GameObject>();
        while(points.Count < 25)
        {
            points.Add(Instantiate(GrapplePointPrefab, transform.position, Quaternion.identity));
        }
        float distance = (transform.position - Player.Position).magnitude;
        float distancePercent = distance / maxDist;
        if (Retracting)
            distancePercent = 2.2f;
        Vector2 prevPoint = (Vector2)Player.Position;
        for (int i = 0; i < points.Count; ++i)
        {
            float percent = i / ((float)points.Count - 1);
            float iPercent = 1 - percent;
            Vector3 targetPos = Vector3.Lerp(Player.Position, transform.position, percent);
            points[i].transform.position = Vector3.Lerp(points[i].transform.position, targetPos, 0.03f + 0.09f * distancePercent);
            points[i].transform.position += (Vector3)RB.velocity * Time.fixedDeltaTime * percent * percent;
            points[i].transform.position -= (Vector3)Player.Instance.RB.velocity * Time.fixedDeltaTime * percent * iPercent;
            points[i].transform.position += (Vector3)Player.Instance.RB.velocity * Time.fixedDeltaTime * iPercent * iPercent;
            Vector2 toPrev = prevPoint - (Vector2)points[i].transform.position;
            points[i].transform.GetChild(0).transform.eulerAngles = new Vector3(0, 0, toPrev.ToRotation() * Mathf.Rad2Deg);
            points[i].transform.GetChild(0).transform.localScale = new Vector3(toPrev.magnitude, 0.1f, 0.1f);
            prevPoint = points[i].transform.position;
        }
    }
    public void OnDestroy()
    {
        for (int i = 0; i < points.Count; ++i)
            Destroy(points[i]);
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("World"))
        {
            Attached = !Retracting;
        }
    }
}

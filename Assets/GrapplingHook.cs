using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public GameObject GrapplePointPrefab;
    public Rigidbody2D RB;
    public List<GameObject> points;
    bool Attached = false;
    bool AttachedPrev = false;
    float speedNextFrame = 0;
    float targetLength = 0;
    /// <summary>
    /// ToDo: Allow right click to move up or down the grapple when combinedw ith Up or Down keys.
    /// ToDo: Fix the right click to make it more intuitive and make momentum transfter properly
    /// ToDo: Improve visuals, add max length, etc.
    /// </summary>
    public void FixedUpdate()
    {
        bool goUp = Player.Control.Up;
        bool goDown = Player.Control.Down;
        bool notOnWall = Player.Instance.TimeSpentNotColliding > 5;
        float minDist = 0.65f;
        float maxDist = 10;
        if (Attached)
        {
            if(!AttachedPrev)
            {
                Player.Instance.RB.velocity *= 0.4f;
                RB.velocity *= 0.0f;
                RB.gravityScale = 0;
            }

            Vector3 toGrapple = transform.position - Player.Position;
            float moveUpOrDown = goUp ? -4 : goDown ? 2 : 0;
            float actualLength = toGrapple.magnitude;
            toGrapple.y *= 0.5f;
            if (goDown && actualLength > targetLength)
                targetLength = actualLength;
            if (goUp && actualLength < targetLength)
                targetLength = actualLength;
            targetLength += moveUpOrDown * Time.fixedDeltaTime;
            if (notOnWall)
            {
                targetLength = Mathf.Clamp(targetLength, minDist, maxDist);
                Vector2 targetGrappleVelo = toGrapple.normalized * (0.2f - Mathf.Abs(moveUpOrDown) * Time.fixedDeltaTime);
                Player.Instance.RB.velocity = new Vector2(Player.Instance.RB.velocity.x, Player.Instance.RB.velocity.y * 0.99f);
                Player.Instance.RB.velocity += targetGrappleVelo;
                float currentSpeed = Player.Instance.RB.velocity.magnitude;
                Vector3 nextPosition = (Vector2)Player.Position + Player.Instance.RB.velocity * Time.fixedDeltaTime;
                Vector2 toGrappleFromNext = transform.position - nextPosition;
                float nextLength = toGrappleFromNext.magnitude;
                float distanceThatNeedsToBeMadeUp = targetLength - nextLength;
                if (((distanceThatNeedsToBeMadeUp < 0 && goUp) || (distanceThatNeedsToBeMadeUp > 0 && goDown) || (!goDown && !goUp) || nextLength < maxDist || nextLength > minDist))
                    Player.Instance.RB.velocity -= (toGrappleFromNext.normalized * distanceThatNeedsToBeMadeUp / Time.fixedDeltaTime); //fix velocity to the grapple point
            }
            else
            {
                if (targetLength > maxDist)
                {
                    float distanceINeedToTravel = targetLength - maxDist;
                    Debug.Log(distanceINeedToTravel);
                    Player.Instance.transform.position += toGrapple.normalized * distanceINeedToTravel;
                }
            }
        }
        if(!Attached)
        {
            Vector2 toGrapple = transform.position - Player.Position;
            targetLength = toGrapple.magnitude + RB.velocity.magnitude * Time.fixedDeltaTime;
            if(targetLength > maxDist)
            {
                Destroy(gameObject);
            }
        }
        UpdatePoints();
        if(RB.velocity.sqrMagnitude > 0.2f)
        {
            RB.rotation = RB.velocity.ToRotation() * Mathf.Rad2Deg; 
        }
        AttachedPrev = Attached;
    }
    public void UpdatePoints()
    {
        if(points == null)
            points = new List<GameObject>();
        while(points.Count < 10)
        {
            points.Add(Instantiate(GrapplePointPrefab, transform.position, Quaternion.identity));
        }
        for(int i = 0; i < points.Count; ++i)
        {
            float percent = i / (float)points.Count;
            Vector3 targetPos = Vector3.Lerp(Player.Position, transform.position, percent);
            points[i].transform.position = Vector3.Lerp(points[i].transform.position, targetPos, 1);
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
            Attached = true;
        }
    }
}

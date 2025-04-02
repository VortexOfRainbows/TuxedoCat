using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public GameObject GrapplePointPrefab;
    public Rigidbody2D RB;
    public List<GameObject> points;
    bool Attached = false;
    float speedNextFrame = 0;
    float targetLength = 0;
    /// <summary>
    /// ToDo: Allow right click to move up or down the grapple when combinedw ith Up or Down keys.
    /// ToDo: Fix the right click to make it more intuitive and make momentum transfter properly
    /// ToDo: Improve visuals, add max length, etc.
    /// </summary>
    public void FixedUpdate()
    {
        if(Attached)
        {
            RB.velocity *= 0.2f;
            Vector2 toGrapple = transform.position - Player.Position;
            targetLength = toGrapple.magnitude;
            Vector2 targetGrappleVelo = toGrapple.normalized * (0.4f + speedNextFrame);
            speedNextFrame = 0;
            if(!Player.Control.MouseRight)
                Player.Instance.RB.velocity *= 0.96f;
            Player.Instance.RB.velocity += targetGrappleVelo;
            float currentSpeed = Player.Instance.RB.velocity.magnitude;
            if(Player.Control.MouseRight)
            {
                Vector3 nextPosition = (Vector2)Player.Position + Player.Instance.RB.velocity * Time.fixedDeltaTime;
                Vector2 toGrappleFromNext = transform.position - nextPosition;
                float nextLength = toGrappleFromNext.magnitude;
                float distanceThatNeedsToBeMadeUp = targetLength - nextLength;
                if(distanceThatNeedsToBeMadeUp > 0)
                {
                    Player.Instance.RB.velocity -= (toGrappleFromNext.normalized * distanceThatNeedsToBeMadeUp / Time.fixedDeltaTime);

                    float speedLost = currentSpeed - Player.Instance.RB.velocity.magnitude;
                    speedNextFrame += speedLost;

                }
            }
        }
        UpdatePoints();
        if(RB.velocity.sqrMagnitude > 0.2f)
        {
            RB.rotation = RB.velocity.ToRotation() * Mathf.Rad2Deg; 
        }
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
            points[i].transform.position = Vector3.Lerp(points[i].transform.position, targetPos, 0.2f * (1.2f - percent));
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

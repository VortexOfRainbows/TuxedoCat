using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gore : MonoBehaviour
{
    private SpriteRenderer r;
    private Rigidbody2D RB;
    private PolygonCollider2D c2D;
    private void Start()
    {
        r = GetComponent<SpriteRenderer>();
        RB = gameObject.AddComponent<Rigidbody2D>();
        c2D = gameObject.AddComponent<PolygonCollider2D>();
        RB.includeLayers = c2D.includeLayers = LayerMask.GetMask("World");
        RB.excludeLayers = c2D.excludeLayers = LayerMask.GetMask("Goon", "Player", "Gore");
        gameObject.transform.parent = null;
        RB.velocity = Utils.RandCircle(5) + Vector2.up * 2;
        foreach(SpriteRenderer r2 in GetComponentsInChildren<SpriteRenderer>())
        {
            r2.sortingOrder -= 10;
            r2.gameObject.layer = 9;
        }
        //r.sortingOrder -= 10;
    }
    private float timer = 0;
    private void FixedUpdate()
    {
        RB.velocity = new Vector2(RB.velocity.x * 0.98f, RB.velocity.y);
        RB.rotation += RB.velocity.x * 2;
        timer++;
        if(timer > 150)
        {
            float percent = 1 - (timer - 150) / 150f;
            //r.color = new Color(r.color.r, r.color.g, r.color.b, percent);
            foreach (SpriteRenderer r2 in GetComponentsInChildren<SpriteRenderer>())
                r2.color = new Color(r2.color.r, r2.color.g, r2.color.b, percent);
            if (percent <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

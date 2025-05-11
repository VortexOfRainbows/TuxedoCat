using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public List<GameObject> gradients = new();
    public GameObject visual;
    public float timer = 0;
    public int ItemType = 0;
    public bool PickedUp = false;
    public void FixedUpdate()
    {
        timer++;

        visual.transform.localPosition = new Vector3(0, Mathf.Sin(timer * Mathf.Deg2Rad) * 0.12f, 0);
        visual.transform.localEulerAngles = Mathf.Cos(timer * 0.34f * Mathf.Deg2Rad) * 15f * Vector3.forward;

        int c = 20;
        while(gradients.Count < c)
            gradients.Add(Instantiate(gradients[0], visual.transform));
        for(int i = 0; i < gradients.Count; ++i)
        {
            int dir = i % 2 * 2 - 1;
            gradients[i].transform.localEulerAngles = Vector3.forward * (360f / c * i + timer * 0.25f * dir);
            float sin = Mathf.Sin(timer * Mathf.Deg2Rad + i % 2 * 180);
            gradients[i].transform.localScale = new Vector3(0.06f + 0.005f * sin, 2.8f - sin * 0.3f, 1);
        }

        if(Utils.RandFloat() < 0.2f)
        {
            Color c2 = gradients[0].GetComponent<SpriteRenderer>().color;
            Vector2 norm = Utils.RandCircle(1).normalized;
            ParticleManager.NewParticle((Vector2)transform.position + norm * Utils.RandFloat(0.3f, 1f), Utils.RandFloat(0.5f, 0.7f), norm + Utils.RandCircle(1), 0.5f, Utils.RandFloat(0.4f, 0.5f), 0, c2);
        }
    }
    public void Acquire()
    {
        PickedUp = true;
        if (ItemType == 0)
            Player.HasHook = true;
        if (ItemType == 1)
            Player.HasGun = true;
        if (ItemType == 2)
            Player.HasClaw = true;
        if (ItemType == 3)
            Player.HasDrone = true;
        if(ItemType >= 0 && ItemType <= 3)
        {
            Player.Instance.ItemType = ItemType;
        }
        Color c2 = gradients[0].GetComponent<SpriteRenderer>().color;
        for(int i = 0; i < 24; ++ i)
        {
            Vector2 norm = Utils.RandCircle(1).normalized;
            ParticleManager.NewParticle((Vector2)transform.position + norm * Utils.RandFloat(0f, .5f), Utils.RandFloat(0.7f, 1.5f), norm * Utils.RandFloat(2, 4) + Utils.RandCircle(2), 0.5f, Utils.RandFloat(1f, 2f), 0, c2);
        }
        Destroy(gameObject);
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 6 && !PickedUp) //Player layer
        {
            Acquire();
        }
    }
}

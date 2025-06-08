using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableCrate : MonoBehaviour
{
    public GameObject[] gores;
    public bool IsBroken = false;
    public void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 8 && collision.gameObject.TryGetComponent(out CatClawSlash slash))
        {
            if(!IsBroken)
                OnBreak();
        }
    }
    public void OnBreak()
    {
        AudioManager.PlaySound(SoundID.Wood, transform.position, 0.5f, 1.0f);
        IsBroken = true;
        foreach(GameObject g in gores)
            g.AddComponent<Gore>();
        for (int i = 0; i < 20; ++i)
        {
            ParticleManager.NewParticle(transform.position, Utils.RandFloat(1f, 2f), Vector2.up, 6f, Utils.RandFloat(1f, 2f), 0, new Color(117 / 255f, 76 / 255f, 43 / 255f));
        }
        for (int i = 0; i < 3; ++i)
        {
            GameObject g = Projectile.NewProjectile<Cheese>(transform.position, Utils.RandCircle(3) + Vector2.up * 3);
            g.GetComponent<ProjComponents>().rb.gravityScale = Utils.RandFloat(0.3f, 0.5f);
            g.GetComponent<Projectile>().Hostile = false;
            g.transform.localScale *= Utils.RandFloat(0.7f, 1.05f);
        }
        if(Main.World != null)
            Main.World.SetTile(Main.World.WorldToCell(transform.position), null);
        Destroy(gameObject);
    }
}

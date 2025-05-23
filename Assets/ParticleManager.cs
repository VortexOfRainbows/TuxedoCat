using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;
    public List<ParticleSystem> systems;
    public static void NewParticle(Vector2 pos, float size, Vector2 velo = default, float randomizeFactor = 0, float lifeTime = 0.5f, int type = 0, Color color = default)
    {
        ParticleSystem.EmitParams style = new ParticleSystem.EmitParams
        {
            position = pos,
            startSize = Instance.systems[type].main.startSizeMultiplier * size * Utils.RandFloat(0.9f, 1.1f),
            velocity = new Vector2(Utils.RandFloat(-1f, 1f), Utils.RandFloat(-1f, 1f)) * randomizeFactor + velo,
            startLifetime = lifeTime,
        };
        if(type != 1)
            style.startColor = color;
        if (type == 3)
        {
            velo = (Vector2)style.velocity;
            velo.x *= -1;
            style.rotation = (velo.ToRotation()) * Mathf.Rad2Deg - 90;
        }
        else
        {
            style.rotation = Utils.RandFloat(360);
        }
        Instance.systems[type].Emit(style, 1);
    }
    void Start()
    {
        Instance = this;
    }
    void Update()
    {
        Instance = this;
    }
}

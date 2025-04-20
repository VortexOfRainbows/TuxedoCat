using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;
    public List<ParticleSystem> systems;
    public static void NewParticle(Vector2 pos, float size, Vector2 velo = default, float randomizeFactor = 0, float lifeTime = 0.5f, int type = 0, Color color = default)
    {
        if (color == default)
            color = Color.white;
        ParticleSystem.EmitParams style = new ParticleSystem.EmitParams
        {
            position = pos,
            rotation = Utils.RandFloat(360),
            startSize = size * Utils.RandFloat(0.9f, 1.1f),
            startColor = color,
            velocity = new Vector2(Utils.RandFloat(-1f, 1f), Utils.RandFloat(-1f, 1f)) * randomizeFactor + velo,
            startLifetime = lifeTime,
        };
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

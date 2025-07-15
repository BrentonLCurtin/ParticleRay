using System.Numerics;
using Raylib_cs;

namespace ParticleRay;

public class TrailPoint
{
    public Vector2 Position { get; set; }
    public float Life { get; set; }
    public float MaxLife { get; }
    
    public TrailPoint(Vector2 position, float maxLife = 2f)
    {
        Position = position;
        Life = MaxLife = maxLife;
    }
    
    public void Update(float deltaTime)
    {
        Life -= deltaTime;
    }
    
    public bool IsAlive => Life > 0;
    public float LifeRatio => Life / MaxLife;
}
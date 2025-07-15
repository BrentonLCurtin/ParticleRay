using System.Numerics;
using Raylib_cs;

namespace ParticleRay;

public class Particle
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Color Color { get; set; }
    public float Size { get; set; }
    public float Life { get; set; }
    public float MaxLife { get; private set; }
    
    private static readonly Random Random = new();
    
    public Particle(Vector2 position)
    {
        Position = position;
        
        Color = new Color(
            Random.Next(100, 255),
            Random.Next(100, 255),
            Random.Next(100, 255),
            255
        );
        
        Velocity = new Vector2();
        Size = 4f;
        MaxLife = Life = 2f;
    }
    
    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
        Life -= deltaTime;
        
        if (Life > 0)
        {
            byte alpha = (byte)(255 * (Life / MaxLife));
            Color = new Color(Color.R, Color.G, Color.B, alpha);
        }
    }
    
    public bool IsAlive => Life > 0;
    
    public void SetLife(float life)
    {
        Life = MaxLife = life;
    }
}
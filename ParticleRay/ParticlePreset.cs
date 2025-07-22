using System.Numerics;
using Raylib_cs;

namespace ParticleRay;

public enum PresetType
{
    Custom,
    Fireworks,
    Fountain,
    Snow,
    Rain,
    Galaxy
}

public class ParticlePreset
{
    public string Name { get; set; }
    public float HoverSpawnRate { get; set; }
    public float ClickSpawnRate { get; set; }
    public float Gravity { get; set; }
    public float Drag { get; set; }
    public float MinSpeed { get; set; }
    public float MaxSpeed { get; set; }
    public float MinSize { get; set; }
    public float MaxSize { get; set; }
    public float MinLife { get; set; }
    public float MaxLife { get; set; }
    public bool UseColorTheme { get; set; }
    public Color[] ColorPalette { get; set; }
    public Func<Vector2>? CustomVelocityGenerator { get; set; }
    
    public ParticlePreset(string name)
    {
        Name = name;
        ColorPalette = new[] { Color.White };
    }
    
    public static ParticlePreset CreateFireworks()
    {
        return new ParticlePreset("Fireworks")
        {
            HoverSpawnRate = 0f,
            ClickSpawnRate = 150f,
            Gravity = 200f,
            Drag = 0.98f,
            MinSpeed = 300f,
            MaxSpeed = 500f,
            MinSize = 3f,
            MaxSize = 8f,
            MinLife = 1.5f,
            MaxLife = 3f,
            UseColorTheme = true,
            ColorPalette = new[]
            {
                new Color(255, 100, 100, 255), // Red
                new Color(100, 255, 100, 255), // Green
                new Color(100, 100, 255, 255), // Blue
                new Color(255, 255, 100, 255), // Yellow
                new Color(255, 100, 255, 255), // Magenta
                new Color(100, 255, 255, 255)  // Cyan
            }
        };
    }
    
    public static ParticlePreset CreateFountain()
    {
        return new ParticlePreset("Fountain")
        {
            HoverSpawnRate = 50f,
            ClickSpawnRate = 200f,
            Gravity = 300f,
            Drag = 0.99f,
            MinSpeed = 200f,
            MaxSpeed = 400f,
            MinSize = 2f,
            MaxSize = 4f,
            MinLife = 2f,
            MaxLife = 4f,
            UseColorTheme = true,
            ColorPalette = new[]
            {
                new Color(100, 150, 255, 255), // Light blue
                new Color(150, 200, 255, 255), // Lighter blue
                new Color(200, 220, 255, 255)  // Very light blue
            },
            CustomVelocityGenerator = () =>
            {
                float angle = (float)((Random.Shared.NextDouble() - 0.5) * Math.PI * 0.5 - Math.PI / 2);
                float speed = (float)(Random.Shared.NextDouble() * 200 + 200);
                return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
            }
        };
    }
    
    public static ParticlePreset CreateSnow()
    {
        return new ParticlePreset("Snow")
        {
            HoverSpawnRate = 30f,
            ClickSpawnRate = 100f,
            Gravity = 50f,
            Drag = 0.99f,
            MinSpeed = 20f,
            MaxSpeed = 50f,
            MinSize = 2f,
            MaxSize = 6f,
            MinLife = 5f,
            MaxLife = 10f,
            UseColorTheme = true,
            ColorPalette = new[]
            {
                new Color(255, 255, 255, 255), // White
                new Color(240, 240, 255, 255), // Slight blue tint
                new Color(230, 230, 240, 255)  // Slight gray
            },
            CustomVelocityGenerator = () =>
            {
                float xSpeed = (float)(Random.Shared.NextDouble() - 0.5) * 30;
                float ySpeed = (float)(Random.Shared.NextDouble() * 30 + 20);
                return new Vector2(xSpeed, ySpeed);
            }
        };
    }
    
    public static ParticlePreset CreateRain()
    {
        return new ParticlePreset("Rain")
        {
            HoverSpawnRate = 50f,
            ClickSpawnRate = 200f,
            Gravity = 800f,
            Drag = 0.999f,
            MinSpeed = 300f,
            MaxSpeed = 400f,
            MinSize = 1f,
            MaxSize = 2f,
            MinLife = 1f,
            MaxLife = 2f,
            UseColorTheme = true,
            ColorPalette = new[]
            {
                new Color(150, 180, 220, 200), // Light blue semi-transparent
                new Color(180, 200, 230, 180), // Lighter blue
                new Color(200, 210, 240, 160)  // Very light blue
            },
            CustomVelocityGenerator = () =>
            {
                float xSpeed = (float)(Random.Shared.NextDouble() - 0.5) * 20;
                float ySpeed = (float)(Random.Shared.NextDouble() * 100 + 300);
                return new Vector2(xSpeed, ySpeed);
            }
        };
    }
    
    public static ParticlePreset CreateGalaxy()
    {
        return new ParticlePreset("Galaxy")
        {
            HoverSpawnRate = 20f,
            ClickSpawnRate = 80f,
            Gravity = 0f,
            Drag = 0.995f,
            MinSpeed = 50f,
            MaxSpeed = 150f,
            MinSize = 1f,
            MaxSize = 4f,
            MinLife = 3f,
            MaxLife = 8f,
            UseColorTheme = true,
            ColorPalette = new[]
            {
                new Color(255, 200, 255, 255), // Pink
                new Color(200, 200, 255, 255), // Light purple
                new Color(255, 255, 200, 255), // Light yellow
                new Color(200, 255, 255, 255), // Cyan
                new Color(255, 220, 200, 255)  // Peach
            },
            CustomVelocityGenerator = () =>
            {
                float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
                float speed = (float)(Random.Shared.NextDouble() * 100 + 50);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                // Add some rotational velocity
                float rotationForce = 30f;
                velocity.X += -velocity.Y * rotationForce * 0.01f;
                velocity.Y += velocity.X * rotationForce * 0.01f;
                
                return velocity;
            }
        };
    }
    
    public Color GetRandomColor()
    {
        if (!UseColorTheme || ColorPalette.Length == 0)
            return new Color(
                (byte)Random.Shared.Next(100, 256),
                (byte)Random.Shared.Next(100, 256),
                (byte)Random.Shared.Next(100, 256),
                (byte)255
            );
        
        return ColorPalette[Random.Shared.Next(ColorPalette.Length)];
    }
}
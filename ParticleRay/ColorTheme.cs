using Raylib_cs;

namespace ParticleRay;

public enum ThemeType
{
    Rainbow,
    Fire,
    Ocean,
    Forest,
    Sunset,
    Aurora,
    Monochrome,
    Custom
}

public class ColorTheme
{
    public string Name { get; set; }
    public Color[] Colors { get; set; }
    public bool SmoothTransition { get; set; }
    
    public ColorTheme(string name, Color[] colors, bool smoothTransition = true)
    {
        Name = name;
        Colors = colors;
        SmoothTransition = smoothTransition;
    }
    
    public Color GetColor(float t)
    {
        if (Colors.Length == 0) return Color.White;
        if (Colors.Length == 1) return Colors[0];
        
        if (!SmoothTransition)
        {
            int index = (int)(t * Colors.Length) % Colors.Length;
            return Colors[index];
        }
        
        // Smooth color interpolation
        float scaledT = t * (Colors.Length - 1);
        int index1 = (int)scaledT;
        int index2 = (index1 + 1) % Colors.Length;
        float localT = scaledT - index1;
        
        return LerpColor(Colors[index1], Colors[index2], localT);
    }
    
    public Color GetRandomColor()
    {
        if (Colors.Length == 0) return Color.White;
        return Colors[Random.Shared.Next(Colors.Length)];
    }
    
    private static Color LerpColor(Color a, Color b, float t)
    {
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t)
        );
    }
    
    public static ColorTheme CreateRainbow()
    {
        return new ColorTheme("Rainbow", new[]
        {
            new Color(255, 0, 0, 255),     // Red
            new Color(255, 127, 0, 255),   // Orange
            new Color(255, 255, 0, 255),   // Yellow
            new Color(0, 255, 0, 255),     // Green
            new Color(0, 0, 255, 255),     // Blue
            new Color(75, 0, 130, 255),    // Indigo
            new Color(148, 0, 211, 255)    // Violet
        });
    }
    
    public static ColorTheme CreateFire()
    {
        return new ColorTheme("Fire", new[]
        {
            new Color(255, 0, 0, 255),     // Red
            new Color(255, 50, 0, 255),    // Red-orange
            new Color(255, 100, 0, 255),   // Orange
            new Color(255, 150, 0, 255),   // Light orange
            new Color(255, 200, 0, 255),   // Yellow-orange
            new Color(255, 255, 100, 255)  // Yellow
        });
    }
    
    public static ColorTheme CreateOcean()
    {
        return new ColorTheme("Ocean", new[]
        {
            new Color(0, 50, 100, 255),    // Deep blue
            new Color(0, 100, 150, 255),   // Medium blue
            new Color(0, 150, 200, 255),   // Light blue
            new Color(0, 200, 255, 255),   // Cyan
            new Color(100, 220, 255, 255), // Light cyan
            new Color(200, 240, 255, 255)  // Very light blue
        });
    }
    
    public static ColorTheme CreateForest()
    {
        return new ColorTheme("Forest", new[]
        {
            new Color(34, 139, 34, 255),   // Forest green
            new Color(0, 128, 0, 255),     // Green
            new Color(85, 107, 47, 255),   // Dark olive green
            new Color(107, 142, 35, 255),  // Olive drab
            new Color(154, 205, 50, 255),  // Yellow green
            new Color(173, 255, 47, 255)   // Green yellow
        });
    }
    
    public static ColorTheme CreateSunset()
    {
        return new ColorTheme("Sunset", new[]
        {
            new Color(255, 94, 77, 255),   // Light red
            new Color(255, 127, 80, 255),  // Coral
            new Color(255, 165, 0, 255),   // Orange
            new Color(255, 192, 203, 255), // Pink
            new Color(255, 105, 180, 255), // Hot pink
            new Color(138, 43, 226, 255)   // Blue violet
        });
    }
    
    public static ColorTheme CreateAurora()
    {
        return new ColorTheme("Aurora", new[]
        {
            new Color(0, 255, 127, 255),   // Spring green
            new Color(0, 255, 255, 255),   // Cyan
            new Color(0, 191, 255, 255),   // Deep sky blue
            new Color(138, 43, 226, 255),  // Blue violet
            new Color(255, 0, 255, 255),   // Magenta
            new Color(255, 20, 147, 255)   // Deep pink
        });
    }
    
    public static ColorTheme CreateMonochrome()
    {
        return new ColorTheme("Monochrome", new[]
        {
            new Color(255, 255, 255, 255), // White
            new Color(200, 200, 200, 255), // Light gray
            new Color(150, 150, 150, 255), // Medium gray
            new Color(100, 100, 100, 255), // Dark gray
            new Color(150, 150, 150, 255), // Medium gray
            new Color(200, 200, 200, 255)  // Light gray
        });
    }
}
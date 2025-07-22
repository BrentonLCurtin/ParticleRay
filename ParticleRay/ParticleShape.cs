using System.Numerics;
using Raylib_cs;

namespace ParticleRay;

public enum ShapeType
{
    Circle,
    Square,
    Star,
    Heart,
    Triangle,
    Diamond
}

public static class ParticleShapes
{
    public static void DrawParticle(Vector2 position, float size, Color color, ShapeType shape)
    {
        switch (shape)
        {
            case ShapeType.Circle:
                Raylib.DrawCircleV(position, size, color);
                break;
                
            case ShapeType.Square:
                Raylib.DrawRectanglePro(
                    new Rectangle(position.X, position.Y, size * 2, size * 2),
                    new Vector2(size, size),
                    0f,
                    color
                );
                break;
                
            case ShapeType.Star:
                DrawStar(position, size, color);
                break;
                
            case ShapeType.Heart:
                DrawHeart(position, size, color);
                break;
                
            case ShapeType.Triangle:
                DrawTriangle(position, size, color);
                break;
                
            case ShapeType.Diamond:
                DrawDiamond(position, size, color);
                break;
        }
    }
    
    private static void DrawStar(Vector2 center, float size, Color color)
    {
        // Simple 4-pointed star using two overlapping squares
        float sqrt2 = (float)Math.Sqrt(2);
        float halfSize = size / sqrt2;
        
        // First diamond (rotated square)
        Vector2 top = center + new Vector2(0, -size);
        Vector2 right = center + new Vector2(size, 0);
        Vector2 bottom = center + new Vector2(0, size);
        Vector2 left = center + new Vector2(-size, 0);
        
        // Second diamond (45 degree rotated)
        Vector2 topRight = center + new Vector2(halfSize, -halfSize);
        Vector2 bottomRight = center + new Vector2(halfSize, halfSize);
        Vector2 bottomLeft = center + new Vector2(-halfSize, halfSize);
        Vector2 topLeft = center + new Vector2(-halfSize, -halfSize);
        
        // Draw first diamond
        Raylib.DrawTriangle(top, left, right, color);
        Raylib.DrawTriangle(bottom, right, left, color);
        
        // Draw second diamond
        Raylib.DrawTriangle(topRight, topLeft, bottomRight, color);
        Raylib.DrawTriangle(bottomLeft, bottomRight, topLeft, color);
    }
    
    private static void DrawHeart(Vector2 center, float size, Color color)
    {
        float scale = size / 10f;
        
        // Draw heart using two circles and a triangle
        Vector2 leftCircle = center + new Vector2(-3 * scale, -3 * scale);
        Vector2 rightCircle = center + new Vector2(3 * scale, -3 * scale);
        
        Raylib.DrawCircleV(leftCircle, 4 * scale, color);
        Raylib.DrawCircleV(rightCircle, 4 * scale, color);
        
        Vector2 bottom = center + new Vector2(0, 6 * scale);
        Vector2 left = center + new Vector2(-6 * scale, 0);
        Vector2 right = center + new Vector2(6 * scale, 0);
        
        Raylib.DrawTriangle(bottom, left, right, color);
        
        // Fill the gap in the middle
        Raylib.DrawRectangleV(center + new Vector2(-3 * scale, -2 * scale), new Vector2(6 * scale, 4 * scale), color);
    }
    
    private static void DrawTriangle(Vector2 center, float size, Color color)
    {
        Vector2 top = center + new Vector2(0, -size);
        Vector2 bottomLeft = center + new Vector2(-size * 0.866f, size * 0.5f);
        Vector2 bottomRight = center + new Vector2(size * 0.866f, size * 0.5f);
        
        Raylib.DrawTriangle(bottomLeft, bottomRight, top, color);
    }
    
    private static void DrawDiamond(Vector2 center, float size, Color color)
    {
        Vector2 top = center + new Vector2(0, -size);
        Vector2 right = center + new Vector2(size * 0.7f, 0);
        Vector2 bottom = center + new Vector2(0, size);
        Vector2 left = center + new Vector2(-size * 0.7f, 0);
        
        // Draw two triangles to form the diamond
        Raylib.DrawTriangle(top, left, right, color);
        Raylib.DrawTriangle(bottom, right, left, color);
    }
}
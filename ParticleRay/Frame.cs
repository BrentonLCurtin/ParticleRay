using System.Numerics;
using Raylib_cs;

namespace ParticleRay;

public class Frame
{
    public List<TrailSegment> TrailSegments { get; set; } = new();
    public List<ParticleSnapshot> ParticleSnapshots { get; set; } = new();
    public int FrameNumber { get; set; }
    
    public Frame(int frameNumber)
    {
        FrameNumber = frameNumber;
    }
    
    public Frame Clone()
    {
        var clone = new Frame(FrameNumber);
        
        // Deep clone trail segments
        foreach (var segment in TrailSegments)
        {
            var clonedSegment = new TrailSegment();
            foreach (var point in segment.Points)
            {
                clonedSegment.Points.Add(new TrailPoint(point.Position, point.Life));
            }
            clone.TrailSegments.Add(clonedSegment);
        }
        
        // Clone particle snapshots
        foreach (var particle in ParticleSnapshots)
        {
            clone.ParticleSnapshots.Add(new ParticleSnapshot
            {
                Position = particle.Position,
                Velocity = particle.Velocity,
                Size = particle.Size,
                Color = particle.Color,
                Shape = particle.Shape,
                Life = particle.Life
            });
        }
        
        return clone;
    }
}

public class ParticleSnapshot
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Size { get; set; }
    public Color Color { get; set; }
    public ShapeType Shape { get; set; }
    public float Life { get; set; }
    
    public static ParticleSnapshot FromParticle(Particle particle)
    {
        return new ParticleSnapshot
        {
            Position = particle.Position,
            Velocity = particle.Velocity,
            Size = particle.Size,
            Color = particle.Color,
            Shape = particle.Shape,
            Life = particle.Life
        };
    }
    
    public Particle ToParticle()
    {
        var particle = new Particle(Position)
        {
            Velocity = Velocity,
            Size = Size,
            Color = Color,
            Shape = Shape
        };
        particle.SetLife(Life);
        return particle;
    }
}
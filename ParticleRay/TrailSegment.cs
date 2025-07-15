using System.Collections.Generic;

namespace ParticleRay;

public class TrailSegment
{
    public List<TrailPoint> Points { get; } = new();
    
    public bool IsAlive => Points.Count > 0 && Points.Any(p => p.IsAlive);
}
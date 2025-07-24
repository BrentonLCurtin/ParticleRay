namespace ParticleRay;

public class AnimationSystem
{
    private List<Frame> frames = new();
    private int currentFrameIndex = 0;
    private bool isPlaying = false;
    private float playbackSpeed = 12f; // FPS
    private float frameTimer = 0f;
    private bool loop = true;
    
    public List<Frame> Frames => frames;
    public int CurrentFrameIndex => currentFrameIndex;
    public bool IsPlaying => isPlaying;
    public float PlaybackSpeed => playbackSpeed;
    public bool Loop => loop;
    public int FrameCount => frames.Count;
    
    public Frame? CurrentFrame => frames.Count > 0 ? frames[currentFrameIndex] : null;
    
    public AnimationSystem()
    {
        // Start with one empty frame
        AddNewFrame();
    }
    
    public void Update(float deltaTime)
    {
        if (!isPlaying || frames.Count <= 1)
            return;
        
        frameTimer += deltaTime;
        float frameDuration = 1f / playbackSpeed;
        
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            NextFrame();
        }
    }
    
    public void Play()
    {
        if (frames.Count > 1)
        {
            isPlaying = true;
            frameTimer = 0f;
        }
    }
    
    public void Pause()
    {
        isPlaying = false;
    }
    
    public void Stop()
    {
        isPlaying = false;
        currentFrameIndex = 0;
        frameTimer = 0f;
    }
    
    public void SetPlaybackSpeed(float fps)
    {
        playbackSpeed = Math.Clamp(fps, 1f, 60f);
    }
    
    public void SetLooping(bool shouldLoop)
    {
        loop = shouldLoop;
    }
    
    public void NextFrame()
    {
        if (frames.Count == 0) return;
        
        currentFrameIndex++;
        if (currentFrameIndex >= frames.Count)
        {
            if (loop)
            {
                currentFrameIndex = 0;
            }
            else
            {
                currentFrameIndex = frames.Count - 1;
                Pause();
            }
        }
    }
    
    public void PreviousFrame()
    {
        if (frames.Count == 0) return;
        
        currentFrameIndex--;
        if (currentFrameIndex < 0)
        {
            if (loop)
            {
                currentFrameIndex = frames.Count - 1;
            }
            else
            {
                currentFrameIndex = 0;
            }
        }
    }
    
    public void GoToFrame(int index)
    {
        if (frames.Count == 0) return;
        currentFrameIndex = Math.Clamp(index, 0, frames.Count - 1);
    }
    
    public Frame AddNewFrame()
    {
        var frame = new Frame(frames.Count);
        frames.Add(frame);
        return frame;
    }
    
    public Frame InsertFrame(int index)
    {
        var frame = new Frame(index);
        frames.Insert(Math.Clamp(index, 0, frames.Count), frame);
        
        // Update frame numbers
        for (int i = 0; i < frames.Count; i++)
        {
            frames[i].FrameNumber = i;
        }
        
        return frame;
    }
    
    public void DeleteFrame(int index)
    {
        if (frames.Count <= 1) return; // Keep at least one frame
        
        frames.RemoveAt(index);
        
        // Update frame numbers
        for (int i = 0; i < frames.Count; i++)
        {
            frames[i].FrameNumber = i;
        }
        
        // Adjust current frame if needed
        if (currentFrameIndex >= frames.Count)
        {
            currentFrameIndex = frames.Count - 1;
        }
    }
    
    public void DuplicateCurrentFrame()
    {
        if (CurrentFrame == null) return;
        
        var duplicate = CurrentFrame.Clone();
        duplicate.FrameNumber = currentFrameIndex + 1;
        frames.Insert(currentFrameIndex + 1, duplicate);
        
        // Update frame numbers
        for (int i = 0; i < frames.Count; i++)
        {
            frames[i].FrameNumber = i;
        }
        
        // Move to the duplicated frame
        currentFrameIndex++;
    }
    
    public void ClearCurrentFrame()
    {
        if (CurrentFrame == null) return;
        
        CurrentFrame.TrailSegments.Clear();
        CurrentFrame.ParticleSnapshots.Clear();
    }
    
    public Frame? GetFrame(int index)
    {
        if (index < 0 || index >= frames.Count) return null;
        return frames[index];
    }
    
    public void InsertFrame(int index, Frame frame)
    {
        frames.Insert(Math.Clamp(index, 0, frames.Count), frame);
        
        // Update frame numbers
        for (int i = 0; i < frames.Count; i++)
        {
            frames[i].FrameNumber = i;
        }
    }
    
    public void SaveCurrentState(List<TrailSegment> trails, List<Particle> particles)
    {
        if (CurrentFrame == null) return;
        
        // Clear existing data
        CurrentFrame.TrailSegments.Clear();
        CurrentFrame.ParticleSnapshots.Clear();
        
        // Save trail segments
        foreach (var segment in trails)
        {
            var savedSegment = new TrailSegment();
            foreach (var point in segment.Points)
            {
                savedSegment.Points.Add(new TrailPoint(point.Position, point.Life));
            }
            CurrentFrame.TrailSegments.Add(savedSegment);
        }
        
        // Save particle snapshots
        foreach (var particle in particles)
        {
            CurrentFrame.ParticleSnapshots.Add(ParticleSnapshot.FromParticle(particle));
        }
    }
    
    public (List<TrailSegment>, List<Particle>) LoadFrameData(Frame frame)
    {
        var trails = new List<TrailSegment>();
        var particles = new List<Particle>();
        
        // Load trail segments
        foreach (var segment in frame.TrailSegments)
        {
            var loadedSegment = new TrailSegment();
            foreach (var point in segment.Points)
            {
                loadedSegment.Points.Add(new TrailPoint(point.Position, point.Life));
            }
            trails.Add(loadedSegment);
        }
        
        // Load particles
        foreach (var snapshot in frame.ParticleSnapshots)
        {
            particles.Add(snapshot.ToParticle());
        }
        
        return (trails, particles);
    }
}
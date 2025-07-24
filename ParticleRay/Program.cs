using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

namespace ParticleRay;

class Program
{
    private static PresetType currentPreset = PresetType.Custom;
    private static ParticlePreset activePreset = new ParticlePreset("Custom");
    private static ThemeType currentTheme = ThemeType.Rainbow;
    private static ColorTheme activeTheme = ColorTheme.CreateRainbow();
    private static bool useColorTheme = false;
    private static float colorTransitionSpeed = 1f;
    private static ShapeType currentShape = ShapeType.Circle;
    private static bool randomizeShapes = false;
    private static List<Particle> particles = new();
    private static List<TrailSegment> sparklerSegments = new();
    private static Queue<Particle> particlePool = new();
    private static int maxParticles = 50000;
    private static AnimationSystem animationSystem = new();
    private static int lastLoadedFrame = -1;
    private static float currentColorTime = 0f;
    private static TrailSegment? currentSegment = null;
    private static float hoverSpawnRate = 10f;
    private static float clickSpawnRate = 100f;
    private static float particleGravity = 100f;
    private static float particleDrag = 0.98f;
    private static float minSpeed = 50f;
    private static float maxSpeed = 250f;
    private static float minSize = 2f;
    private static float maxSize = 6f;
    private static float minLife = 1f;
    private static float maxLife = 3f;
    private static float sparklerTrailLife = 3f;
    private static float sparklerThickness = 8f;
    private static bool showImGui = true;
    private static bool autoSpawn = false;
    private static bool infiniteTrails = false;
    private static float autoSpawnTimer = 0f;
    private static bool isMouseOverImGui = false;
    private static Vector2? lastMousePos = null;
    private static bool showOnionSkin = false;
    private static float onionSkinOpacity = 0.3f;
    private static bool autoSaveFrame = true;
    private static UndoRedoSystem undoRedoSystem = new();
    
    static void Main()
    {
        // Enable MSAA and fullscreen before creating window
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.FullscreenMode | ConfigFlags.TopmostWindow | ConfigFlags.AlwaysRunWindow);
        
        // Get monitor size for fullscreen
        int screenWidth = Raylib.GetMonitorWidth(0);
        int screenHeight = Raylib.GetMonitorHeight(0);
        
        Raylib.InitWindow(screenWidth, screenHeight, "ParticleRay - Interactive Particle Playground");
        Raylib.HideCursor();
        
        // Ensure window is maximized and on top
        Raylib.MaximizeWindow();
        
        rlImGui.Setup(true);
        
        // Configure ImGui for high DPI displays
        ImGuiIOPtr io = ImGui.GetIO();
        float dpiScale = Math.Max(1.0f, Raylib.GetWindowScaleDPI().X);
        io.FontGlobalScale = dpiScale;
        
        while (!Raylib.WindowShouldClose())
        {
            float deltaTime = Raylib.GetFrameTime();
            
            Update(deltaTime);
            Draw();
        }
        
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
    
    static void Update(float deltaTime)
    {
        currentColorTime += deltaTime * colorTransitionSpeed;
        Vector2 mousePos = Raylib.GetMousePosition();
        bool isMousePressed = Raylib.IsMouseButtonDown(MouseButton.Left);
        bool isTouchActive = Raylib.GetTouchPointCount() > 0;
        
        // Update animation system
        animationSystem.Update(deltaTime);
        
        // Handle key inputs first (before checking if playing)
        if (Raylib.IsKeyPressed(KeyboardKey.H))
        {
            showImGui = !showImGui;
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.C))
        {
            if (animationSystem.CurrentFrame != null)
            {
                var action = new ClearFrameAction(animationSystem.CurrentFrame, animationSystem.CurrentFrameIndex);
                undoRedoSystem.ExecuteAction(action);
                particles.Clear();
                sparklerSegments.Clear();
                currentSegment = null;
            }
        }
        
        // Frame navigation hotkeys
        if (Raylib.IsKeyPressed(KeyboardKey.Left))
        {
            SaveCurrentFrame(); // Save before switching
            animationSystem.PreviousFrame();
            LoadCurrentFrame();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            SaveCurrentFrame(); // Save before switching
            animationSystem.NextFrame();
            LoadCurrentFrame();
        }
        
        // A/D navigation (only when Ctrl is not held)
        if (!Raylib.IsKeyDown(KeyboardKey.LeftControl) && !Raylib.IsKeyDown(KeyboardKey.RightControl))
        {
            if (Raylib.IsKeyPressed(KeyboardKey.A))
            {
                SaveCurrentFrame(); // Save before switching
                animationSystem.PreviousFrame();
                LoadCurrentFrame();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.D))
            {
                SaveCurrentFrame(); // Save before switching
                animationSystem.NextFrame();
                LoadCurrentFrame();
            }
        }
        
        // Ctrl+A for new frame
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.A))
        {
            SaveCurrentFrame();
            var action = new AddFrameAction(
                animationSystem,
                animationSystem.FrameCount,
                (index) => { animationSystem.GoToFrame(index); ClearCanvas(); },
                () => LoadCurrentFrame()
            );
            undoRedoSystem.ExecuteAction(action);
        }
        
        // Ctrl+X for delete frame
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.X))
        {
            if (animationSystem.FrameCount > 1) // Don't delete if only one frame
            {
                var action = new DeleteFrameAction(
                    animationSystem, 
                    animationSystem.CurrentFrameIndex,
                    () => LoadCurrentFrame(),
                    (index) => { animationSystem.GoToFrame(index); LoadCurrentFrame(); }
                );
                undoRedoSystem.ExecuteAction(action);
            }
        }
        
        // Ctrl+Z for undo
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.Z))
        {
            undoRedoSystem.Undo();
        }
        
        // Ctrl+Y for redo
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.Y))
        {
            undoRedoSystem.Redo();
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            if (animationSystem.IsPlaying)
            {
                animationSystem.Pause();
                lastLoadedFrame = -1; // Reset so we can edit the current frame
            }
            else
            {
                animationSystem.Play();
            }
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            Raylib.CloseWindow();
        }
        
        // Handle frame switching when playing
        if (animationSystem.IsPlaying && animationSystem.CurrentFrame != null)
        {
            // Only load frame data when we switch to a new frame
            if (lastLoadedFrame != animationSystem.CurrentFrameIndex)
            {
                var (loadedTrails, loadedParticles) = animationSystem.LoadFrameData(animationSystem.CurrentFrame);
                sparklerSegments = loadedTrails;
                particles = loadedParticles;
                lastLoadedFrame = animationSystem.CurrentFrameIndex;
            }
            
            // Update particle physics during playback
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.Update(deltaTime);
                
                var vel = particle.Velocity;
                vel *= particleDrag;
                vel.Y += particleGravity * deltaTime;
                particle.Velocity = vel;
                
                if (!particle.IsAlive)
                {
                    particles.RemoveAt(i);
                }
            }
            
            // Update trail segments during playback
            for (int i = sparklerSegments.Count - 1; i >= 0; i--)
            {
                var segment = sparklerSegments[i];
                
                // Update all points in the segment
                foreach (var point in segment.Points)
                {
                    if (!infiniteTrails)
                    {
                        point.Update(deltaTime);
                    }
                }
                
                // Remove dead points from the beginning
                if (!infiniteTrails)
                {
                    while (segment.Points.Count > 0 && !segment.Points[0].IsAlive)
                    {
                        segment.Points.RemoveAt(0);
                    }
                }
                
                // Remove dead segments
                if (!segment.IsAlive)
                {
                    sparklerSegments.RemoveAt(i);
                }
            }
            
            return; // Skip drawing input when playing
        }
        
        // Track sparkler trail when clicking/touching
        if ((isMousePressed || isTouchActive) && !isMouseOverImGui)
        {
            Vector2 currentPos = isTouchActive ? Raylib.GetTouchPosition(0) : mousePos;
            
            // Start a new segment if we don't have one
            if (currentSegment == null)
            {
                currentSegment = new TrailSegment();
                sparklerSegments.Add(currentSegment);
                lastMousePos = null;
            }
            
            // Add interpolated points for smooth lines
            if (lastMousePos.HasValue)
            {
                float distance = Vector2.Distance(lastMousePos.Value, currentPos);
                int pointsToAdd = Math.Max(1, (int)(distance / 2f)); // Add points every 2 pixels
                
                for (int i = 0; i < pointsToAdd; i++)
                {
                    float t = (float)i / pointsToAdd;
                    Vector2 interpolatedPos = Vector2.Lerp(lastMousePos.Value, currentPos, t);
                    currentSegment.Points.Add(new TrailPoint(interpolatedPos, sparklerTrailLife));
                }
            }
            
            currentSegment.Points.Add(new TrailPoint(currentPos, sparklerTrailLife));
            lastMousePos = currentPos;
            
            // Refresh ALL points in the current segment to create pulse effect
            foreach (var point in currentSegment.Points)
            {
                // Keep all points in the active segment at full life
                point.Life = sparklerTrailLife;
            }
        }
        else
        {
            // End the current segment when mouse is released
            currentSegment = null;
            lastMousePos = null;
        }
        
        // Update all segments and their points
        for (int i = sparklerSegments.Count - 1; i >= 0; i--)
        {
            var segment = sparklerSegments[i];
            
            // Update all points in the segment
            foreach (var point in segment.Points)
            {
                if (!infiniteTrails)
                {
                    point.Update(deltaTime);
                }
            }
            
            // Remove dead points from the beginning to create undraw effect
            if (!infiniteTrails)
            {
                while (segment.Points.Count > 0 && !segment.Points[0].IsAlive)
                {
                    segment.Points.RemoveAt(0);
                }
            }
            
            // Remove dead segments
            if (!segment.IsAlive)
            {
                sparklerSegments.RemoveAt(i);
            }
            
            // Generate sparkle particles from active segments
            if (segment.IsAlive && particles.Count < maxParticles && segment.Points.Count > 1)
            {
                // Start from a random offset to avoid always sampling the same points
                int startOffset = Random.Shared.Next(3);
                for (int j = startOffset; j < segment.Points.Count; j += 3) // Sample every 3rd point
                {
                    if (Random.Shared.NextDouble() < 0.2) // Increased chance
                    {
                        var point = segment.Points[j];
                        if (point.LifeRatio > 0.1f)
                        {
                            var sparkle = new Particle(point.Position + new Vector2(
                                (float)(Random.Shared.NextDouble() - 0.5) * 20,
                                (float)(Random.Shared.NextDouble() - 0.5) * 20))
                            {
                                Velocity = new Vector2(
                                    (float)(Random.Shared.NextDouble() - 0.5) * 50,
                                    (float)(Random.Shared.NextDouble() - 0.5) * 50 - 20),
                                Size = (float)(Random.Shared.NextDouble() * 2 + 1),
                                Color = GetParticleColor(),
                                Shape = GetParticleShape()
                            };
                            sparkle.SetLife(0.5f);
                            if (particles.Count < maxParticles)
                            {
                                particles.Add(sparkle);
                            }
                        }
                    }
                }
            }
        }
        
        // Don't spawn particles if mouse is over ImGui window
        if (!isMouseOverImGui)
        {
            // Check if mouse is in window bounds
            bool isMouseInWindow = mousePos.X >= 0 && mousePos.X < Raylib.GetScreenWidth() && 
                                   mousePos.Y >= 0 && mousePos.Y < Raylib.GetScreenHeight();
            
            if (isMouseInWindow || isTouchActive)
            {
                Vector2 spawnPos = isTouchActive ? Raylib.GetTouchPosition(0) : mousePos;
                
                float currentSpawnRate = (isMousePressed || isTouchActive) ? clickSpawnRate : hoverSpawnRate;
                float particlesToSpawnFloat = currentSpawnRate * deltaTime;
                
                // Handle fractional particles with probability
                int particlesToSpawn = (int)particlesToSpawnFloat;
                float fractionalPart = particlesToSpawnFloat - particlesToSpawn;
                if (Random.Shared.NextDouble() < fractionalPart)
                {
                    particlesToSpawn++;
                }
                
                for (int i = 0; i < particlesToSpawn; i++)
                {
                    var jitter = new Vector2(
                        (float)(Random.Shared.NextDouble() - 0.5) * 10,
                        (float)(Random.Shared.NextDouble() - 0.5) * 10
                    );
                    var particle = new Particle(spawnPos + jitter)
                    {
                        Velocity = activePreset.CustomVelocityGenerator?.Invoke() ?? GenerateVelocity(),
                        Size = (float)(Random.Shared.NextDouble() * (maxSize - minSize) + minSize),
                        Color = GetParticleColor(),
                        Shape = GetParticleShape()
                    };
                    particle.SetLife((float)(Random.Shared.NextDouble() * (maxLife - minLife) + minLife));
                    if (particles.Count < maxParticles)
                    {
                        particles.Add(particle);
                    }
                }
            }
        }
        
        if (autoSpawn)
        {
            autoSpawnTimer += deltaTime;
            if (autoSpawnTimer >= 0.1f)
            {
                autoSpawnTimer = 0f;
                var screenCenter = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
                for (int i = 0; i < 5; i++)
                {
                    var particle = new Particle(screenCenter)
                    {
                        Velocity = activePreset.CustomVelocityGenerator?.Invoke() ?? GenerateVelocity(),
                        Size = (float)(Random.Shared.NextDouble() * (maxSize - minSize) + minSize),
                        Color = GetParticleColor(),
                        Shape = GetParticleShape()
                    };
                    particle.SetLife((float)(Random.Shared.NextDouble() * (maxLife - minLife) + minLife));
                    if (particles.Count < maxParticles)
                    {
                        particles.Add(particle);
                    }
                }
            }
        }
        
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var particle = particles[i];
            particle.Update(deltaTime);
            
            var vel = particle.Velocity;
            vel *= particleDrag;
            vel.Y += particleGravity * deltaTime;
            particle.Velocity = vel;
            
            if (!particle.IsAlive)
            {
                particles.RemoveAt(i);
            }
        }
        
        // Auto-save frame when drawing
        if (autoSaveFrame && !animationSystem.IsPlaying)
        {
            animationSystem.SaveCurrentState(sparklerSegments, particles);
        }
    }
    
    static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        
        // Draw onion skin (previous/next frames)
        if (showOnionSkin && !animationSystem.IsPlaying)
        {
            DrawOnionSkin();
        }
        
        // Draw sparkler trail segments
        foreach (var segment in sparklerSegments)
        {
            if (segment.Points.Count > 1)
            {
                for (int i = 1; i < segment.Points.Count; i++)
                {
                    var point1 = segment.Points[i - 1];
                    var point2 = segment.Points[i];
                    
                    // Calculate opacity based on life
                    float opacity1 = point1.LifeRatio;
                    float opacity2 = point2.LifeRatio;
                
                    // Reduced glow layers for performance
                    for (int glow = 2; glow >= 0; glow--)
                    {
                        float glowSize = sparklerThickness + glow * 4;
                        byte glowAlpha = (byte)(30 * opacity2 / (glow + 1));
                        
                        // Golden sparkler color
                        Color glowColor = new Color((byte)255, (byte)200, (byte)100, glowAlpha);
                        
                        // Draw thick line segment
                        DrawThickLine(point1.Position, point2.Position, glowSize, glowColor, opacity1, opacity2);
                    }
                    
                    // Core bright line
                    Color coreColor = new Color((byte)255, (byte)240, (byte)200, (byte)(255 * opacity2));
                    DrawThickLine(point1.Position, point2.Position, sparklerThickness * 0.5f, coreColor, opacity1, opacity2);
                    
                    // Move sparkle particle creation to Update method
                }
            }
        }
        
        foreach (var particle in particles)
        {
            ParticleShapes.DrawParticle(particle.Position, particle.Size, particle.Color, particle.Shape);
        }
        
        Raylib.DrawText($"Particles: {particles.Count} | Frame: {animationSystem.CurrentFrameIndex + 1}/{animationSystem.FrameCount}", 10, 10, 20, Color.White);
        Raylib.DrawText("Hover: Light spawn | Click/Touch: Heavy spawn | H: Toggle UI | C: Clear | Left/Right: Navigate | Ctrl+A: New Frame | Space: Play/Pause | ESC: Exit", 10, 35, 16, Color.Gray);
        
        // Draw playback indicator
        if (animationSystem.IsPlaying)
        {
            Raylib.DrawText("▶ PLAYING", 10, 60, 20, Color.Green);
        }
        
        // Draw custom cursor
        Vector2 mousePos = Raylib.GetMousePosition();
        Raylib.DrawCircleV(mousePos, 5, Color.White);
        Raylib.DrawCircleV(mousePos, 3, Color.Black);
        
        // Always draw the bottom toolbar
        rlImGui.Begin();
        
        DrawBottomToolbar();
        
        if (showImGui)
        {
            DrawImGui();
        }
        
        // Check if mouse is over ImGui after drawing UI
        isMouseOverImGui = ImGui.GetIO().WantCaptureMouse;
        
        rlImGui.End();
        
        Raylib.EndDrawing();
    }
    
    static void DrawBottomToolbar()
    {
        // Create a fixed toolbar at the bottom
        float toolbarHeight = 60f;
        float windowWidth = Raylib.GetScreenWidth();
        float windowHeight = Raylib.GetScreenHeight();
        
        ImGui.SetNextWindowPos(new Vector2(0, windowHeight - toolbarHeight));
        ImGui.SetNextWindowSize(new Vector2(windowWidth, toolbarHeight));
        
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | 
                                ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | 
                                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;
        
        ImGui.Begin("Toolbar", flags);
        
        // Center the controls
        float buttonWidth = 50f;
        float buttonHeight = 40f;
        float spacing = 10f;
        
        // Frame info
        ImGui.Text($"Frame {animationSystem.CurrentFrameIndex + 1}/{animationSystem.FrameCount}");
        ImGui.SameLine();
        
        // Navigation buttons
        if (ImGui.Button("<<##first", new Vector2(buttonWidth, buttonHeight))) 
        { 
            SaveCurrentFrame(); 
            animationSystem.GoToFrame(0);
            LoadCurrentFrame(); 
        }
        ImGui.SameLine();
        
        if (ImGui.Button("<##prev", new Vector2(buttonWidth, buttonHeight))) 
        { 
            SaveCurrentFrame(); 
            animationSystem.PreviousFrame(); 
            LoadCurrentFrame(); 
        }
        ImGui.SameLine();
        
        // Play/Pause button
        string playPauseText = animationSystem.IsPlaying ? "||" : ">";
        if (ImGui.Button(playPauseText + "##playpause", new Vector2(buttonWidth, buttonHeight)))
        {
            if (animationSystem.IsPlaying)
            {
                animationSystem.Pause();
                lastLoadedFrame = -1;
            }
            else
            {
                animationSystem.Play();
            }
        }
        ImGui.SameLine();
        
        if (ImGui.Button(">##next", new Vector2(buttonWidth, buttonHeight))) 
        { 
            SaveCurrentFrame(); 
            animationSystem.NextFrame(); 
            LoadCurrentFrame(); 
        }
        ImGui.SameLine();
        
        if (ImGui.Button(">>##last", new Vector2(buttonWidth, buttonHeight))) 
        { 
            SaveCurrentFrame(); 
            animationSystem.GoToFrame(animationSystem.FrameCount - 1);
            LoadCurrentFrame(); 
        }
        ImGui.SameLine();
        
        ImGui.Dummy(new Vector2(spacing, 0));
        ImGui.SameLine();
        
        // Frame operations
        if (ImGui.Button("+##add", new Vector2(buttonWidth, buttonHeight)))
        {
            SaveCurrentFrame();
            animationSystem.AddNewFrame();
            animationSystem.GoToFrame(animationSystem.FrameCount - 1);
            ClearCanvas();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Add new frame (Ctrl+A)");
        }
        ImGui.SameLine();
        
        if (ImGui.Button("-##delete", new Vector2(buttonWidth, buttonHeight)) && animationSystem.FrameCount > 1)
        {
            var action = new DeleteFrameAction(
                animationSystem, 
                animationSystem.CurrentFrameIndex,
                () => LoadCurrentFrame(),
                (index) => { animationSystem.GoToFrame(index); LoadCurrentFrame(); }
            );
            undoRedoSystem.ExecuteAction(action);
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Delete current frame");
        }
        ImGui.SameLine();
        
        ImGui.Dummy(new Vector2(spacing, 0));
        ImGui.SameLine();
        
        // Clear button
        if (ImGui.Button("Clear##clear", new Vector2(buttonWidth * 1.5f, buttonHeight)))
        {
            if (animationSystem.CurrentFrame != null)
            {
                var action = new ClearFrameAction(animationSystem.CurrentFrame, animationSystem.CurrentFrameIndex);
                undoRedoSystem.ExecuteAction(action);
                particles.Clear();
                sparklerSegments.Clear();
                currentSegment = null;
                if (autoSaveFrame)
                {
                    animationSystem.SaveCurrentState(sparklerSegments, particles);
                }
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Clear current frame (C)");
        }
        ImGui.SameLine();
        
        // Timeline slider
        ImGui.Dummy(new Vector2(spacing, 0));
        ImGui.SameLine();
        ImGui.Text("Timeline:");
        ImGui.SameLine();
        
        int currentFrame = animationSystem.CurrentFrameIndex;
        ImGui.SetNextItemWidth(200f);
        if (ImGui.SliderInt("##Timeline", ref currentFrame, 0, animationSystem.FrameCount - 1))
        {
            SaveCurrentFrame();
            animationSystem.GoToFrame(currentFrame);
            LoadCurrentFrame();
        }
        ImGui.SameLine();
        
        // FPS controls
        ImGui.Dummy(new Vector2(spacing, 0));
        ImGui.SameLine();
        ImGui.Text("Speed:");
        ImGui.SameLine();
        
        float fps = animationSystem.PlaybackSpeed;
        if (ImGui.Button("0.5x##speed05", new Vector2(40f, buttonHeight))) animationSystem.SetPlaybackSpeed(6f);
        ImGui.SameLine();
        if (ImGui.Button("1x##speed1", new Vector2(35f, buttonHeight))) animationSystem.SetPlaybackSpeed(12f);
        ImGui.SameLine();
        if (ImGui.Button("2x##speed2", new Vector2(35f, buttonHeight))) animationSystem.SetPlaybackSpeed(24f);
        ImGui.SameLine();
        
        ImGui.SetNextItemWidth(100f);
        if (ImGui.SliderFloat("##FPS", ref fps, 1f, 60f, "%.0f fps"))
        {
            animationSystem.SetPlaybackSpeed(fps);
        }
        ImGui.SameLine();
        
        // Onion skinning toggle
        ImGui.Dummy(new Vector2(spacing, 0));
        ImGui.SameLine();
        if (ImGui.Checkbox("Onion Skin##toolbar", ref showOnionSkin))
        {
            // Toggle handled automatically
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Show previous (blue) and next (red) frames");
        }
        ImGui.SameLine();
        
        if (showOnionSkin)
        {
            ImGui.SetNextItemWidth(80f);
            ImGui.SliderFloat("##OnionOpacity", ref onionSkinOpacity, 0.1f, 0.5f, "%.1f");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Onion skin opacity");
            }
        }
        
        ImGui.SameLine();
        ImGui.Dummy(new Vector2(spacing, 0));
        ImGui.SameLine();
        
        // UI toggle button
        string uiButtonText = showImGui ? "Hide UI" : "Show UI";
        if (ImGui.Button(uiButtonText + "##toggleui", new Vector2(80f, buttonHeight)))
        {
            showImGui = !showImGui;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Toggle settings windows (H)");
        }
        
        ImGui.End();
    }
    
    static void DrawImGui()
    {
        // Animation Controls Window
        ImGui.Begin("Animation Controls");
        
        ImGui.Text($"Frame {animationSystem.CurrentFrameIndex + 1} of {animationSystem.FrameCount}");
        
        // Frame navigation
        if (ImGui.Button("<<")) { SaveCurrentFrame(); animationSystem.PreviousFrame(); LoadCurrentFrame(); }
        ImGui.SameLine();
        if (animationSystem.IsPlaying)
        {
            if (ImGui.Button("Pause")) animationSystem.Pause();
        }
        else
        {
            if (ImGui.Button("Play")) animationSystem.Play();
        }
        ImGui.SameLine();
        if (ImGui.Button(">>")) { SaveCurrentFrame(); animationSystem.NextFrame(); LoadCurrentFrame(); }
        
        // Frame timeline
        int currentFrame = animationSystem.CurrentFrameIndex;
        if (ImGui.SliderInt("Timeline", ref currentFrame, 0, animationSystem.FrameCount - 1))
        {
            SaveCurrentFrame();
            animationSystem.GoToFrame(currentFrame);
            LoadCurrentFrame();
        }
        
        // Playback settings
        ImGui.Separator();
        ImGui.Text("Playback Settings");
        
        float fps = animationSystem.PlaybackSpeed;
        if (ImGui.Button("0.5x")) animationSystem.SetPlaybackSpeed(6f);
        ImGui.SameLine();
        if (ImGui.Button("1x")) animationSystem.SetPlaybackSpeed(12f);
        ImGui.SameLine();
        if (ImGui.Button("2x")) animationSystem.SetPlaybackSpeed(24f);
        ImGui.SameLine();
        if (ImGui.Button("4x")) animationSystem.SetPlaybackSpeed(48f);
        
        if (ImGui.SliderFloat("FPS", ref fps, 1f, 60f))
        {
            animationSystem.SetPlaybackSpeed(fps);
        }
        
        bool loop = animationSystem.Loop;
        if (ImGui.Checkbox("Loop", ref loop))
        {
            animationSystem.SetLooping(loop);
        }
        
        ImGui.Separator();
        ImGui.Text("Frame Operations");
        
        if (ImGui.Button("Add Frame"))
        {
            SaveCurrentFrame();
            animationSystem.AddNewFrame();
            animationSystem.GoToFrame(animationSystem.FrameCount - 1);
            ClearCanvas();
        }
        ImGui.SameLine();
        if (ImGui.Button("Duplicate"))
        {
            SaveCurrentFrame();
            animationSystem.DuplicateCurrentFrame();
        }
        
        if (ImGui.Button("Insert Before"))
        {
            SaveCurrentFrame();
            animationSystem.InsertFrame(animationSystem.CurrentFrameIndex);
            LoadCurrentFrame();
        }
        ImGui.SameLine();
        if (ImGui.Button("Delete Frame") && animationSystem.FrameCount > 1)
        {
            animationSystem.DeleteFrame(animationSystem.CurrentFrameIndex);
            LoadCurrentFrame();
        }
        
        if (ImGui.Button("Clear Frame"))
        {
            animationSystem.ClearCurrentFrame();
            ClearCanvas();
        }
        
        ImGui.Separator();
        ImGui.Text("Drawing Options");
        ImGui.Checkbox("Auto-save Frame", ref autoSaveFrame);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Automatically saves your drawing to the current frame as you draw.\nDisable to draw without saving (useful for testing).");
        }
        ImGui.Checkbox("Show Onion Skin", ref showOnionSkin);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Shows previous frame (blue) and next frame (red) as ghosts.\nHelps with animation flow.");
        }
        if (showOnionSkin)
        {
            ImGui.SliderFloat("Onion Opacity", ref onionSkinOpacity, 0.1f, 0.5f);
        }
        
        ImGui.End();
        
        // Original Particle Settings Window
        ImGui.Begin("Particle Settings");
        
        ImGui.Text("Presets");
        if (ImGui.BeginCombo("##Preset", currentPreset.ToString()))
        {
            foreach (PresetType preset in Enum.GetValues<PresetType>())
            {
                if (ImGui.Selectable(preset.ToString(), currentPreset == preset))
                {
                    currentPreset = preset;
                    ApplyPreset(preset);
                }
            }
            ImGui.EndCombo();
        }
        ImGui.Separator();
        
        ImGui.Text($"Active Particles: {particles.Count}");
        ImGui.Separator();
        
        ImGui.Text("Spawn Rates");
        ImGui.SliderFloat("Hover Spawn Rate", ref hoverSpawnRate, 0f, 50f);
        ImGui.SliderFloat("Click Spawn Rate", ref clickSpawnRate, 10f, 200f);
        
        ImGui.Separator();
        ImGui.Text("Physics");
        ImGui.SliderFloat("Gravity", ref particleGravity, -500f, 500f);
        ImGui.SliderFloat("Drag", ref particleDrag, 0.9f, 1f);
        
        ImGui.Separator();
        ImGui.Text("Speed Range");
        ImGui.SliderFloat("Min Speed", ref minSpeed, 0f, 500f);
        ImGui.SliderFloat("Max Speed", ref maxSpeed, minSpeed, 500f);
        
        ImGui.Separator();
        ImGui.Text("Size Range");
        ImGui.SliderFloat("Min Size", ref minSize, 1f, 20f);
        ImGui.SliderFloat("Max Size", ref maxSize, minSize, 20f);
        
        ImGui.Separator();
        ImGui.Text("Life Range");
        ImGui.SliderFloat("Min Life", ref minLife, 0.1f, 5f);
        ImGui.SliderFloat("Max Life", ref maxLife, minLife, 5f);
        
        ImGui.Separator();
        ImGui.Text("Performance");
        ImGui.SliderInt("Max Particles", ref maxParticles, 5000, 200000);
        
        ImGui.Separator();
        ImGui.Text("Sparkler Trail");
        ImGui.SliderFloat("Trail Life", ref sparklerTrailLife, 0.5f, 5f);
        ImGui.SliderFloat("Trail Thickness", ref sparklerThickness, 2f, 20f);
        
        ImGui.Separator();
        ImGui.Checkbox("Auto Spawn", ref autoSpawn);
        ImGui.Checkbox("Infinite Trails", ref infiniteTrails);
        
        ImGui.Separator();
        ImGui.Text("Color Theme");
        ImGui.Checkbox("Use Color Theme", ref useColorTheme);
        
        if (useColorTheme)
        {
            if (ImGui.BeginCombo("##Theme", currentTheme.ToString()))
            {
                foreach (ThemeType theme in Enum.GetValues<ThemeType>())
                {
                    if (theme == ThemeType.Custom) continue;
                    if (ImGui.Selectable(theme.ToString(), currentTheme == theme))
                    {
                        currentTheme = theme;
                        ApplyTheme(theme);
                    }
                }
                ImGui.EndCombo();
            }
            
            ImGui.SliderFloat("Transition Speed", ref colorTransitionSpeed, 0.1f, 5f);
        }
        
        ImGui.Separator();
        ImGui.Text("Particle Shapes");
        ImGui.Checkbox("Randomize Shapes", ref randomizeShapes);
        
        if (!randomizeShapes)
        {
            if (ImGui.BeginCombo("##Shape", currentShape.ToString()))
            {
                foreach (ShapeType shape in Enum.GetValues<ShapeType>())
                {
                    if (ImGui.Selectable(shape.ToString(), currentShape == shape))
                    {
                        currentShape = shape;
                    }
                }
                ImGui.EndCombo();
            }
        }
        
        if (ImGui.Button("Clear All"))
        {
            particles.Clear();
            sparklerSegments.Clear();
            currentSegment = null;
        }
        
        ImGui.End();
    }
    
    static Vector2 GenerateVelocity()
    {
        float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
        float speed = (float)(Random.Shared.NextDouble() * (maxSpeed - minSpeed) + minSpeed);
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
    }
    
    static void ApplyPreset(PresetType preset)
    {
        activePreset = preset switch
        {
            PresetType.Fireworks => ParticlePreset.CreateFireworks(),
            PresetType.Fountain => ParticlePreset.CreateFountain(),
            PresetType.Snow => ParticlePreset.CreateSnow(),
            PresetType.Rain => ParticlePreset.CreateRain(),
            PresetType.Galaxy => ParticlePreset.CreateGalaxy(),
            _ => new ParticlePreset("Custom")
        };
        
        if (preset != PresetType.Custom)
        {
            hoverSpawnRate = activePreset.HoverSpawnRate;
            clickSpawnRate = activePreset.ClickSpawnRate;
            particleGravity = activePreset.Gravity;
            particleDrag = activePreset.Drag;
            minSpeed = activePreset.MinSpeed;
            maxSpeed = activePreset.MaxSpeed;
            minSize = activePreset.MinSize;
            maxSize = activePreset.MaxSize;
            minLife = activePreset.MinLife;
            maxLife = activePreset.MaxLife;
        }
    }
    
    static void ApplyTheme(ThemeType theme)
    {
        activeTheme = theme switch
        {
            ThemeType.Rainbow => ColorTheme.CreateRainbow(),
            ThemeType.Fire => ColorTheme.CreateFire(),
            ThemeType.Ocean => ColorTheme.CreateOcean(),
            ThemeType.Forest => ColorTheme.CreateForest(),
            ThemeType.Sunset => ColorTheme.CreateSunset(),
            ThemeType.Aurora => ColorTheme.CreateAurora(),
            ThemeType.Monochrome => ColorTheme.CreateMonochrome(),
            _ => ColorTheme.CreateRainbow()
        };
    }
    
    static Color GetParticleColor()
    {
        if (useColorTheme)
        {
            float t = currentColorTime % 1f;
            return activeTheme.GetColor(t);
        }
        else if (activePreset.UseColorTheme)
        {
            return activePreset.GetRandomColor();
        }
        else
        {
            return new Color(
                (byte)Random.Shared.Next(100, 256),
                (byte)Random.Shared.Next(100, 256),
                (byte)Random.Shared.Next(100, 256),
                (byte)255
            );
        }
    }
    
    static ShapeType GetParticleShape()
    {
        if (randomizeShapes)
        {
            var shapes = Enum.GetValues<ShapeType>();
            return shapes[Random.Shared.Next(shapes.Length)];
        }
        return currentShape;
    }
    
    static void DrawThickLine(Vector2 start, Vector2 end, float thickness, Color color, float startOpacity, float endOpacity)
    {
        Vector2 direction = Vector2.Normalize(end - start);
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * (thickness / 2);
        
        // Create a quad for the thick line
        Vector2 p1 = start - perpendicular;
        Vector2 p2 = start + perpendicular;
        Vector2 p3 = end + perpendicular;
        Vector2 p4 = end - perpendicular;
        
        // Adjust colors based on opacity
        Color startColor = new Color(color.R, color.G, color.B, (byte)(color.A * startOpacity));
        Color endColor = new Color(color.R, color.G, color.B, (byte)(color.A * endOpacity));
        
        // Draw as two triangles
        Raylib.DrawTriangle(p1, p2, p3, startColor);
        Raylib.DrawTriangle(p1, p3, p4, endColor);
    }
    
    static void LoadCurrentFrame()
    {
        if (animationSystem.CurrentFrame == null) return;
        
        var (loadedTrails, loadedParticles) = animationSystem.LoadFrameData(animationSystem.CurrentFrame);
        sparklerSegments = loadedTrails;
        particles = loadedParticles;
    }
    
    static void SaveCurrentFrame()
    {
        animationSystem.SaveCurrentState(sparklerSegments, particles);
    }
    
    static void ClearCanvas()
    {
        particles.Clear();
        sparklerSegments.Clear();
        currentSegment = null;
    }
    
    static void DrawOnionSkin()
    {
        // Draw previous frame
        if (animationSystem.CurrentFrameIndex > 0)
        {
            var prevFrame = animationSystem.Frames[animationSystem.CurrentFrameIndex - 1];
            DrawFrameWithOpacity(prevFrame, onionSkinOpacity * 0.5f, new Color(100, 100, 255, 255));
        }
        
        // Draw next frame
        if (animationSystem.CurrentFrameIndex < animationSystem.FrameCount - 1)
        {
            var nextFrame = animationSystem.Frames[animationSystem.CurrentFrameIndex + 1];
            DrawFrameWithOpacity(nextFrame, onionSkinOpacity * 0.5f, new Color(255, 100, 100, 255));
        }
    }
    
    static void DrawFrameWithOpacity(Frame frame, float opacity, Color tint)
    {
        // Draw trails with opacity
        foreach (var segment in frame.TrailSegments)
        {
            if (segment.Points.Count > 1)
            {
                for (int i = 1; i < segment.Points.Count; i++)
                {
                    var point1 = segment.Points[i - 1];
                    var point2 = segment.Points[i];
                    
                    // Draw thicker lines for better visibility
                    Color lineColor = new Color(
                        (byte)(tint.R),
                        (byte)(tint.G),
                        (byte)(tint.B),
                        (byte)(255 * opacity)
                    );
                    
                    Raylib.DrawLineEx(point1.Position, point2.Position, 4f, lineColor);
                }
            }
        }
        
        // Draw particles with opacity
        foreach (var snapshot in frame.ParticleSnapshots)
        {
            Color particleColor = new Color(
                (byte)(tint.R),
                (byte)(tint.G),
                (byte)(tint.B),
                (byte)(255 * opacity * 0.7f)
            );
            
            ParticleShapes.DrawParticle(snapshot.Position, snapshot.Size * 0.8f, particleColor, snapshot.Shape);
        }
    }
}
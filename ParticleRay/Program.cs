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
        
        if (Raylib.IsKeyPressed(KeyboardKey.H))
        {
            showImGui = !showImGui;
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.C))
        {
            particles.Clear();
            sparklerSegments.Clear();
            currentSegment = null;
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            Raylib.CloseWindow();
        }
    }
    
    static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        
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
        
        Raylib.DrawText($"Particles: {particles.Count}", 10, 10, 20, Color.White);
        Raylib.DrawText("Hover: Light spawn | Click/Touch: Heavy spawn | H: Toggle UI | C: Clear | ESC: Exit", 10, 35, 16, Color.Gray);
        
        // Draw custom cursor
        Vector2 mousePos = Raylib.GetMousePosition();
        Raylib.DrawCircleV(mousePos, 5, Color.White);
        Raylib.DrawCircleV(mousePos, 3, Color.Black);
        
        if (showImGui)
        {
            rlImGui.Begin();
            
            // Check if mouse is over ImGui before drawing UI
            isMouseOverImGui = ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);
            
            DrawImGui();
            rlImGui.End();
        }
        else
        {
            isMouseOverImGui = false;
        }
        
        Raylib.EndDrawing();
    }
    
    static void DrawImGui()
    {
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
}
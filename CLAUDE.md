# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ParticleRay is an interactive particle playground application that creates colorful particle effects and sparkler trails based on mouse/touch input. It runs in fullscreen with a customizable UI for tweaking effects in real-time.

**Current State**: Fully implemented with particle system, sparkler trails, and comprehensive UI controls.

## Development Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Run from project directory
dotnet run --project ParticleRay

# Clean build artifacts
dotnet clean
```

## Architecture & Structure

### Technology Stack
- **Language**: C# (.NET 8.0)
- **Graphics Library**: Raylib-cs (7.0.1)
- **UI Library**: ImGui.NET (1.91.6.1) with rlImgui-cs (3.2.0)
- **Project Type**: Fullscreen graphics application with MSAA and VSync

### Project Layout
```
ParticleRay.sln          # Solution file
ParticleRay/
├── ParticleRay.csproj   # Project configuration
├── Program.cs           # Main application and game loop
├── Particle.cs          # Particle entity with physics
├── TrailPoint.cs        # Trail point for sparkler effect
├── TrailSegment.cs      # Trail segment container
├── bin/                 # Build output
└── obj/                 # Intermediate files
```

## Key Features

### Particle System
- **Hover Spawning**: Particles spawn lightly when hovering (10/sec default)
- **Click Spawning**: Heavy particle spawn on click/touch (100/sec default)
- **Physics**: Configurable gravity and drag
- **Customization**: Size, speed, lifetime, and color ranges

### Sparkler Trail
- **Segmented Drawing**: Each click/drag creates a separate trail segment
- **Fade Effect**: Trails fade over configurable lifetime (3 seconds default)
- **Glow Effect**: Multi-layer glow for realistic sparkler appearance
- **Sparkle Particles**: Random particles emit from the trail

### UI Controls (ImGui)
- Spawn rate sliders for hover and click
- Physics controls (gravity, drag)
- Particle property ranges (speed, size, life)
- Sparkler trail settings (life, thickness)
- Auto-spawn toggle
- Clear all button

### Input & Controls
- **Mouse Hover**: Light particle spawning
- **Left Click/Touch**: Heavy particle spawning + sparkler trail
- **H Key**: Toggle UI visibility
- **C Key**: Clear all particles and trails
- **ESC Key**: Exit application

## Implementation Details

### Window Configuration
```csharp
ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.FullscreenMode | ConfigFlags.TopmostWindow | ConfigFlags.AlwaysRunWindow
```
- Fullscreen borderless window
- 4x MSAA anti-aliasing
- VSync enabled (60 FPS)
- Always on top (covers taskbar/start menu)
- Continues running when unfocused

### Performance Considerations
- Particles are removed when life <= 0
- Trail segments are removed when all points are dead
- Fractional particle spawning for smooth low spawn rates
- Efficient line interpolation for smooth trails

## Development Notes

- Nullable reference types are enabled
- Implicit usings are enabled
- Custom cursor drawn as particles spawn at mouse position
- ImGui blocks particle spawning when mouse is over UI
- No automated tests currently implemented
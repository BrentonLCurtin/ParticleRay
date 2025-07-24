# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ParticleRay is a frame-based animation drawing application that creates colorful particle effects and sparkler trails. It combines a particle playground with animation tools, allowing users to create frame-by-frame animations with dynamic particle physics. The app runs in fullscreen with comprehensive UI controls.

**Current State**: Fully implemented animation system with frame management, playback controls, onion skinning, and particle physics.

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
├── Frame.cs             # Frame data storage for animation
├── AnimationSystem.cs   # Animation playback and frame management
├── ParticleShapes.cs    # Particle shape rendering
├── ParticlePreset.cs    # Preset configurations
├── ColorTheme.cs        # Color theme system
├── bin/                 # Build output
└── obj/                 # Intermediate files
```

## Key Features

### Animation System
- **Frame-based Animation**: Create frame-by-frame animations with particle effects
- **Timeline Navigation**: Scrub through frames with timeline slider
- **Playback Controls**: Adjustable playback speed (1-60 FPS)
- **Onion Skinning**: See previous (blue) and next (red) frames as ghosts
- **Auto-save Frame**: Automatically saves drawings to current frame
- **Frame Management**: Add, delete, duplicate, and clear frames

### Particle System
- **Hover Spawning**: Particles spawn lightly when hovering (10/sec default)
- **Click Spawning**: Heavy particle spawn on click/touch (100/sec default)
- **Physics**: Configurable gravity and drag
- **Customization**: Size, speed, lifetime, and color ranges
- **Live Physics**: Particles continue animating during playback

### Sparkler Trail
- **Segmented Drawing**: Each click/drag creates a separate trail segment
- **Fade Effect**: Trails fade over configurable lifetime (3 seconds default)
- **Glow Effect**: Multi-layer glow for realistic sparkler appearance
- **Sparkle Particles**: Random particles emit from the trail

### UI Controls
- **Fixed Bottom Toolbar**: Always-visible animation controls
  - Frame navigation (first, prev, play/pause, next, last)
  - Add/delete frames
  - Clear current frame
  - Timeline slider
  - Playback speed controls
  - Onion skin toggle with opacity
- **Animation Controls Window** (Toggle with H)
  - Detailed frame operations
  - Playback settings
  - Drawing options
- **Particle Settings Window** (Toggle with H)
  - Spawn rate sliders
  - Physics controls
  - Particle properties
  - Color themes and presets

### Input & Controls
- **Mouse Hover**: Light particle spawning
- **Left Click/Touch**: Heavy particle spawning + sparkler trail
- **Left/Right Arrows**: Navigate between frames
- **Ctrl+A**: Add new frame
- **Space**: Play/pause animation
- **H Key**: Toggle settings windows
- **C Key**: Clear current frame
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
- Frame data only loaded once per frame during playback
- Particle physics continue during animation playback

### Animation Features
- **Frame Storage**: Each frame stores complete particle and trail state
- **Playback System**: Configurable FPS with loop/once modes
- **Frame Operations**: Add, delete, duplicate, insert, clear
- **Onion Skinning**: Previous frame (blue), next frame (red)
- **Auto-save Mode**: Continuously saves drawing to current frame

## Development Notes

- Nullable reference types are enabled
- Implicit usings are enabled
- Custom cursor drawn as particles spawn at mouse position
- ImGui blocks particle spawning when mouse is over UI
- Fixed bottom toolbar always visible for animation controls
- Unique ImGui IDs used for buttons to avoid conflicts
- No automated tests currently implemented
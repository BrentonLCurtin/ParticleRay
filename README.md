# ParticleRay

An interactive particle playground that creates mesmerizing visual effects with particles and sparkler trails. Move your mouse to spawn colorful particles, click and drag to draw glowing sparkler trails that fade over time!

![ParticleRay Demo](demo.gif) *(Add a demo gif here)*

## Features

### 🎨 Interactive Particle System
- **Hover Effects**: Particles gently spawn as you move your mouse
- **Click Burst**: Click or touch for intense particle bursts
- **Customizable Physics**: Adjust gravity, drag, speed, and more in real-time
- **Random Colors**: Each particle has a unique vibrant color

### ✨ Sparkler Trail
- **Draw in the Air**: Click and drag to create glowing trails like a sparkler
- **Realistic Glow**: Multi-layer rendering creates an authentic sparkler effect
- **Fading Magic**: Trails gradually fade away, leaving a temporary mark
- **Sparkle Particles**: Tiny sparks emit from the trail for extra realism

### 🎛️ Real-time Controls
- Adjust spawn rates for hover and click separately
- Control particle physics (gravity, drag)
- Customize particle properties (size, speed, lifetime)
- Configure sparkler trail (thickness, fade time)
- Auto-spawn mode for continuous effects

## Controls

| Input | Action |
|-------|--------|
| **Mouse Move** | Spawn particles lightly |
| **Left Click/Touch** | Heavy particle spawn + draw sparkler trail |
| **H** | Toggle UI panel |
| **C** | Clear all particles and trails |
| **ESC** | Exit application |

## Requirements

- .NET 8.0 or later
- Windows, macOS, or Linux
- Graphics card with OpenGL 3.3 support

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/ParticleRay.git
cd ParticleRay
```

2. Build and run:
```bash
dotnet build
dotnet run --project ParticleRay
```

## Technical Details

- **Framework**: Raylib-cs for high-performance graphics
- **UI**: Dear ImGui via ImGui.NET for the control panel
- **Rendering**: Fullscreen with 4x MSAA anti-aliasing and VSync
- **Performance**: Efficient particle pooling and segment-based trail rendering

## Configuration

The application runs in fullscreen by default. All settings can be adjusted in real-time through the ImGui panel:

- **Spawn Rates**: 0-200 particles/second
- **Gravity**: -500 to 500 (negative for upward motion)
- **Particle Size**: 1-20 pixels
- **Trail Life**: 0.5-5 seconds
- **Trail Thickness**: 2-20 pixels

## License

This project is open source and available under the [MIT License](LICENSE).

## Acknowledgments

- Built with [Raylib-cs](https://github.com/ChrisDill/Raylib-cs)
- UI powered by [Dear ImGui](https://github.com/ocornut/imgui) via [ImGui.NET](https://github.com/mellinoe/ImGui.NET)
- Inspired by the joy of sparklers and particle effects!
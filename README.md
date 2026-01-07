# Genesis Test

A Unity project featuring a third-person combat system with Animancer integration.

## Features

- **Third-Person Combat System**: Punch combos, kicks, and dodge mechanics
- **Animancer Integration**: Smooth animation blending and transitions
- **Hit Detection**: Physics-based combat hit detection
- **Training Dummy**: Interactive dummy with hit reactions

## Requirements

- Unity 2021.3 or later
- Animancer package
- Input System package
- Cinemachine package

## Setup

1. Clone this repository
2. Open the project in Unity
3. Let Unity import all assets
4. Open the main scene in `Assets/Scenes/`

## Combat Controls

- **Punch**: Left Mouse Button / Gamepad Button
- **Kick**: Right Mouse Button / Gamepad Button
- **Dodge**: Space / Gamepad Button
- **Movement**: WASD / Left Stick

## Project Structure

```
Assets/
├── Scripts/
│   ├── CombatSystem/        # Combat mechanics and animation
│   ├── Dummy/               # Training dummy controllers
│   ├── Player/              # Player controllers
│   └── ...
├── Animations/              # Animation clips and controllers
├── Prefabs/                 # Reusable game objects
└── Scenes/                  # Game scenes
```

## Development

This project uses:
- **Git LFS** for large binary files (models, textures, audio)
- **Animancer** for procedural animation control
- **Unity Input System** for cross-platform input handling

## License

[Add your license here]

## Credits

[Add credits here]

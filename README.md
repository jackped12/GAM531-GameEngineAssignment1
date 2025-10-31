# OpenTK 3D Scene - Midterm Jackson Pedvis

A playable 3D demo built in C# and OpenTK 4.x, demonstrating real-time 3D rendering, Phong lighting, textured models, and interactive gameplay in an original small environment.

---

## 📸 Screenshots

![Main Scene Screenshot]("Screenshot 2025-10-31 102242.png")


---

## 📖 Project Description

This project presents a compact, interactive 3D scene using a custom mini-engine designed in OpenTK and .NET. The scene features physically plausible Phong lighting, dynamic player controls, three unique mesh types, texture mapping, object interaction, and modular code for extensibility.

---

## 🎮 Gameplay Instructions

- **Move Camera**: `W`, `A`, `S`, `D` keys
- **Look Around**: Hold right mouse button and move mouse
- **Pick Up Item**: `E` key (when near the pyramid)
- **Toggle Light**: `E` key (when not near any item)
- **Release Cursor / Quit**: `Escape` key

---

## ✨ Features

- **First-person Camera**: Smooth FPS-style movement and mouselook
- **Mesh Variety**: Three 3D shapes – textured cube, pyramid pick-up, and ground plane
- **Real-time Lighting**: Phong (ambient, diffuse, specular) with keyboard toggle
- **Texture Mapping**: PNG texture loading for cube and ground
- **Object Interaction**: Collectible item (proximity-based detection)
- **Code Quality**: Strong OOP design (separate Shader, Mesh, TextureLoader, Camera, Interact classes)
- **Shader-based Rendering**: GLSL 330+ with custom attributes and uniforms

---

## ⚙️ How to Build & Run

### Requirements

- [.NET 6 SDK or newer](https://dotnet.microsoft.com/download)
- [OpenTK 4.x](https://www.nuget.org/packages/OpenTK/)
- [SixLabors.ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp)
- Visual Studio 2022+ (or `dotnet` CLI)




---

## 📝 Credits

- **Geometry**: All models (cube, pyramid, plane) are generated procedurally in code.
- **Textures**:  
https://www.pexels.com/photo/moon-on-a-misty-evening-11403947/
https://www.pexels.com/photo/brown-wooden-floor-172292/
- **All code and shaders**: Original, unless specified above.

---

<!-- Add more screenshots, external source/attribution notes, or setup instructions below if needed. -->



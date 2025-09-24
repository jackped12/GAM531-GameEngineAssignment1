# 3D Cube Demo

## Overview
This project is a simple 3D cube rendering application built using C# and the OpenTK library. It displays a colored, rotating cube that can be controlled using keyboard inputs.

## Library Used
- **OpenTK**: This project uses OpenTK, a C# wrapper for OpenGL, to handle graphics rendering and window management. OpenTK was chosen for its cross-platform support and ease of integration with .NET for rendering 3D graphics.

## Cube Rendering Explanation
The cube is rendered using OpenGL through the OpenTK library. Here's a brief overview of the rendering process:
1. **Setup**:
   - A window is created using OpenTK's `GameWindow` with a resolution of 800x600.
   - OpenGL is initialized with depth testing enabled and a perspective projection matrix for 3D rendering.
   - Vertex and fragment shaders are compiled and linked into a shader program to handle cube rendering.
   - Vertex data (positions and colors) for the cube's six faces is defined and stored in a Vertex Buffer Object (VBO) and Vertex Array Object (VAO).

2. **Rendering**:
   - The cube is rendered as a set of triangles (36 vertices, 6 per face).
   - A Model-View-Projection (MVP) matrix is computed, combining:
     - **Model**: A rotation matrix derived from a quaternion updated based on user input.
     - **View**: A camera positioned at (2.5, 2.5, 2.5) looking at the origin.
     - **Projection**: A perspective projection with a 60-degree field of view.
   - The MVP matrix is passed to the vertex shader to transform the cube's vertices.
   - Each face of the cube is assigned a distinct color (red, green, blue, yellow, magenta, cyan) via the vertex data.

3. **Interactivity**:
   - The cube's rotation is controlled using quaternions, updated in the `OnUpdateFrame` method based on keyboard input (W/S for X-axis rotation, A/D for Y-axis rotation).
   - The `OnRenderFrame` method clears the screen, applies the MVP matrix, and draws the cube using `GL.DrawArrays`.

4. **Cleanup**:
   - Resources (VBO, VAO, shader program) are properly disposed of in the `OnUnload` method to prevent memory leaks.

## Controls
- **W**: Rotate cube up (around X-axis).
- **S**: Rotate cube down (around X-axis).
- **A**: Rotate cube left (around Y-axis).
- **D**: Rotate cube right (around Y-axis).
- **Escape**: Close the application.

## Requirements
- .NET Framework or .NET Core (compatible with OpenTK).
- OpenTK library (available via NuGet: `Install-Package OpenTK`).

## How to Run
1. Clone the repository.
2. Install OpenTK via NuGet.
3. Build and run the solution in a .NET-compatible IDE (e.g., Visual Studio).
4. Use the keyboard controls to interact with the rotating cube.

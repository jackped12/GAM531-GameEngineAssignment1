# Vector & Matrix Demo

This project demonstrates basic vector and matrix operations using C# with OpenTK, a library for OpenGL bindings. It includes a simple graphical application that renders a rotating, colored triangle and performs vector and matrix calculations displayed in the console.

## Prerequisites

- **.NET SDK**: Version 6.0 or later.
- **OpenTK**: Install the `OpenTK` NuGet package (version compatible with .NET 6.0+).
- **Operating System**: Windows (due to console allocation using `kernel32.dll`).

## Setup

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/jackped12/GAM531-GameEngineAssignment1.git
   cd GAM531-GameEngineAssignment1
   ```

2. **Install Dependencies**:
   Ensure the `OpenTK` package is installed. Run the following command in the project directory:
   ```bash
   dotnet add package OpenTK
   ```

3. **Build the Project**:
   ```bash
   dotnet build
   ```

4. **Run the Application**:
   ```bash
   dotnet run
   ```

## Project Structure

- **MatrixOperations.cs**: Contains methods for matrix operations like identity, scaling, rotation, and multiplication using OpenTK's `Matrix4`.
- **VectorOperations.cs**: Defines static vector operations (addition, subtraction, dot product, and cross product) using OpenTK's `Vector3`.
- **Game.cs**: Implements a `GameWindow` using OpenTK to render a rotating triangle with vertex and fragment shaders.
- **Program.cs**: Entry point that launches the game window and demonstrates vector and matrix operations in the console.

## Features

- **Graphical Output**: Displays a rotating, colored triangle scaled to 0.5x using OpenGL.
- **Console Output**: Shows results of vector operations (addition, subtraction, dot product, cross product) and matrix operations (identity, scale, rotation, and combined transformation).
- **Shader Usage**: Uses GLSL shaders for rendering the triangle with dynamic rotation.

## Usage

- Run the application to open a window displaying a rotating triangle.
- A console window will also appear, showing:
  - Vector operations results for predefined vectors `A(1,2,3)` and `B(4,5,6)`.
  - Matrix operations results, including identity matrix, scaling matrix, rotation matrix, and their combination.

## Example Output

### Console Output
```
=== Vector Operations ===
A + B = (5, 7, 9)
A - B = (-3, -3, -3)
Dot(A, B) = 32
Cross(A, B) = (-3, 6, -3)

=== Matrix Operations ===
Identity:
[1, 0, 0, 0]
[0, 1, 0, 0]
[0, 0, 1, 0]
[0, 0, 0, 1]
Scale (2,2,2):
[2, 0, 0, 0]
[0, 2, 0, 0]
[0, 0, 2, 0]
[0, 0, 0, 1]
Rotation Z (90°):
[0, -1, 0, 0]
[1, 0, 0, 0]
[0, 0, 1, 0]
[0, 0, 0, 1]
Combined Scale + Rotation:
[...]
```

### Graphical Output
- A window titled "Vector & Matrix Demo" (800x600) showing a triangle with red, green, and blue vertices, rotating around the Z-axis.

## Notes

- The rotation angle in the graphical demo increments based on frame time, creating a smooth animation.
- The project uses `DllImport` for console allocation, which is Windows-specific. For cross-platform support, consider removing or replacing the console allocation logic.
- Ensure OpenGL drivers are up-to-date for smooth rendering.

## License

This project is licensed under the MIT License.

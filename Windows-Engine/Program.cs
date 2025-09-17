using System;

namespace Windows_Engine
{
    class Program
    {
        static void Main()
        {
            using (Game game = new Game())
            {
                game.Run(); // Launch OpenTK game window
            }

            // Demo vector operations
            Console.WriteLine("\n=== Vector Operations ===");
            Console.WriteLine($"A + B = {VectorOperations.Add}");
            Console.WriteLine($"A - B = {VectorOperations.Subtract}");
            Console.WriteLine($"Dot(A, B) = {VectorOperations.Dot}");
            Console.WriteLine($"Cross(A, B) = {VectorOperations.Cross}");

            // Demo matrix operations
            Console.WriteLine("\n=== Matrix Operations ===");
            var identity = MatrixOperations.Identity();
            var scale = MatrixOperations.Scale(2, 2, 2);
            var rotation = MatrixOperations.RotationZ(MathF.PI / 2);

            Console.WriteLine($"Identity:\n{identity}");
            Console.WriteLine($"Scale (2,2,2):\n{scale}");
            Console.WriteLine($"Rotation Z (90°):\n{rotation}");

            // Combine scale + rotation using Multiply
            var combined = MatrixOperations.Multiply(rotation, scale);
            Console.WriteLine("\nCombined Scale + Rotation:");
            Console.WriteLine(combined);
        }
    }
}

using OpenTK.Windowing.Desktop;
using System;
using Windows_Engine;

namespace Windows_Engine
{
    class Program
    {
        static void Main()
        {
            using var game = new Game(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Title = "OpenTK Phong Scene",
                ClientSize = new OpenTK.Mathematics.Vector2i(1280, 720),
                Vsync = OpenTK.Windowing.Common.VSyncMode.On
            });
            game.Run();
        }
    }
}

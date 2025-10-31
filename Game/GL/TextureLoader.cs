using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Graphics.OpenGL4;

namespace Windows_Engine
{
    public static class TextureLoader
    {
        public static int LoadTexture(string path)
        {
            using var image = Image.Load<Rgba32>(path);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba,
                image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            return tex;
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpriteFontPlus;
using ArcadeMaker.Core.Resources.Serializeables;

namespace ArcadeMaker.Engines.MonoGame.Core.Graphics
{
    internal static class Fonts
    {
        internal static Dictionary<GameFont, SpriteFont> All { get; } = [];
        internal static SpriteFont? Current { get; set; }
        internal static SpriteFont FromRaw(byte[] raw, float size, GraphicsDevice graphicsDevice)
        {
            // bake the font into a SpriteFont
            var fontBakeResult = TtfFontBaker.Bake(raw, size, 1024, 1024, [CharacterRange.BasicLatin]);
            return fontBakeResult.CreateSpriteFont(graphicsDevice);
        }

        internal static SpriteFont FromTTF(string ttfPath, float size, GraphicsDevice graphicsDevice) => FromRaw(File.ReadAllBytes(ttfPath), size, graphicsDevice);
        internal static SpriteFont FromStream(Stream stream, float size, GraphicsDevice graphicsDevice)
        {
            List<byte> raw = [];
            int next = stream.ReadByte();
            while (next >= 0)
            {
                raw.Add((byte)next);
                next = stream.ReadByte();
            }

            return FromRaw([.. raw], size, graphicsDevice);
        }

        internal static SpriteFont FromGameFont(string projectFilePath, GameFont gameFont, GraphicsDevice graphicsDevice)
        {
            return FromStream(SerializeableGameProject.OpenStream(projectFilePath, gameFont.ttf, false)!, gameFont.size, graphicsDevice);
        }

        internal static SpriteFont FromGameFont(Stream bundledProjectFilePath, GameFont gameFont, GraphicsDevice graphicsDevice)
        {
            return FromStream(SerializeableGameProject.OpenStream(bundledProjectFilePath, gameFont.ttf, false)!, gameFont.size, graphicsDevice);
        }
    }
}
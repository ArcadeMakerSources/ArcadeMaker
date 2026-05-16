using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RectpackSharp;
using Exp;
using ArcadeMaker.IDE.Items;

namespace ArcadeMaker.IDE.Textures;

internal static class TextureAtlas
{
    internal static Bitmap FromImages(ImageRect[] textures)
    {
        if (textures.Length == 0)
            return new Bitmap(1, 1);

        // pack
        var rects = textures.Map(r => r.Rect).ToArray();
        RectanglePacker.Pack(rects, out PackingRectangle bounds);

        // create empty atlas image
        var atlas = new Bitmap((int)bounds.Width, (int)bounds.Height);

        // fill atlas image
        using var graphics = Graphics.FromImage(atlas);
        int i = 0;
        foreach (var texture in textures)
        {
            texture.Rect = rects[i++];
            graphics.DrawImage(texture.Image, texture.Rect.X, texture.Rect.Y);
        }

        return atlas;
    }

    internal static Bitmap FromProjectSprites(GameProject project, out SpriteImageRect[] map)
    {
        List<SpriteImageRect> allImages = [];
        foreach (var sprite in project.items.OfType<GameSprite>())
        {
            int i = 0;
            allImages.AddRange(sprite.images.Map(_ => new SpriteImageRect(sprite, i++)));
        }

        map = allImages.ToArray();

        return FromImages(map);
    }
}

internal class ImageRect
{
    internal Bitmap Image { get; }
    internal PackingRectangle Rect { get; set; }
    internal ImageRect(Bitmap image)
    {
        this.Image = image;
        Rect = new()
        {
            Width = (uint)image.Width,
            Height = (uint)image.Height
        };
    }
}

internal class SpriteImageRect : ImageRect
{
    internal GameSprite Sprite { get; }
    internal int Index { get; }
    internal SpriteImageRect(GameSprite sprite, int index) : base(sprite.images[index])
    {
        this.Sprite = sprite;
        this.Index = index;
    }
}
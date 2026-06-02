using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Resources;

public class Sprite(string name, string? imageFile, int numOfImages, int originX, int originY, SpriteMask mask) : IItem
{
    public string Name => name;
    public string? ImageFile => imageFile;
    public int NumberOfImages => numOfImages;
    public int OriginX => originX;
    public int OriginY => originY;
    public SpriteMask Mask => mask;
}

public class SpriteMask(int top, int left, int right, int bottom)
{
    public int Top => top;
    public int Left => left;
    public int Right => right;
    public int Bottom => bottom;
}
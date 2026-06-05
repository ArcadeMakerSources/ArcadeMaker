using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc.Drawing;

/// <summary>
/// Represents a color in ABGR format, which is used by MonoGame's <c>Color.Color(uint)</c> constructor.
/// </summary>
[ExpEnum]
public enum Color : uint
{
    // --- Basics ---
    Red = 0xFF0000FF,  // R=FF G=00 B=00
    Green = 0xFF008000,  // R=00 G=80 B=00
    Blue = 0xFFFF0000,  // R=00 G=00 B=FF
    White = 0xFFFFFFFF,
    Black = 0xFF000000,
    Transparent = 0x00000000,

    // --- Yellows & Oranges ---
    Yellow = 0xFF00FFFF,  // R=FF G=FF B=00
    Gold = 0xFF00D7FF,  // R=FF G=D7 B=00
    Orange = 0xFF00A5FF,  // R=FF G=A5 B=00
    OrangeRed = 0xFF0045FF,  // R=FF G=45 B=00
    DarkOrange = 0xFF008CFF,  // R=FF G=8C B=00

    // --- Reds & Pinks ---
    Crimson = 0xFF3C14DC,  // R=DC G=3C B=14
    DarkRed = 0xFF00008B,  // R=8B G=00 B=00
    Tomato = 0xFF4763FF,  // R=FF G=63 B=47
    HotPink = 0xFFB469FF,  // R=FF G=69 B=B4
    DeepPink = 0xFF9314FF,  // R=FF G=14 B=93
    LightPink = 0xFFC1B6FF,  // R=FF G=B6 B=C1
    Pink = 0xFFCBC0FF,  // R=FF G=C0 B=CB
    MediumVioletRed = 0xFF8515C7,  // R=C7 G=15 B=85
    IndianRed = 0xFF5C5CCD,  // R=CD G=5C B=5C
    Salmon = 0xFF7280FA,  // R=FA G=80 B=72
    LightSalmon = 0xFF7AA0FF,  // R=FF G=A0 B=7A

    // --- Purples & Violets ---
    Purple = 0xFF800080,  // R=80 G=00 B=80
    Violet = 0xFFEE82EE,  // R=EE G=82 B=EE
    Magenta = 0xFFFF00FF,  // R=FF G=00 B=FF
    Fuchsia = 0xFFFF00FF,  // R=FF G=00 B=FF
    Orchid = 0xFFD670DA,  // R=DA G=70 B=D6
    Plum = 0xFFDDA0DD,  // R=DD G=A0 B=DD
    MediumPurple = 0xFFDB7093,  // R=93 G=70 B=DB
    BlueViolet = 0xFFE22B8A,  // R=8A G=2B B=E2
    DarkViolet = 0xFFD30094,  // R=94 G=00 B=D3
    Indigo = 0xFF82004B,  // R=4B G=00 B=82
    DarkMagenta = 0xFF8B008B,  // R=8B G=00 B=8B
    Lavender = 0xFFFAE6E6,  // R=E6 G=E6 B=FA
    MediumOrchid = 0xFFD355BA,  // R=BA G=55 B=D3
    Thistle = 0xFFD8BFD8,  // R=D8 G=BF B=D8

    // --- Blues ---
    Cyan = 0xFFFFFF00,  // R=00 G=FF B=FF
    Aqua = 0xFFFFFF00,  // R=00 G=FF B=FF
    SkyBlue = 0xFFEBCE87,  // R=87 G=CE B=EB
    DeepSkyBlue = 0xFFFFBF00,  // R=00 G=BF B=FF
    LightBlue = 0xFFE6D8AD,  // R=AD G=D8 B=E6
    CornflowerBlue = 0xFFED9564,  // R=64 G=95 B=ED
    RoyalBlue = 0xFFE16941,  // R=41 G=69 B=E1
    MediumBlue = 0xFFCD0000,  // R=00 G=00 B=CD
    DarkBlue = 0xFF8B0000,  // R=00 G=00 B=8B
    Navy = 0xFF800000,  // R=00 G=00 B=80
    MidnightBlue = 0xFF701919,  // R=19 G=19 B=70
    DodgerBlue = 0xFFFF901E,  // R=1E G=90 B=FF
    SteelBlue = 0xFFB48246,  // R=46 G=82 B=B4
    LightSteelBlue = 0xFFDEC4B0,  // R=B0 G=C4 B=DE
    SlateBlue = 0xFFCD5A6A,  // R=6A G=5A B=CD
    MediumSlateBlue = 0xFFEE687B,  // R=7B G=68 B=EE
    DarkSlateBlue = 0xFF8B3D48,  // R=48 G=3D B=8B
    PowderBlue = 0xFFE6E0B0,  // R=B0 G=E0 B=E6
    AliceBlue = 0xFFFFF8F0,  // R=F0 G=F8 B=FF
    LightCyan = 0xFFFFFFE0,  // R=E0 G=FF B=FF

    // --- Greens ---
    Lime = 0xFF00FF00,  // R=00 G=FF B=00
    LimeGreen = 0xFF32CD32,  // R=32 G=CD B=32
    ForestGreen = 0xFF228B22,  // R=22 G=8B B=22
    DarkGreen = 0xFF006400,  // R=00 G=64 B=00
    OliveGreen = 0xFF2F6B55,  // R=55 G=6B B=2F
    SeaGreen = 0xFF578B2E,  // R=2E G=8B B=57
    MediumSeaGreen = 0xFF71B33C,  // R=3C G=B3 B=71
    LightGreen = 0xFF90EE90,  // R=90 G=EE B=90
    PaleGreen = 0xFF98FB98,  // R=98 G=FB B=98
    SpringGreen = 0xFF7FFF00,  // R=00 G=FF B=7F
    MediumSpringGreen = 0xFF9AFA00, // R=00 G=FA B=9A
    Chartreuse = 0xFF00FF7F,  // R=7F G=FF B=00
    GreenYellow = 0xFF2FFFAD,  // R=AD G=FF B=2F
    YellowGreen = 0xFF32CD9A,  // R=9A G=CD B=32
    DarkOliveGreen = 0xFF2F6B55,  // R=55 G=6B B=2F
    Olive = 0xFF008080,  // R=80 G=80 B=00
    DarkSeaGreen = 0xFF8FBC8F,  // R=8F G=BC B=8F
    MediumAquamarine = 0xFFAACD66, // R=66 G=CD B=AA
    Aquamarine = 0xFFD4FF7F,  // R=7F G=FF B=D4
    Teal = 0xFF808000,  // R=00 G=80 B=80
    DarkCyan = 0xFF8B8B00,  // R=00 G=8B B=8B

    // --- Browns ---
    Brown = 0xFF2A2AA5,  // R=A5 G=2A B=2A
    SaddleBrown = 0xFF13458B,  // R=8B G=45 B=13
    Sienna = 0xFF2D52A0,  // R=A0 G=52 B=2D
    Chocolate = 0xFF1E69D2,  // R=D2 G=69 B=1E
    Peru = 0xFF3F85CD,  // R=CD G=85 B=3F
    Tan = 0xFF8CB4D2,  // R=D2 G=B4 B=8C
    BurlyWood = 0xFF87B8DE,  // R=DE G=B8 B=87
    Wheat = 0xFFB3DEF5,  // R=F5 G=DE B=B3
    Maroon = 0xFF000080,  // R=80 G=00 B=00
    RosyBrown = 0xFF8F8FBC,  // B=BC G=8F B=8F
    SandyBrown = 0xFF60A4F4,  // R=F4 G=A4 B=60

    // --- Grays ---
    Gainsboro = 0xFFDCDCDC,
    LightGray = 0xFFD3D3D3,
    Silver = 0xFFC0C0C0,
    DarkGray = 0xFFA9A9A9,
    Gray = 0xFF808080,
    DimGray = 0xFF696969,
    LightSlateGray = 0xFF998877,  // R=77 G=88 B=99
    SlateGray = 0xFF908070,  // R=70 G=80 B=90
    DarkSlateGray = 0xFF4F4F2F,  // R=2F G=4F B=4F

    // --- Whites & Near-Whites ---
    Snow = 0xFFFAFAFF,  // R=FF G=FA B=FA
    Ivory = 0xFFF0FFFF,  // R=FF G=FF B=F0
    Beige = 0xFFDCF5F5,  // R=F5 G=F5 B=DC
    Honeydew = 0xFFF0FFF0,  // R=F0 G=FF B=F0
    MintCream = 0xFFFAFFF5,  // R=F5 G=FF B=FA
    Azure = 0xFFFFFFF0,  // R=F0 G=FF B=FF
    FloralWhite = 0xFFF0FAFF,  // R=FF G=FA B=F0
    GhostWhite = 0xFFFFF8F8,  // R=F8 G=F8 B=FF
    WhiteSmoke = 0xFFF5F5F5,
    Linen = 0xFFE6F0FA,  // R=FA G=F0 B=E6
    OldLace = 0xFFE6F5FD,  // R=FD G=F5 B=E6
    Cornsilk = 0xFFDCF8FF,  // R=FF G=F8 B=DC
    BlanchedAlmond = 0xFFCDEBFF,  // R=FF G=EB B=CD
    Bisque = 0xFFC4E4FF,  // R=FF G=E4 B=C4
    PapayaWhip = 0xFFD5EFFF,  // R=FF G=EF B=D5
    PeachPuff = 0xFFB9DAFF,  // R=FF G=DA B=B9
    MistyRose = 0xFFE1E4FF,  // R=FF G=E4 B=E1
    LavenderBlush = 0xFFF5F0FF,  // R=FF G=F0 B=F5
    SeaShell = 0xFFEEF5FF,  // R=FF G=F5 B=EE
    AntiqueWhite = 0xFFD7EBFA,  // R=FA G=EB B=D7

    // --- Misc ---
    Coral = 0xFF507FFF,  // R=FF G=7F B=50
    DarkKhaki = 0xFF6BB7BD,  // R=BD G=B7 B=6B
    Khaki = 0xFF8CE6F0,  // R=F0 G=E6 B=8C
    Turquoise = 0xFFD0E040,  // R=40 G=E0 B=D0
    MediumTurquoise = 0xFFCCD148,  // R=48 G=D1 B=CC
    DarkTurquoise = 0xFFD1CE00,  // R=00 G=CE B=D1
    PaleTurquoise = 0xFFEEEEAF,  // R=AF G=EE B=EE
    CadetBlue = 0xFFA09E5F,  // R=5F G=9E B=A0
    LightSeaGreen = 0xFFAAB220,  // R=20 G=B2 B=AA
    DarkGoldenrod = 0xFF0B86B8,  // R=B8 G=86 B=0B
    Goldenrod = 0xFF20A5DA,  // R=DA G=A5 B=20
    PaleGoldenrod = 0xFFAAE8EE,  // R=EE G=E8 B=AA
    Moccasin = 0xFFB5E4FF,  // R=FF G=E4 B=B5
    NavajoWhite = 0xFFADDEFF,  // R=FF G=DE B=AD
}
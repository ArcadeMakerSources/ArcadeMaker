namespace ArcadeMaker.Core.ExpSrc.Controls;

[ExpEnum]
public enum Keys
{
    // ==========================================
    // MAIN CATEGORY
    // ==========================================

    //
    // Summary:
    //     ENTER key.
    Enter = 13,
    //
    // Summary:
    //     SPACEBAR key.
    Space = 32,
    //
    // Summary:
    //     LEFT ARROW key.
    [SeparatorValue]
    Left = 37,
    //
    // Summary:
    //     UP ARROW key.
    Up = 38,
    //
    // Summary:
    //     RIGHT ARROW key.
    Right = 39,
    //
    // Summary:
    //     DOWN ARROW key.
    Down = 40,
    //
    // Summary:
    //     Left SHIFT key.
    [SeparatorValue]
    LeftShift = 160,
    //
    // Summary:
    //     Right SHIFT key.
    RightShift = 161,

    // ==========================================
    // NUMBERS CATEGORY
    // ==========================================

    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D0 = 48,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D1 = 49,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D2 = 50,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D3 = 51,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D4 = 52,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D5 = 53,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D6 = 54,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D7 = 55,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D8 = 56,
    //
    // Summary:
    //     Used for miscellaneous characters; it can vary by keyboard.
    [Category(KeyboardCategories.Digits)]
    D9 = 57,
    //
    // Summary:
    //     Numeric keypad 0 key.
    [Category(KeyboardCategories.Digits)]
    NumPad0 = 96,
    //
    // Summary:
    //     Numeric keypad 1 key.
    [Category(KeyboardCategories.Digits)]
    NumPad1 = 97,
    //
    // Summary:
    //     Numeric keypad 2 key.
    [Category(KeyboardCategories.Digits)]
    NumPad2 = 98,
    //
    // Summary:
    //     Numeric keypad 3 key.
    [Category(KeyboardCategories.Digits)]
    NumPad3 = 99,
    //
    // Summary:
    //     Numeric keypad 4 key.
    [Category(KeyboardCategories.Digits)]
    NumPad4 = 100,
    //
    // Summary:
    //     Numeric keypad 5 key.
    [Category(KeyboardCategories.Digits)]
    NumPad5 = 101,
    //
    // Summary:
    //     Numeric keypad 6 key.
    [Category(KeyboardCategories.Digits)]
    NumPad6 = 102,
    //
    // Summary:
    //     Numeric keypad 7 key.
    [Category(KeyboardCategories.Digits)]
    NumPad7 = 103,
    //
    // Summary:
    //     Numeric keypad 8 key.
    [Category(KeyboardCategories.Digits)]
    NumPad8 = 104,
    //
    // Summary:
    //     Numeric keypad 9 key.
    [Category(KeyboardCategories.Digits)]
    NumPad9 = 105,

    // ==========================================
    // LETTERS CATEGORY
    // ==========================================

    //
    // Summary:
    //     A key.
    [Category(KeyboardCategories.Letters)]
    A = 65,
    //
    // Summary:
    //     B key.
    [Category(KeyboardCategories.Letters)]
    B = 66,
    //
    // Summary:
    //     C key.
    [Category(KeyboardCategories.Letters)]
    C = 67,
    //
    // Summary:
    //     D key.
    [Category(KeyboardCategories.Letters)]
    D = 68,
    //
    // Summary:
    //     E key.
    [Category(KeyboardCategories.Letters)]
    E = 69,
    //
    // Summary:
    //     F key.
    [Category(KeyboardCategories.Letters)]
    F = 70,
    //
    // Summary:
    //     G key.
    [Category(KeyboardCategories.Letters)]
    G = 71,
    //
    // Summary:
    //     H key.
    [Category(KeyboardCategories.Letters)]
    H = 72,
    //
    // Summary:
    //     I key.
    [Category(KeyboardCategories.Letters)]
    I = 73,
    //
    // Summary:
    //     J key.
    [Category(KeyboardCategories.Letters)]
    J = 74,
    //
    // Summary:
    //     K key.
    [Category(KeyboardCategories.Letters)]
    K = 75,
    //
    // Summary:
    //     L key.
    [Category(KeyboardCategories.Letters)]
    L = 76,
    //
    // Summary:
    //     M key.
    [Category(KeyboardCategories.Letters)]
    M = 77,
    //
    // Summary:
    //     N key.
    [Category(KeyboardCategories.Letters)]
    N = 78,
    //
    // Summary:
    //     O key.
    [Category(KeyboardCategories.Letters)]
    O = 79,
    //
    // Summary:
    //     P key.
    [Category(KeyboardCategories.Letters)]
    P = 80,
    //
    // Summary:
    //     Q key.
    [Category(KeyboardCategories.Letters)]
    Q = 81,
    //
    // Summary:
    //     R key.
    [Category(KeyboardCategories.Letters)]
    R = 82,
    //
    // Summary:
    //     S key.
    [Category(KeyboardCategories.Letters)]
    S = 83,
    //
    // Summary:
    //     T key.
    [Category(KeyboardCategories.Letters)]
    T = 84,
    //
    // Summary:
    //     U key.
    [Category(KeyboardCategories.Letters)]
    U = 85,
    //
    // Summary:
    //     V key.
    [Category(KeyboardCategories.Letters)]
    V = 86,
    //
    // Summary:
    //     W key.
    [Category(KeyboardCategories.Letters)]
    W = 87,
    //
    // Summary:
    //     X key.
    [Category(KeyboardCategories.Letters)]
    X = 88,
    //
    // Summary:
    //     Y key.
    [Category(KeyboardCategories.Letters)]
    Y = 89,
    //
    // Summary:
    //     Z key.
    [Category(KeyboardCategories.Letters)]
    Z = 90,

    // ==========================================
    // FUNCTION KEYS CATEGORY
    // ==========================================

    //
    // Summary:
    //     F1 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F1 = 112,
    //
    // Summary:
    //     F2 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F2 = 113,
    //
    // Summary:
    //     F3 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F3 = 114,
    //
    // Summary:
    //     F4 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F4 = 115,
    //
    // Summary:
    //     F5 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F5 = 116,
    //
    // Summary:
    //     F6 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F6 = 117,
    //
    // Summary:
    //     F7 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F7 = 118,
    //
    // Summary:
    //     F8 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F8 = 119,
    //
    // Summary:
    //     F9 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F9 = 120,
    //
    // Summary:
    //     F10 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F10 = 121,
    //
    // Summary:
    //     F11 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F11 = 122,
    //
    // Summary:
    //     F12 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F12 = 123,
    //
    // Summary:
    //     F13 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F13 = 124,
    //
    // Summary:
    //     F14 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F14 = 125,
    //
    // Summary:
    //     F15 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F15 = 126,
    //
    // Summary:
    //     F16 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F16 = 127,
    //
    // Summary:
    //     F17 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F17 = 128,
    //
    // Summary:
    //     F18 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F18 = 129,
    //
    // Summary:
    //     F19 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F19 = 130,
    //
    // Summary:
    //     F20 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F20 = 131,
    //
    // Summary:
    //     F21 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F21 = 132,
    //
    // Summary:
    //     F22 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F22 = 133,
    //
    // Summary:
    //     F23 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F23 = 134,
    //
    // Summary:
    //     F24 key.
    [Category(KeyboardCategories.FunctionKeys)]
    F24 = 135,

    // ==========================================
    // OTHERS CATEGORY
    // ==========================================

    //
    // Summary:
    //     Reserved.
    [Category(KeyboardCategories.Others)]
    None = 0,
    //
    // Summary:
    //     BACKSPACE key.
    [Category(KeyboardCategories.Others)]
    Back = 8,
    //
    // Summary:
    //     TAB key.
    [Category(KeyboardCategories.Others)]
    Tab = 9,
    //
    // Summary:
    //     CAPS LOCK key.
    [Category(KeyboardCategories.Others)]
    CapsLock = 20,
    //
    // Summary:
    //     ESC key.
    [Category(KeyboardCategories.Others)]
    Escape = 27,
    //
    // Summary:
    //     PAGE UP key.
    [Category(KeyboardCategories.Others)]
    PageUp = 33,
    //
    // Summary:
    //     PAGE DOWN key.
    [Category(KeyboardCategories.Others)]
    PageDown = 34,
    //
    // Summary:
    //     END key.
    [Category(KeyboardCategories.Others)]
    End = 35,
    //
    // Summary:
    //     HOME key.
    [Category(KeyboardCategories.Others)]
    Home = 36,
    //
    // Summary:
    //     SELECT key.
    [Category(KeyboardCategories.Others)]

    Select = 41,
    //
    // Summary:
    // PRINT key.
    [Category(KeyboardCategories.Others)]
    Print = 42,
    //
    // Summary:
    // EXECUTE key.
    [Category(KeyboardCategories.Others)]
    Execute = 43,
    //
    // Summary:
    // PRINT SCREEN key.
    [Category(KeyboardCategories.Others)]
    PrintScreen = 44,
    //
    // Summary:
    // INS key.
    [Category(KeyboardCategories.Others)]
    Insert = 45,
    //
    // Summary:
    // DEL key.
    [Category(KeyboardCategories.Others)]
    Delete = 46,
    //
    // Summary:
    // HELP key.
    [Category(KeyboardCategories.Others)]
    Help = 47,
    //
    // Summary:
    // Left Windows key.
    [Category(KeyboardCategories.Others)]
    LeftWindows = 91,
    //
    // Summary:
    // Right Windows key.
    [Category(KeyboardCategories.Others)]
    RightWindows = 92,
    //
    // Summary:
    // Applications key.
    [Category(KeyboardCategories.Others)]
    Apps = 93,
    //
    // Summary:
    // Computer Sleep key.
    [Category(KeyboardCategories.Others)]
    Sleep = 95,
    //
    // Summary:
    // Multiply key.
    [Category(KeyboardCategories.Others)]
    Multiply = 106,
    //
    // Summary:
    // Add key.
    [Category(KeyboardCategories.Others)]
    Add = 107,
    //
    // Summary:
    // Separator key.
    [Category(KeyboardCategories.Others)]
    Separator = 108,
    //
    // Summary:
    // Subtract key.
    [Category(KeyboardCategories.Others)]
    Subtract = 109,
    //
    // Summary:
    // Decimal key.
    [Category(KeyboardCategories.Others)]
    Decimal = 110,
    //
    // Summary:
    // Divide key.
    [Category(KeyboardCategories.Others)]
    Divide = 111,
    //
    // Summary:
    // NUM LOCK key.
    [Category(KeyboardCategories.Others)]
    NumLock = 144,
    //
    // Summary:
    // SCROLL LOCK key.
    [Category(KeyboardCategories.Others)]
    Scroll = 145,
    //
    // Summary:
    // Left CONTROL key.
    [Category(KeyboardCategories.Others)]
    LeftControl = 162,
    //
    // Summary:
    // Right CONTROL key.
    [Category(KeyboardCategories.Others)]
    RightControl = 163,
    //
    // Summary:
    // Left ALT key.
    [Category(KeyboardCategories.Others)]
    LeftAlt = 164,
    //
    // Summary:
    // Right ALT key.
    [Category(KeyboardCategories.Others)]
    RightAlt = 165,
    //
    // Summary:
    // Browser Back key.
    [Category(KeyboardCategories.Others)]
    BrowserBack = 166,
    //
    // Summary:
    // Browser Forward key.
    [Category(KeyboardCategories.Others)]
    BrowserForward = 167,
    //
    // Summary:
    // Browser Refresh key.
    [Category(KeyboardCategories.Others)]
    BrowserRefresh = 168,
    //
    // Summary:
    // Browser Stop key.
    [Category(KeyboardCategories.Others)]
    BrowserStop = 169,
    //
    // Summary:
    // Browser Search key.
    [Category(KeyboardCategories.Others)]
    BrowserSearch = 170,
    //
    // Summary:
    // Browser Favorites key.
    [Category(KeyboardCategories.Others)]
    BrowserFavorites = 171,
    //
    // Summary:
    // Browser Start and Home key.
    [Category(KeyboardCategories.Others)]
    BrowserHome = 172,
    //
    // Summary:
    // Volume Mute key.
    [Category(KeyboardCategories.Others)]
    VolumeMute = 173,
    //
    // Summary:
    // Volume Down key.
    [Category(KeyboardCategories.Others)]
    VolumeDown = 174,
    //
    // Summary:
    //     Volume Up key.
    [Category(KeyboardCategories.Others)]
    VolumeUp = 175,
    //
    // Summary:
    //     Next Track key.
    [Category(KeyboardCategories.Others)]
    MediaNextTrack = 176,
    //
    // Summary:
    //     Previous Track key.
    [Category(KeyboardCategories.Others)]
    MediaPreviousTrack = 177,
    //
    // Summary:
    //     Stop Media key.
    [Category(KeyboardCategories.Others)]
    MediaStop = 178,
    //
    // Summary:
    //     Play/Pause Media key.
    [Category(KeyboardCategories.Others)]
    MediaPlayPause = 179,
    //
    // Summary:
    //     Launch Mail key.
    [Category(KeyboardCategories.Others)]
    LaunchMail = 180,
    //
    // Summary:
    //     Select Media key.
    [Category(KeyboardCategories.Others)]
    SelectMedia = 181,
    //
    // Summary:
    //     Launch Application 1 key.
    [Category(KeyboardCategories.Others)]
    LaunchApplication1 = 182,
    //
    // Summary:
    //     Launch Application 2 key.
    [Category(KeyboardCategories.Others)]
    LaunchApplication2 = 183
}
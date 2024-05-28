﻿namespace LocalUtilities.TypeGeneral;

[Flags]
public enum Directions
{
    None = 0x0,
    Left = 0x1,
    Top = 0x2,
    Right = 0x4,
    Bottom = 0x8,
    LeftTop = Left | Top,
    TopRight = Top | Right,
    LeftBottom = Left | Bottom,
    BottomRight = Bottom | Right,
    Center = 0x16,
}
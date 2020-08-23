#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;

#endregion

namespace Osu_BackgroundPurge.OsuParser {
    [Flags] public enum OsuHitObjectType { HitCircle = 0, Slider = 1, NewCombo = 2, Spinner = 3, SkipOneCombo = 4, SkipTwoCombos = 5, SkipThreeCombos = 6, OsuManiaHold = 7 }
}
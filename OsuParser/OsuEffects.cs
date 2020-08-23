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
    /// <summary> Timing points have two extra effects that can be toggled using bits 0 and 3 (from least to most significant) in the effects integer:
    /// 0: Whether or not kiai time is enabled
    /// 3: Whether or not the first barline is omitted in osu!taiko and osu!mania
    /// The rest of the bits are unused.
    /// </summary>
    [Flags] public enum OsuEffects { KiaiTime = 0, _ = 1, __ = 2, BarlineOmission = 3 }
}